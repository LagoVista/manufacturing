﻿using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Commands;
using LagoVista.IoT.DeviceMessaging.Models.Cot;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Util;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                if (line != null)
                {
                    _writer.Write(line);
                    if(line != "?" && line != "M114")
                    {
                        AddStatusMessage(StatusMessageTypes.SentLinePriority, line.TrimStart().TrimEnd());
                    }
                }

                _writer.Flush();
            }
        }
        private void SendNormalPriorityItems()
        {
            var send_line = _toSend.Peek();
            if(String.IsNullOrEmpty(send_line))
            {
                Debug.WriteLine("EMPTY SEND LINE!");
                _toSend.Dequeue();
            }

            var zAxisMoveRegEx = new Regex("G([01])\\s+Z([RL])([\\d.]+)");
            var match = zAxisMoveRegEx.Match(send_line);
            if (match.Success)
            {
                var moveType = match.Groups[1].Value;
                var zaxis = match.Groups[2];
                var distance = Convert.ToDouble(match.Groups[3].Value);
                if (zaxis.Value == "R")
                {
                    //distance = (31.5 - distance) + 31.5;
                    distance = (97 - distance);
                }

                send_line = $"G{moveType} Z{distance}";
            }


            UnacknowledgedBytesSent += send_line.Length + 1;

            if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP &&
                (send_line == "M400" || send_line == "G28"))
            {
                _isOnHold = true;
            }

            _writer.Write(send_line);
            _writer.Write('\n');
            _writer.Flush();

            DebugWriteLine($"    [SEND] {send_line}");

            if (send_line != "M114")
            {
                UpdateStatus(send_line.ToString());
                AddStatusMessage(StatusMessageTypes.SentLine, send_line.ToString());
            }

            _sentQueue.Enqueue(send_line);
            _toSend.Dequeue();

            if(send_line != "M114")
                AddSentMessage(StatusMessageTypes.Info, send_line);
        }

        private void TransmitJobItem(GCodeCommand cmd)
        {
            var trimmedLine = cmd.Line.Trim('\r', '\n');

            if (Settings.FirmwareType == FirmwareTypes.Repeteir_PnP &&
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

        private void SendQueryStatus()
        {
            if (_spinningWhileBusy)
                return;

            var Now = DateTime.Now;

            if (Mode == OperatingMode.Manual)
            {
                if ((Now - _lastPollTime).TotalMilliseconds > Settings.StatusPollIntervalIdle && LocationUpdateEnabled)
                {
                    if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 ||
                        Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom ||
                        Settings.FirmwareType == FirmwareTypes.LagoVista ||
                        Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
                    {
                        if (!_internalPendingQueue.Contains("?"))
                            Enqueue("?", true);
                    }
                    else
                    {
                        if(!_internalPendingQueue.Contains("M114"))
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
                        MachinePosition = _gcodeCommandHandler.CurrentCommand.CurrentPosition;
                    else
                    {
                        if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 && LocationUpdateEnabled || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
                        {
                            if (!_internalPendingQueue.Contains("?"))
                                Enqueue("?", true);
                        }
                        else
                        {
                            if (!_internalPendingQueue.Contains("M114"))
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
                    if (Settings.FirmwareType == FirmwareTypes.GRBL1_1 || Settings.FirmwareType == FirmwareTypes.GRBL1_1_SL_Custom)
                    {
                        if (!_internalPendingQueue.Contains("?"))
                            Enqueue("?", true);
                    }
                    else
                    {
                        if (!_internalPendingQueue.Contains("M114"))
                            Enqueue("M114");
                    }

                    _lastPollTime = Now;
                }
            }
        }

   

        private bool ShouldSendNormalPriorityItems()
        {
            lock (_toSend)
            {
                if (_toSend.Count == 0)
                    return false;

                if (_toSend.Count > 0 && _toSend.Peek() != null)
                    return _toSend.Count > 0 && ((_toSend.Peek()?.ToString()).Length + 1) < (Settings.ControllerBufferSize - Math.Max(0, UnacknowledgedBytesSent));
                return true;
            }
        }

        DateTime _nextSend = DateTime.MinValue;

        private void Send()
        {
            SendHighPriorityItems();

            if (!_isOnHold)
            {

                lock (_toSend)
                {
                    if (Mode == OperatingMode.SendingGCodeFile &&
                        _toSend.Count == 0 &&
                        Settings.ControllerBufferSize - Math.Max(0, UnacknowledgedBytesSent) > 24)
                    {
                        var nextCommand = _gcodeCommandHandler.GetNextJobItem();
                        if (nextCommand != null)
                            TransmitJobItem(nextCommand);
                    }
                    else if (ShouldSendNormalPriorityItems())
                    {
                        SendNormalPriorityItems();
                    }                    
                }


                if (Mode != OperatingMode.PlacingParts && _nextSend < DateTime.Now)
                {
                    SendQueryStatus();
                    _nextSend = _nextSend.Add(_waitTime);
                }
            }
        }

        String messageBuffer = String.Empty;

        private void WorkLoop()
        {
            if (_reader.BaseStream.CanRead == false)
            {
                return;
            }


            if(Settings.ConnectionType == ConnectionTypes.Serial_Port)
            {
                var buffer = new char[1024];
                var lineTask = _reader.ReadAsync(buffer, 0, 1024);
                while (!lineTask.IsCompleted)
                {
                    if (!Connected)
                    {
                        return;
                    }

                    Send();
                }

                if (lineTask.IsCompleted)
                {
                    if (lineTask.Status != TaskStatus.Faulted)
                    {

                        var received = System.Text.ASCIIEncoding.ASCII.GetString(buffer.Select(ch => (byte)ch).ToArray(), 0, lineTask.Result);
                        foreach(var ch in received)
                        {
                            if(ch == '\n' || ch == '\r')
                            {
                                if (!String.IsNullOrEmpty(messageBuffer))
                                {
                                    LineReceived?.Invoke(this, messageBuffer);
                                    ProcessResponseLine(messageBuffer);
                                }
                                messageBuffer = String.Empty;
                            }
                            else
                            {
                                messageBuffer += ch;
                            }
                        }
                        
                    }
                }
            }
            else
            {
                var lineTask = _reader.ReadLineAsync();
                /* While we are awaiting for a line to come in process any outgoing stuff */
                while (!lineTask.IsCompleted)
                {
                    if (!Connected)
                    {
                        return;
                    }

                    Send();
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

                await GrblErrorProvider.InitAsync();

                UnacknowledgedBytesSent = 0;

                if (Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
                {
                    Enqueue("*", true);
                }

                while (Connected)
                {
                    try
                    {
                         WorkLoop();
                        await Task.Delay(2);
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
