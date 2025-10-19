// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7eb41d5a191b870ecc7e0e9aad3ab5ed4a3a35fb23f2201b4600241a784022e8
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.Core.Attributes;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.Core.Interfaces;
using LagoVista.PickAndPlace.Models;
using System.Runtime.InteropServices;

namespace LagoVista.Manufacturing.Models
{
    public enum JogDirections
    {
        YPlus,
        YMinus,
        XPlus,
        XMinus,
        ZPlus,
        ZMinus,
        T0Plus,
        T0Minus,
        T1Plus,
        T1Minus,
        LeftToolHeadRotateMinus,
        LeftToolHeadRotatePlus,
        RightToolHeadRotateMinus,
        RightToolHeadRotatePlus,
    }

    public enum MachineOrigin
    {
        Top_Left,
        Bottom_Left,
        Top_Right,
        Bottom_Right,
    }

    public enum ConnectionTypes
    {
        Serial_Port,
        Network
    }

    public enum JogGCodeCommand
    {
        G0,
        G1
    }

    public enum Axis
    {
        XY,
        Z
    }

    public enum ViewTypes
    {
        Camera,
        Tool1,
        Tool2
    }

    public enum ResetAxis
    {
        X,
        Y,
        Z,
        T0,
        T1,
        C,
        All
    }

    public enum HomeAxis
    {
        X,
        Y,
        Z,
        T0,
        T1,
        C,
        All
    }


    public enum StepModes
    {
        Micro,
        Small,
        Medium,
        Large,
        XLarge
    }

    public enum FirmwareTypes
    {
        GRBL1_1,
        GRBL1_1_SL_Custom,
        Marlin,
        Marlin_Laser,
        LagoVista,
        LagoVista_PnP,
        SimulatedMachine,
        Repeteir_PnP,
        LumenPnP_V4_Marlin
    }

    public enum MachineTypes
    {
        Laser,
        Cnc,
        PickAndPlace,
        Other,
    }

    public enum OperatingMode
    {
        Manual,
        Alarm,
        SendingGCodeFile,
        PendingToolChange,
        ProbingHeightMap,
        ProbingHeight,
        AligningBoard,
        Disconnected,
        PlacingParts,
    }

    public enum StatusMessageTypes
    {
        ReceivedLine,
        SentLine,
        SentLinePriority,
        Info,
        Warning,
        FatalError
    }

    public enum MessageVerbosityLevels
    {
        Diagnostics,
        Verbose,
        Normal,
        Quiet,
    }
    
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Machine_Title, ManufacturingResources.Names.Machine_Description,
        ManufacturingResources.Names.Machine_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-ruler", Cloneable: true,
        SaveUrl: "/api/mfg/machine", GetUrl: "/api/mfg/machine/{id}", GetListUrl: "/api/mfg/machines", FactoryUrl: "/api/mfg/machine/factory",
        DeleteUrl: "/api/mfg/machine/{id}", ListUIUrl: "/mfg/machinesettings", EditUIUrl: "/mfg/machine/{id}", CreateUIUrl: "/mfg/machine/add")]
    public class Machine : MfgModelBase, ISummaryFactory, INotifyPropertyChanged, IFormDescriptor, IFormDescriptorCol2
    {

        public int StatusPollIntervalIdle { get; set; }
        public int StatusPollIntervalRunning { get; set; }
        public int ControllerBufferSize { get; set; } = 128;

        public double ViewportArcSplit { get; set; }
        public double ArcToLineSegmentLength { get; set; }
        public double SplitSegmentLength { get; set; }

        private Point2D<double> _knownCalibrationPoint = new Point2D<double>();
        public Point2D<double> KnownCalibrationPoint
        {
            get { return _knownCalibrationPoint; }
            set { Set(ref _knownCalibrationPoint, value); }
        }

        private Point2D<double> _tool1Offset = new Point2D<double>();
        public Point2D<double> Tool1Offset
        {
            get { return _tool1Offset; }
            set { Set(ref _tool1Offset, value); }
        }

        private Point2D<double> _tool2Offset = new Point2D<double>();
        public Point2D<double> Tool2Offset
        {
            get { return _tool2Offset; }
            set { Set(ref _tool2Offset, value); }
        }

        private Point2D<double> _pcbOffset = new Point2D<double>();
        public Point2D<double> PCBOffset
        {
            get { return _pcbOffset; }
            set { Set(ref _pcbOffset, value); }
        }

        private Point3D<double> _defaultWorkspaceHome = new Point3D<double>();
        public Point3D<double> DefaultWorkspaceHome
        {
            get { return _defaultWorkspaceHome; }
            set { Set(ref _defaultWorkspaceHome, value); }
        }

        private Point2D<double> _machineFiducial = new Point2D<double>();
        public Point2D<double> MachineFiducial
        {
            get { return _machineFiducial; }
            set { Set(ref _machineFiducial, value); }
        }

        private Point2D<double> _defaultWorkOrigin = new Point2D<double>();
        public Point2D<double> DefaultWorkOrigin
        {
            get => _defaultWorkOrigin;
            set => Set(ref _defaultWorkOrigin, value);
        }

        double _workOriginZ = 5.5;
        public double WorkOriginZ
        {
            get => _workOriginZ;
            set => Set(ref _workOriginZ, value);
        }

        private int _fastFeedRate = 10000;
        public int FastFeedRate
        {
            get { return _fastFeedRate; }
            set { Set(ref _fastFeedRate, value); }
        }

        private double _defaultSafeMoveHeight = 0;
        public double DefaultSafeMoveHeight
        {
            get => _defaultSafeMoveHeight; 
            set => Set(ref _defaultSafeMoveHeight, value);
        }

        private SerialPortInfo _currentSerialPort;
        public SerialPortInfo CurrentSerialPort
        {
            get { return _currentSerialPort; }
            set { Set(ref _currentSerialPort, value); }
        }

        private SerialPortInfo _serialPort2;
        public SerialPortInfo SerialPort2
        {
            get { return _serialPort2; }
            set { Set(ref _serialPort2, value); }
        }

        private ConnectionTypes _connectionType;
        public ConnectionTypes ConnectionType
        {
            get { return _connectionType; }
            set { Set(ref _connectionType, value); }
        }

        private String _ipAddress;
        public String IPAddress
        {
            get { return _ipAddress; }
            set { Set(ref _ipAddress, value); }
        }

        private String _telnetPort;
        public String TelnetPort
        {
            get { return _telnetPort; }
            set { Set(ref _telnetPort, value); }
        }

        private Point2D<double> _partStripScaler = new Point2D<double>() { X = 1.0, Y = 1.0 };
        public Point2D<double> PartStripScaler
        {
            get => _partStripScaler;
            set => Set(ref _partStripScaler, value);
        }


        [JsonIgnore]
        public double SafMoveHeight
        {
            get
            {

                return DefaultSafeMoveHeight;
                //if (_currentNozzle == null || !_currentNozzle.SafeMoveHeight.HasValue)
                    

                //return _currentNozzle.SafeMoveHeight.Value;
            }
        }

        //[JsonIgnore]
        //public double? ToolPickHeight
        //{
        //    get { return _currentNozzle?.PickHeight; }
        //    set
        //    {
        //        _currentNozzle.PickHeight = value;
        //        RaisePropertyChanged(nameof(ToolPickHeight));
        //    }
        //}

        ToolNozzleTip _currentNozzle = new ToolNozzleTip();
        public ToolNozzleTip CurrentNozzle
        {
            get { return _currentNozzle; }
            set
            {
                if (_currentNozzle != null)
                {
                    //var currentNozzle = Nozzles.Where(noz => noz.Id == _currentNozzle.Id).FirstOrDefault();
                    //if (currentNozzle != null)
                    //{
                    //    currentNozzle.Name = _currentNozzle.Name;
                    //    currentNozzle.BoardHeight = _currentNozzle.BoardHeight;
                    //    currentNozzle.PickHeight = _currentNozzle.PickHeight;
                    //    currentNozzle.SafeMoveHeight = _currentNozzle.SafeMoveHeight;
                    //}

                    _currentNozzle = value;
                    RaisePropertyChanged(nameof(CurrentNozzle));
           //         RaisePropertyChanged(nameof(ToolPickHeight));
                   // RaisePropertyChanged(nameof(ToolBoardHeight));
                    RaisePropertyChanged(nameof(SafMoveHeight));
                }
            }
        }

        ObservableCollection<ToolNozzleTip> _nozzles = new ObservableCollection<ToolNozzleTip>();
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTips_Title, ChildListDisplayMember:"nozzleTip.text", FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/machine/nozzletip/factory",
            OpenByDefault: true, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<ToolNozzleTip> Nozzles
        {
            get { return _nozzles; }
            set { Set(ref _nozzles, value); }
        }

        ObservableCollection<MachineStagingPlate> _stagingPlates = new ObservableCollection<MachineStagingPlate>();
        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlates_Title, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/machine/stagingplate/factory",
           OpenByDefault: true, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<MachineStagingPlate> StagingPlates
        {
            get { return _stagingPlates; }
            set { Set(ref _stagingPlates, value); }
        }


        ObservableCollection<MachineFeederRail> _feederRails = new ObservableCollection<MachineFeederRail>();
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRails, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/machine/feederrail/factory",
           OpenByDefault: true, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<MachineFeederRail> FeederRails
        {
            get { return _feederRails; }
            set { Set(ref _feederRails, value); }
        }

        /// <summary>
        /// Absolute position of the board in the Z axis, the actual place location will be 
        /// the this location plus the height of the part, we also will likely have different
        /// heights for the different nozzles.
        ///// </summary>
        //[JsonIgnore]
        //public double? ToolBoardHeight
        //{
        //    get { return CurrentNozzle?.BoardHeight; }
        //    set
        //    {
        //        CurrentNozzle.BoardHeight = value;
        //        RaisePropertyChanged(nameof(ToolBoardHeight));
        //    }
        //}


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-ruler";


        private Point2D<Double> _defaultToolReferencePoint;
        public Point2D<double> DefaultToolReferencePoint
        {
            get => _defaultToolReferencePoint;
            set => Set(ref _defaultToolReferencePoint, value);
        }

        private bool _connecttoMQTT;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_ConnectToMQTT, FieldType: FieldTypes.Bool, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public bool ConnectToMQTT
        {
            get => _connecttoMQTT;
            set => Set(ref _connecttoMQTT, value);
        }

        private string _mqttHostName;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_HostName, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string MqttHostName
        {
            get => _mqttHostName;
            set => Set(ref _mqttHostName, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_Port, FieldType: FieldTypes.Integer, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public int ConnectToPort { get; set; }

        bool _secureConnection;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_SecureConnection, FieldType: FieldTypes.Bool, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public bool SecureConnection
        {
            get => _secureConnection;
            set => Set(ref _secureConnection, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_DeviceId, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string DeviceId { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Machine_UserName, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string UserName { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_Password, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Password { get; set; }

        public string PasswordSecretId { get; set; }

        public string DefaultPnPMachineFile { get; set; }

        public bool EnableCodePreview { get; set; }
        public double ProbeSafeHeight { get; set; }
        public double ProbeMaxDepth { get; set; }
        public double ProbeMinimumHeight { get; set; }

        public bool PauseOnToolChange { get; set; }

        public double ProbeHeightMovementFeed { get; set; }

        public int ProbeTimeoutSeconds { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_GCodeMapping, WaterMark: ManufacturingResources.Names.Machine_GCodeMapping_Select, EntityHeaderPickerUrl: "/api/mfg/gcodemappings",
            FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<GCodeMapping> GcodeMapping { get; set; }

        Point2D<double> _workAreaSize = new Point2D<double>(400, 400);
        [FormField(LabelResource: ManufacturingResources.Names.Machine_WorkAreaSize, HelpResource: ManufacturingResources.Names.Machine_WorkAreaSize_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> WorkAreaSize
        {
            get { return _workAreaSize; }
            set { Set(ref _workAreaSize, value); }
        }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_WorkAreaOrigin, HelpResource: ManufacturingResources.Names.Machine_WorkAreaOrigin_Help,
            FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> WorkAreaOrigin { get; set; } = new Point2D<double>(0, 0);


        Point3D<double> _frameSize = new Point3D<double>(600, 600, 30);
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FrameSize, HelpResource: ManufacturingResources.Names.Machine_FrameSize_Help, FieldType: FieldTypes.Point3D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<double> FrameSize
        {
            get => _frameSize;
            set => Set(ref _frameSize, value);
        }

        Point3D<double> _workspaceFrameOffset = new Point3D<double>(100, 100, 100);
        [FormField(LabelResource: ManufacturingResources.Names.Machine_WorkspaceFrameOffset, HelpResource: ManufacturingResources.Names.Machine_WorkspaceFrameOffset_Help,
            FieldType: FieldTypes.Point3D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<double> WorkspaceFrameOffset
        {
            get => _workspaceFrameOffset;
            set => Set(ref _workspaceFrameOffset, value);
        }

        public bool AbortOnProbeFail { get; set; }


        double _probeFeed;
        public double ProbeFeed
        {
            get => _probeFeed;
            set => Set(ref _probeFeed, value);
        }

        private double _xyStepSize = 1;
        public double XYStepSize
        {
            get { return _xyStepSize; }
            set { Set(ref _xyStepSize, value); }
        }

        private double _zStepSize = 1;
        public double ZStepSize
        {
            get { return _zStepSize; }
            set { Set(ref _zStepSize, value); }
        }

        private MachineOrigin _machineOrigin = MachineOrigin.Bottom_Left;
        public MachineOrigin MachineOrigin
        {
            get { return _machineOrigin; }
            set { Set(ref _machineOrigin, value); }
        }

        private JogGCodeCommand _jogGCodeCommand = JogGCodeCommand.G1;
        public JogGCodeCommand JogGCodeCommand
        {
            get { return _jogGCodeCommand; }
            set { Set(ref _jogGCodeCommand, value); }
        }

        private MessageVerbosityLevels _messageVerbosity = MessageVerbosityLevels.Normal;
        public MessageVerbosityLevels MessageVerbosity
        {
            get { return _messageVerbosity; }
            set { Set(ref _messageVerbosity, value); }
        }

        private int _maximumFeedRate = 25000;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_MaxFeedRate, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int MaximumFeedRate
        {
            get => _maximumFeedRate;
            set => Set(ref _maximumFeedRate, value);
        }

        private int _jogFeedRate = 25000;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_JogFeedRate, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int JogFeedRate
        {
            get { return _jogFeedRate; }
            set { Set(ref _jogFeedRate, value); }
        }

        private int _powerOrRpm = 100;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_JogFeedRate, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int PowerOrRpm
        {
            get { return _powerOrRpm; }
            set { Set(ref _powerOrRpm, value); }
        }

        private StepModes _xyStepMode = StepModes.Medium;
        public StepModes XYStepMode
        {
            get { return _xyStepMode; }
            set { Set(ref _xyStepMode, value); }
        }

        private StepModes _zStepMode = StepModes.Medium;
        public StepModes ZStepMode
        {
            get { return _zStepMode; }
            set { Set(ref _zStepMode, value); }
        }

        private bool _hasLeftVacuumSensor;
        public bool HasLeftVacuumSensor
        {
            get => _hasLeftVacuumSensor;
            set => Set(ref _hasLeftVacuumSensor, value);
        }

        private bool _hasRightVacuumSensor;
        public bool HasRightVacuumSensor
        {
            get => _hasRightVacuumSensor;
            set => Set(ref _hasRightVacuumSensor, value);
        }

        private bool _hasLeftVacuum;
        public bool HasLeftVacuum
        {
            get => _hasLeftVacuum;
            set => Set(ref _hasLeftVacuum, value);
        }

        private bool _hasRightVacuum;
        public bool HasRightVacuum
        {
            get => _hasRightVacuum;
            set => Set(ref _hasRightVacuum, value);
        }

        private ulong _maxLeftVacuum;
        public ulong MaxLeftVacuum
        {
            get => _maxLeftVacuum;
            set => Set(ref _maxLeftVacuum, value);
        }

        private ulong _maxRightVacuum;
        public ulong MaxRightVacuum
        {
            get => _maxRightVacuum;
            set => Set(ref _maxRightVacuum, value);
        }

        private ulong _minLeftVacuum;
        public ulong MinLeftVacuum
        {
            get => _minLeftVacuum;
            set => Set(ref _minLeftVacuum, value);
        }

        private ulong _minRightVacuum;
        public ulong MinRightVacuum
        {
            get => _minRightVacuum;
            set => Set(ref _minRightVacuum, value);
        }


        private ulong _maxRpm = 100;
        public ulong MaxPowerOrRpm 
        {
            get => _maxRpm;
            set => Set(ref _maxRpm, value);
        }

        private ObservableCollection<MachineCamera> _cameras = new ObservableCollection<MachineCamera>();
        [FormField(LabelResource: ManufacturingResources.Names.Machine_Cameras, FactoryUrl: "/api/mfg/machine/camera/factory", FieldType: FieldTypes.ChildListInline, OpenByDefault: true, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<MachineCamera> Cameras
        {
            get => _cameras;
            set => Set(ref _cameras, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_ToolHeads, FactoryUrl: "/api/mfg/machine/toolhead/factory", FieldType: FieldTypes.ChildListInline, OpenByDefault: true, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<MachineToolHead> ToolHeads { get; set; } = new ObservableCollection<MachineToolHead>();

        //private MachineCamera _positioningCamera;
        //public MachineCamera PositioningCamera
        //{
        //    get { return Cameras.FirstOrDefault(cam => cam.CameraType.Value == CameraTypes.Position); }
        //}

        //private MachineCamera _partInspectionCamera;
        //public MachineCamera PartInspectionCamera
        //{
        //    get { return _partInspectionCamera; }
        //    set { Set(ref _partInspectionCamera, value); }
        //}



        FirmwareTypes _firmwareType;
        public FirmwareTypes FirmwareType
        {
            get => _firmwareType;
            set => Set(ref _firmwareType, value);
        }

        MachineTypes _cartesianMachineType;
        public MachineTypes CartesianMachineType 
        {
            get => _cartesianMachineType;
            set => Set(ref _cartesianMachineType, value);
        }

        public List<string> Validate()
        {
            var errs = new List<string>();

            if (String.IsNullOrEmpty(Name))
            {
                errs.Add("Machine Name is Requried.");
            }

            return errs;
        }

        public double ProbeOffset { get; set; }

        public Machine Clone()
        {
            return this.MemberwiseClone() as Machine;
        }

        public MachineSummary CreateSummary()
        {
            return new MachineSummary()
            {
                Id = Id,
                Icon = Icon,
                Name = Name,
                Key = Key,
                Description = Description,
                IsPublic = IsPublic
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

        public void ApplyDefaults()
        {
            ProbeOffset = 0.0;
            ControllerBufferSize = 120;
            StatusPollIntervalIdle = 1000;
            StatusPollIntervalRunning = 100;
            JogFeedRate = 2000;
            ProbeTimeoutSeconds = 30;
            MessageVerbosity = MessageVerbosityLevels.Normal;
            MachineOrigin = MachineOrigin.Bottom_Left;
            JogGCodeCommand = JogGCodeCommand.G0;
            ViewportArcSplit = 1;
            EnableCodePreview = true;
            ProbeSafeHeight = 5;
            ProbeMinimumHeight = 1;
            ProbeMaxDepth = 5;
            AbortOnProbeFail = false;
            ProbeFeed = 20;
            ProbeHeightMovementFeed = 1000;
            ArcToLineSegmentLength = 1;
            SplitSegmentLength = 5;
            XYStepSize = 1;
            ZStepSize = 1;
            MaxPowerOrRpm = 100;
            WorkAreaSize = new Point2D<double>(400, 400);
            FrameSize = new Point3D<double>(600, 600, 30);
        }

        public static Machine CreateDefault()
        {

            return new Machine()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Machine 1",
                ProbeOffset = 0.0,
                ControllerBufferSize = 120,
                StatusPollIntervalIdle = 1000,
                StatusPollIntervalRunning = 100,
                JogFeedRate = 2000,
                ProbeTimeoutSeconds = 30,
                MessageVerbosity = MessageVerbosityLevels.Normal,
                MachineOrigin = MachineOrigin.Bottom_Left,
                JogGCodeCommand = JogGCodeCommand.G0,
                ViewportArcSplit = 1,
                EnableCodePreview = true,
                ProbeSafeHeight = 5,
                ProbeMinimumHeight = 1,
                ProbeMaxDepth = 5,
                AbortOnProbeFail = false,
                ProbeFeed = 20,
                ProbeHeightMovementFeed = 1000,
                ArcToLineSegmentLength = 1,
                SplitSegmentLength = 5,
                XYStepSize = 1,
                ZStepSize = 1,
                MaxPowerOrRpm = 100,
                WorkAreaSize = new Point2D<double>(400, 400),
                FrameSize = new Point3D<double>(600, 600, 30)
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(WorkAreaSize),
                nameof(FrameSize),
                nameof(WorkspaceFrameOffset),
                nameof(GcodeMapping),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(MaximumFeedRate),
                nameof(JogFeedRate),
                nameof(ConnectToMQTT),
                nameof(DeviceId),
                nameof(MqttHostName),
                nameof(ConnectToPort),
                nameof(SecureConnection),
                nameof(UserName),
                nameof(Password),
                nameof(Nozzles),
                nameof(StagingPlates),
                nameof(FeederRails),
                nameof(ToolHeads),
                nameof(Cameras),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Machine_Title, ManufacturingResources.Names.Machine_Description,
        ManufacturingResources.Names.Machine_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-ruler", Cloneable: true,
        SaveUrl: "/api/mfg/machine", GetUrl: "/api/mfg/machine/{id}", GetListUrl: "/api/mfg/machines", FactoryUrl: "/api/mfg/machine/factory",
        DeleteUrl: "/api/mfg/machine/{id}", ListUIUrl: "/mfg/machinesettings", EditUIUrl: "/mfg/machine/{id}", CreateUIUrl: "/mfg/machine/add")]
    public class MachineSummary : SummaryData
    {

    }
}
