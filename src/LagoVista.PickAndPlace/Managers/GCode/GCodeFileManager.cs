using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.GCode;
using LagoVista.GCode.Commands;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public partial class GCodeFileManager : MachineViewModelBase, IGCodeFileManager, IGCodeCommandHandler
    {
        ILogger _logger;
        IToolChangeManager _toolChangeManager;

        GCodeFile _file;

        bool _isDirty;

        int _tail = 0;
        int _head = 0;

        int? _pendingToolChangeLine = null;

        DateTime? _started;

        public GCodeFileManager(IMachineRepo machineRepo, ILogger logger, IToolChangeManager toolChangeManager) : base(machineRepo)
        {
            _logger = logger;
            _toolChangeManager = toolChangeManager;

            Lines = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            RapidMoves = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            Arcs = new System.Collections.ObjectModel.ObservableCollection<Line3D>();
            OpenFileCommand = new Core.Commanding.RelayCommand(OpenFile);
            SendGCodeFileCommand = CreateCommand(StartJob, CanSendGcodeFile);
        }

        protected override void MachineChanged(IMachine machine)
        {
            base.MachineChanged(machine);
            machine.RegisterGCodeFileCommandHandler(this);
        }

        async void OpenFile()
        {
            var result = await this.Popups.ShowOpenFileAsync();
            if (!String.IsNullOrEmpty(result))
                await OpenFileAsync(result);

        }

        private async void HandleToolChange(ToolChangeCommand mcode)
        {
            if(!await _toolChangeManager.HandleToolChange(mcode))
            {
                ResetJob();
            }
            _pendingToolChangeLine = null;
        }

        public GCodeCommand GetNextJobItem()
        {
            if (_started == null)
                _started = DateTime.Now;

            /* If we have queued up a pending tool change, don't send any more lines until tool change completed */
            if (_pendingToolChangeLine != null)
            {
                return null;
            }

            if (Head < _file.Commands.Count)
            {
                var cmd = _file.Commands[Head];

                /* If Next Command up is a GCodeOperationTool Change, set the nullable property to that line and bail. */
                if (cmd is ToolChangeCommand)
                {
                    if (MachineRepo.CurrentMachine.Settings.PauseOnToolChange)
                    {
                        _pendingToolChangeLine = Head;
                    }

                    return null;
                }

                Head++;

                cmd.Status = GCodeCommand.StatusTypes.Queued;

                return cmd;
            }

            return null;
        }

        public void SetGCode(string gcode)
        {
            var file = GCodeFile.FromString(gcode, _logger);
            FindExtents(file);
            File = file;
        }

        public GCodeCommand CurrentCommand
        {
            get { return _file == null || Tail < _file.Commands.Count ? null : _file.Commands[Tail]; }
        }

        public int CommandAcknowledged()
        {
            var sentCommandLength = _file.Commands[Tail].MessageLength;
            if (_file.Commands[Tail].Status == GCodeCommand.StatusTypes.Sent)
            {
                _file.Commands[Tail].Status = GCodeCommand.StatusTypes.Acknowledged;
                Tail++;

                if (_pendingToolChangeLine != null && _pendingToolChangeLine.Value == Tail)
                {
                    _file.Commands[Tail].Status = GCodeCommand.StatusTypes.Internal;
                    HandleToolChange(_file.Commands[Tail] as ToolChangeCommand);
                    Head++;
                    Tail++;
                }

                if (Tail < _file.Commands.Count)
                {
                    _file.Commands[Tail].StartTimeStamp = DateTime.Now;
                }
                else
                {
                    RaisePropertyChanged(nameof(IsCompleted));
                }

                return sentCommandLength;
            }
            else
            {
                Debug.WriteLine("Attempt to acknowledge command but not sent.");

                return 0;
            }
        }

        public void ResetJob()
        {
            Head = 0;
            Tail = 0;
            _pendingToolChangeLine = null;

            if (Commands != null)
            {
                foreach (var cmd in Commands)
                {
                    cmd.Status = GCodeCommand.StatusTypes.Ready;
                }
            }
        }

        public void QueueAllItems()
        {
            foreach (var cmd in Commands)
            {
                cmd.Status = GCodeCommand.StatusTypes.Ready;
            }
        }

        public void ApplyHeightMap(HeightMap map)
        {
            if (_file == null)
            {
                _logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "GCodeFileManager_ApplyHeightMap", "Attempt to apply height map to empty gcode file.");
                MachineRepo.CurrentMachine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to apply height map to empty gcode file");
            }
            else
            {
                _file = map.ApplyHeightMap(_file);
                IsDirty = true;
                RaisePropertyChanged(nameof(File));
                RaisePropertyChanged(nameof(Lines));
                RaisePropertyChanged(nameof(Arcs));
                RaisePropertyChanged(nameof(RapidMoves));
                RenderPaths(_file);
            }
        }

        public void ArcToLines(double length)
        {
            if (_file == null)
            {
                _logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "GCodeFileManager_ArcToLines", "Attempt to convert arc to lines with empty gcode file.");
                MachineRepo.CurrentMachine.AddStatusMessage(StatusMessageTypes.Warning, "Attempt to convert arc to lines with an empty gcode file");
            }
            else
            {
                _file = _file.ArcsToLines(length);
                IsDirty = true;
            }
        }

        public void ApplyOffset(double xOffset, double yOffset, double angle = 0)
        {
            foreach (var command in _file.Commands)
            {
                var motionCommand = command as GCodeMotion;
                if (motionCommand != null)
                {
                    motionCommand.ApplyOffset(xOffset, yOffset, angle);
                }
            }

            IsDirty = true;
            RaisePropertyChanged(nameof(IsDirty));
            RaisePropertyChanged(nameof(File));
            RenderPaths(File);
        }

        public void StartJob()
        {
            ResetJob();
            MachineRepo.CurrentMachine.SetMode(OperatingMode.SendingGCodeFile);
            RaiseCanExecuteChanged();
        }

        public void CancelJob()
        {
            _pendingToolChangeLine = null;
            RaiseCanExecuteChanged();

        }

        public void PauseJob()
        {
            RaiseCanExecuteChanged();
        }

        public Task<bool> OpenFileAsync(string path)
        {
            try
            {
                if (String.IsNullOrEmpty(path))
                {
                    File = null;
                    return Task.FromResult(false);
                }

                var file = GCodeFile.Load(path);
                if (file != null)
                {
                    Head = 0;
                    Tail = 0;
                    File = file;
                    var parts = path.Split('\\');
                    FileName = parts[parts.Length - 1];
                }
                else
                {
                    File = null;
                }

                RaiseCanExecuteChanged();

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.AddException("GCodeFileManager", ex);
                return Task.FromResult(false);
            }
        }


        private void FindExtents(GCodeFile file)
        {
            var min = new Point3D<double>() { X = 99999.0, Y = 99999.0, Z = 99999.0 };
            var max = new Point3D<double>() { X = -99999.0, Y = -99999.0, Z = -999999.0 };

            bool first = true;
            foreach (var cmd in file.Commands)
            {
                var motionCmd = cmd as GCodeMotion;
                if (motionCmd != null)
                {
                    if (!first)
                    {
                        min.X = Math.Min(min.X, motionCmd.Start.X);
                        min.Y = Math.Min(min.Y, motionCmd.Start.Y);
                        min.Z = Math.Min(min.Z, motionCmd.Start.Z);
                        min.X = Math.Min(min.X, motionCmd.End.X);
                        min.Y = Math.Min(min.Y, motionCmd.End.Y);
                        min.Z = Math.Min(min.Z, motionCmd.End.Z);
                    }
                    else
                    {
                        first = false;
                    }

                    max.X = Math.Max(max.X, motionCmd.Start.X);
                    max.Y = Math.Max(max.Y, motionCmd.Start.Y);
                    max.Z = Math.Max(max.Z, motionCmd.Start.Z);
                    max.X = Math.Max(max.X, motionCmd.End.X);
                    max.Y = Math.Max(max.Y, motionCmd.End.Y);
                    max.Z = Math.Max(max.Z, motionCmd.End.Z);
                }
            }

            Max = max;
            Min = min;
        }

        public async Task SaveGCodeAsync(String fileName)
        {
            if (File == null)
            {
                throw new Exception("Attempt to save file when none exists");
            }

            var lines = new List<string>();

            foreach (var cmd in File.Commands)
            {
                lines.Add(cmd.OriginalLine);
            }

            await Core.PlatformSupport.Services.Storage.WriteAllLinesAsync(fileName, lines);
            IsDirty = false;

            RaiseCanExecuteChanged();
        }

        public void SetFile(GCodeFile file)
        {
            File = file;
        }

        public Task CloseFileAsync()
        {
            File = null;
            RaiseCanExecuteChanged();
            return Task.FromResult(default(object));
        }

        public ObservableCollection<Line3D> Lines { get; private set; }

        public ObservableCollection<Line3D> RapidMoves { get; private set; }

        public ObservableCollection<Line3D> Arcs { get; private set; }

        public TimeSpan EstimatedTimeRemaining { get { return _file == null ? TimeSpan.Zero : _file.EstimatedRunTime - ElapsedTime; } }

        public TimeSpan ElapsedTime { get { return _started.HasValue ? DateTime.Now - _started.Value : TimeSpan.Zero; } }

        public DateTime EstimatedCompletion { get { return _started.HasValue ? _started.Value.Add(_file.EstimatedRunTime) : DateTime.Now; } }

        public int CurrentIndex { get { return _head; } }
        public int TotalLines { get { return _file == null ? 0 : _file.Commands.Count; } }

        public bool HeightMapApplied
        {
            get
            {
                return _file == null ? false : _file.HeightMapApplied;
            }
        }

        LagoVista.Core.Models.Drawing.Point3D<double> _min;
        public LagoVista.Core.Models.Drawing.Point3D<double> Min
        {
            get { return _min; }
            set
            {
                _min = value;
                RaisePropertyChanged();
            }
        }

        LagoVista.Core.Models.Drawing.Point3D<double> _max;
        public LagoVista.Core.Models.Drawing.Point3D<double> Max
        {
            get { return _max; }
            set
            {
                _max = value;
                RaisePropertyChanged();
            }
        }

        public int Head
        {
            get { return _head; }
            set { Set(ref _head, value); }
        }

        public int Tail
        {
            get { return _tail; }
            set { Set(ref _tail, Math.Max(value, 0)); }
        }

        public GCodeFile File
        {
            get { return _file; }
            set
            {

                if (value != null)
                {
                    FindExtents(value);
                    RenderPaths(value);
                }
                else
                {
                    FileName = "<empty>";
                    Max = null;
                    Min = null;
                    ClearPaths();
                }

                _file = value;

                _head = 0;
                _tail = 0;

                RaisePropertyChanged(nameof(HasValidFile));
                RaisePropertyChanged(nameof(Commands));
                RaisePropertyChanged(nameof(EstimatedTimeRemaining));
                RaisePropertyChanged(nameof(ElapsedTime));
                RaisePropertyChanged(nameof(EstimatedCompletion));
                RaisePropertyChanged(nameof(TotalLines));
                RaisePropertyChanged(nameof(CurrentIndex));
            }
        }

        private string _fileName = "<empty>";
        public string FileName
        {
            get { return _fileName; }
            set { Set(ref _fileName, value); }
        }

        public IEnumerable<GCodeCommand> Commands
        {
            get { return _file == null ? null : _file.Commands; }
        }

        public bool HasValidFile
        {
            get { return _file != null; }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { Set(ref _isDirty, value); }
        }

        public bool IsCompleted { get { return Tail == TotalLines; } }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand CloseFileCommand { get; }

        public bool CanSendGcodeFile()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                HasValidFile &&
                MachineRepo.CurrentMachine.Connected &&
                MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        private void RenderPaths(GCodeFile file)
        {
            ClearPaths();

            foreach (var cmd in file.Commands)
            {
                if (cmd is GCodeLine)
                {
                    var gcodeLine = cmd as GCodeLine;
                    if (gcodeLine.Rapid)
                    {
                        RapidMoves.Add(new Line3D()
                        {
                            Start = gcodeLine.Start,
                            End = gcodeLine.End
                        });
                    }
                    else
                    {
                        Lines.Add(new Line3D()
                        {
                            Start = gcodeLine.Start,
                            End = gcodeLine.End
                        });
                    }
                }

                if (cmd is GCodeArc)
                {
                    var arc = cmd as GCodeArc;
                    var segmentLength = arc.Length / 50;
                    var segments = (cmd as GCodeArc).Split(segmentLength);
                    foreach (var segment in segments)
                    {
                        Arcs.Add(new Line3D()
                        {
                            Start = segment.Start,
                            End = segment.End
                        });
                    }
                }
            }

            RaisePropertyChanged(nameof(Lines));
            RaisePropertyChanged(nameof(RapidMoves));
            RaisePropertyChanged(nameof(Arcs));
        }

        private void ClearPaths()
        {
            Lines.Clear();
            RapidMoves.Clear();
            Arcs.Clear();
        }

        public RelayCommand SendGCodeFileCommand { get; }
    }
}           