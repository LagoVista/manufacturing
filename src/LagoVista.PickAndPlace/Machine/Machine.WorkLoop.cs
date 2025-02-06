using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Commands;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public partial class Machine
    {
        DateTime _connectTime;

        Queue<string> _sentQueue = new Queue<string>();
        Queue<string> _toSend = new Queue<string>();
        Queue<string> _toSendPriority = new Queue<string>();

        StreamReader _reader;
        StreamWriter _writer;

        DateTime _lastPollTime;
        TimeSpan _waitTime;

        private void SendHighPriorityItems()
        {
            while (_toSendPriority.Count > 0)
            {
                var line = _toSendPriority.Dequeue();

                _writer.Write(line);
                if (line != "?" && line != "M114")
                {
                    AddStatusMessage(StatusMessageTypes.SentLinePriority, line.TrimStart().TrimEnd());
                }

                _writer.Flush();
            }
        }
        private void SendNormalPriorityItems()
        {
            var send_line = _toSend.Peek();
            var zAxisMoveRegEx = new Regex("G([01])\\s+Z([RL])([\\d.]+)");
            var match = zAxisMoveRegEx.Match(send_line);
            if (match.Success)
            {
                var moveType = match.Groups[1].Value;
                var zaxis = match.Groups[2];
                var distance = Convert.ToDouble(match.Groups[3].Value);
                if (zaxis.Value == "R")
                {
                    distance = (31.5 - distance) + 31.5;
                }

                send_line = $"G{moveType} Z{distance}";
            }


            UnacknowledgedBytesSent += send_line.Length + 1;

            if (Settings.MachineType == FirmwareTypes.Repeteir_PnP &&
                (send_line == "M400" || send_line == "G28"))
            {
                _isOnHold = true;
            }

            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            if (send_line != "M114")
            {
                UpdateStatus(send_line.ToString());
                AddStatusMessage(StatusMessageTypes.SentLine, send_line.ToString());
            }

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();
        }

        private void TransmitJobItem(GCodeCommand cmd)
        {
            var trimmedLine = cmd.Line.Trim('\r', '\n');

            if (Settings.MachineType == FirmwareTypes.Repeteir_PnP &&
                (trimmedLine == "M400" || trimmedLine == "G28"))
            {
                _isOnHold = true;
            }

            UnacknowledgedBytesSent += trimmedLine.Length + 1;

            /* Make sure we normalize the line ending so it's only \n */
            _writer.Write(trimmedLine);
            _writer.Write('\n');
            _writer.Flush();

            cmd.Status = GCodeCommand.StatusTypes.Sent;

            UpdateStatus(cmd.Line.ToString());
            AddStatusMessage(StatusMessageTypes.SentLine, cmd.Line.ToString());

            _sentQueue.Enqueue(cmd.Line);
        }

        private async Task QueryStatus()
        {
            var Now = DateTime.Now;

            if (Mode == OperatingMode.Manual)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalIdle && LocationUpdateEnabled)
                {
                    if (Settings.MachineType == FirmwareTypes.GRBL1_1 ||
                        Settings.MachineType == FirmwareTypes.LagoVista ||
                        Settings.MachineType == FirmwareTypes.LagoVista_PnP)
                    {
                        Enqueue("?", true);
                    }
                    else
                    {
                        Enqueue("M114");
                    }

                    _lastPollTime = Now;
                }
            }
            else if (Mode == OperatingMode.SendingGCodeFile)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalRunning)
                {
                    if (Settings.CurrentSerialPort.Name == "Simulated")
                        MachinePosition = GCodeFileManager.CurrentCommand.CurrentPosition;
                    else
                    {
                        if (Settings.MachineType == FirmwareTypes.GRBL1_1 && LocationUpdateEnabled)
                        {
                            Enqueue("?", true);
                        }
                        else
                        {
                            Enqueue("M114");
                        }

                    }

                    _lastPollTime = Now;
                }
            }
            else if (Mode == OperatingMode.ProbingHeightMap)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalRunning)
                {
                    if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                    {
                        Enqueue("?", true);
                    }
                    else
                    {
                        Enqueue("M114");
                    }

                    _lastPollTime = Now;
                }
            }

            await Task.Delay(_waitTime);
        }

        private bool ShouldSendNormalPriorityItems()
        {
            if (_toSend.Count == 0)
                return false;

            if(_toSend.Count > 0 && _toSend.Peek() != null)
                return _toSend.Count > 0 && ((_toSend.Peek()?.ToString()).Length + 1) < (Settings.ControllerBufferSize - Math.Max(0, UnacknowledgedBytesSent));
            return true;
        }

        private async Task Send()
        {
            SendHighPriorityItems();

            if (!_isOnHold)
            {
                if (Mode == OperatingMode.SendingGCodeFile &&
                    _toSend.Count == 0 &&
                    Settings.ControllerBufferSize - Math.Max(0, UnacknowledgedBytesSent) > 24)
                {
                    var nextCommand = GCodeFileManager.GetNextJobItem();
                    if (nextCommand != null)
                        TransmitJobItem(nextCommand);
                }
                else if (ShouldSendNormalPriorityItems())
                {
                    SendNormalPriorityItems();
                }
                else
                {
                    await QueryStatus();
                }
            }
        }

        private async Task WorkLoop()
        {
            if (_reader.BaseStream.CanRead == false)
            {
                return;
            }

            await Task.Delay(5);
             
            var lineTask = Settings.ConnectionType == ConnectionTypes.Serial_Port ? _reader.ReadToEndAsync() : _reader.ReadLineAsync(); 

            /* While we are awaiting for a line to come in process any outgoing stuff */
            while (!lineTask.IsCompleted)
            {
                if (!Connected)
                {
                    return;
                }

                await Send();
            }

            if (lineTask.IsCompleted)
            {
                if (lineTask.Status != TaskStatus.Faulted)
                {
                    LineReceived?.Invoke(this, lineTask.Result);
                    ProcessResponseLine(lineTask.Result);
                }
            }
        }

        private async void Work(Stream inputStream, Stream outputStream)
        {
            try
            {
                _waitTime = TimeSpan.FromMilliseconds(0.5);
                _lastPollTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
                _connectTime = DateTime.Now;

                _reader = new StreamReader(inputStream);
                _writer = new StreamWriter(outputStream);

                UnacknowledgedBytesSent = 0;

                if (Settings.MachineType == FirmwareTypes.GRBL1_1)
                {
                    Enqueue("\n$G\n", true);
                }
                else if (Settings.MachineType == FirmwareTypes.LagoVista_PnP)
                {
                    Enqueue("*", true);
                }

                while (Connected)
                {
                    try
                    {
                        await WorkLoop();
                    }catch(Exception ex)
                    {
                        AddStatusMessage(StatusMessageTypes.FatalError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                AddStatusMessage(StatusMessageTypes.FatalError, $"Fatal Error: {ex.Message}");
                await DisconnectAsync();
                MachineDisconnected?.Invoke(this, null);
            }
        }
    }
}
