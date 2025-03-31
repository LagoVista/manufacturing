using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        private StringBuilder _messageBuffer = new StringBuilder();

        private bool _isOnHold = false;

        IGCodeCommandHandler _gcodeCommandHandler;

        public void RegisterGCodeFileCommandHandler(IGCodeCommandHandler commandHandler)
        {
            _gcodeCommandHandler = commandHandler;
        }

        private void ParseMessage(string fullMessageLine)
        {
            fullMessageLine = fullMessageLine.ToLower();
            if (fullMessageLine.StartsWith("ok") ||
                            fullMessageLine.StartsWith("<ok:"))
            {
                if (_gcodeCommandHandler.HasValidFile && Mode == OperatingMode.SendingGCodeFile)
                {
                    lock (this)
                    {
                        _gcodeCommandHandler.CommandAcknowledged();

                        lock (_queueAccessLocker)
                        {
                            if (_sentQueue.Any())
                            {
                                var sentLine = _sentQueue.Dequeue();
                                UnacknowledgedBytesSent -= (sentLine.Length + 1);
                            }
                        }

                        if (_gcodeCommandHandler.IsCompleted)
                        {
                            Mode = OperatingMode.Manual;
                        }
                    }
                }                
                else
                {
                    lock (_queueAccessLocker)
                    {
                        if (PendingQueue.Count > 0)
                        {
                            if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP || Settings.FirmwareType == FirmwareTypes.LumenPnP_V4_Marlin)
                            {
                                Services.DispatcherServices.Invoke(() =>
                                {
                                    PendingQueue.RemoveAt(0);
                                });
                            }
                            else
                            {
                                var responseRegEx = new Regex("<ok:(?'CODE'[A-Za-z0-9]+).*>");
                                var responseGCode = responseRegEx.Match(fullMessageLine);
                                if (responseGCode.Success)
                                {
                                    var code = responseGCode.Groups["CODE"].Value;

                                    if (PendingQueue[0].StartsWith(code.ToUpper()))
                                    {
                                        Debug.WriteLine($"MATCH =D -> TOP ITEM {PendingQueue[0]} - {code} ");

                                        Services.DispatcherServices.Invoke(() =>
                                        {
                                            PendingQueue.RemoveAt(0);
                                        });
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"MISMATCH :( -> TOP ITEM {PendingQueue[0]} - {code} ");
                                    }
                                }
                            }
                        }

                        if (_sentQueue.Any())
                        {
                            var sentLine = _sentQueue.Dequeue();
                            UnacknowledgedBytesSent -= (sentLine.Length + 1);
                            return;
                        }
                    }

                    LagoVista.Core.PlatformSupport.Services.Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Warning, "Machine_Work", "Received OK without anything in the Sent Buffer");
                    UnacknowledgedBytesSent = 0;
                }
            }
            else if (fullMessageLine != null)
            {
                if (fullMessageLine == "wait")
                {
                    if(_isOnHold)
                    {
                        Debug.WriteLine("RECOVERREDDD!");
                    }

                    _isOnHold = false;
                }
                else if (fullMessageLine.StartsWith("alarm:"))
                {
                    var alarmNumberRegEx = new Regex("alarm:(?'ErrorCode'-?[0-9\\.]*)?");
                    var alarmMatch = alarmNumberRegEx.Match(fullMessageLine);

                    if(alarmMatch.Success)
                    {
                        var strErrorCode = alarmMatch.Groups["ErrorCode"].Value;                        
                        var err = GrblErrorProvider.Instance.GetAlarmCode(Convert.ToInt32(strErrorCode));
                        AddStatusMessage(StatusMessageTypes.Warning, err, MessageVerbosityLevels.Normal);
                    }
                }
                else if (fullMessageLine.StartsWith("error:"))
                {
                    var errorline = _sentQueue.Any() ? _sentQueue.Dequeue() : "?????";


                    var errNumbRegEx = new Regex("error:(?'ErrorCode'-?[0-9\\.]*)?");

                    var errMatch = errNumbRegEx.Match(fullMessageLine);
                    if (errMatch.Success)
                    {
                        var strErrorCode = errMatch.Groups["ErrorCode"].Value;
                        var err = GrblErrorProvider.Instance.GetErrorMessage(Convert.ToInt32(strErrorCode));
                        AddStatusMessage(StatusMessageTypes.Warning, err, MessageVerbosityLevels.Normal);
                    }
                    else
                    {
                        AddStatusMessage(StatusMessageTypes.Warning, $"{fullMessageLine}: {errorline}", MessageVerbosityLevels.Normal);

                        if (_sentQueue.Count != 0)
                        {
                            var sentLine = _sentQueue.Dequeue();
                            UnacknowledgedBytesSent -= sentLine.Length + 1;
                        }
                        else
                        {
                            if ((DateTime.Now - _connectTime).TotalMilliseconds > 200)
                            {
                                AddStatusMessage(StatusMessageTypes.Warning, $"Received <{fullMessageLine}> without anything in the Sent Buffer", MessageVerbosityLevels.Normal);
                            }

                            UnacknowledgedBytesSent = 0;
                        }
                    }

                    Mode = OperatingMode.Manual;
                }
                else if (fullMessageLine.StartsWith("<"))
                {
                    if (Settings.FirmwareType == FirmwareTypes.LagoVista || Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
                    {
                        if (ParseLagoVistaLine(fullMessageLine))
                        {
                            AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine, MessageVerbosityLevels.Diagnostics);
                        }
                    }
                    else if (ParseStatus(fullMessageLine))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine, MessageVerbosityLevels.Diagnostics);
                    }
                    else if (ParseLine(fullMessageLine))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine, MessageVerbosityLevels.Diagnostics);
                    }
                    else
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine);
                    }
                }
                else if (fullMessageLine.StartsWith("[prb:"))
                {
                    var probeResult = ParseProbeLine(fullMessageLine);
                    if (probeResult != null)
                    {
                        if(_probeCompletedHandler != null)
                            _probeCompletedHandler.ProbeCompleted(probeResult.Value);
                        else
                            AddStatusMessage(StatusMessageTypes.Warning, "Unexpected PRB return message.");
                    }
                }
                else if (fullMessageLine.StartsWith("["))
                {
                    UpdateStatus(fullMessageLine);

                    AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine);
                }
                else if (fullMessageLine.StartsWith("ALARM"))
                {
                    AddStatusMessage(StatusMessageTypes.FatalError, fullMessageLine);
                    Mode = OperatingMode.Manual;
                }
                else if (fullMessageLine.Length > 0)
                {
                    if (!ParseLine(fullMessageLine))
                    {
                        AddStatusMessage(StatusMessageTypes.ReceivedLine, fullMessageLine);
                    }

                    /*
                     * Not sure why we would dequeue if we couldn't parse the line...may need to revisit *
                    lock (_queueAccessLocker)
                    {
                        if (_sentQueue.Any())
                        {
                            var sentLine = _sentQueue.Dequeue();
                            UnacknowledgedBytesSent -= (sentLine.FeederLength + 1);
                        }
                    }*/
                }
            }
            else
            {
                AddStatusMessage(StatusMessageTypes.Warning, $"Empty Response From Machine.", MessageVerbosityLevels.Normal);
            }
        }

        public bool RegisterProbeCompletedHandler(IProbeCompletedHandler handler)
        {
            if (_probeCompletedHandler == null)
            {
                _probeCompletedHandler = handler;
                return true;
            }
            else
            {
                AddStatusMessage(StatusMessageTypes.Warning, $"Can not register probe handler, one already exists {_probeCompletedHandler.GetType().Name}");
                return false;
            }
        }

        public bool UnregisterProbeCompletedHandler()
        {
            if(_probeCompletedHandler == null)
            {
                AddStatusMessage(StatusMessageTypes.Warning, $"Attempt to unregister probe handler but none has been registered.");
            }

            _probeCompletedHandler = null;
            return true;
        }

        IProbeCompletedHandler _probeCompletedHandler;

        private static Regex ProbeEx = new Regex(@"\[prb:(?'mx'-?[0-9]+\.?[0-9]*),(?'my'-?[0-9]+\.?[0-9]*),(?'mz'-?[0-9]+\.?[0-9]*):(?'success'0|1)\]");

        /// <summary>
        /// Parses a recevied probe report
        /// </summary>
        Vector3? ParseProbeLine(string line)
        {
            Match probeMatch = ProbeEx.Match(line);
            Group mx = probeMatch.Groups["mx"];
            Group my = probeMatch.Groups["my"];
            Group mz = probeMatch.Groups["mz"];
            Group success = probeMatch.Groups["success"];

            if (!probeMatch.Success || !(mx.Success & my.Success & mz.Success & success.Success))
            {
                AddStatusMessage(StatusMessageTypes.Warning, $"Received Bad Probe: '{line}'");
                return null;
            }

            var probePos = new Vector3(double.Parse(mx.Value, Constants.DecimalParseFormat), double.Parse(my.Value, Constants.DecimalParseFormat), double.Parse(mz.Value, Constants.DecimalParseFormat));

            probePos += WorkspacePosition - MachinePosition;     //Mpos, Wpos only get updated by the same dispatcher, so this should be thread safe
            return probePos;
        }

        private void ProcessResponseLine(String line)
        {

            if (String.IsNullOrEmpty(line))
            {
                return;
            }

            ParseMessage(line);

            /*            foreach (var ch in line.ToCharArray())
                        {
                            if (ch == '\r')
                            {
                                ParseMessage(_messageBuffer.ToString());
                                _messageBuffer.Clear();
                            }
                            else
                            {
                                _messageBuffer.Append(ch);
                            }
                        }*/
        }
    }
}
