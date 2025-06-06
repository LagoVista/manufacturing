﻿using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{


    public interface IMachine : INotifyPropertyChanged
    {
        event EventHandler<string> LineReceived;

        event EventHandler MachineConnected;
        event EventHandler MachineDisconnected;
        event EventHandler SettingsLocked;
        event EventHandler SettingsUnlocked;



        /// <summary>
        /// As commands are sent to the machine the number of bytes for that command are added to 
        /// the UnacknowledgedByteSet property, once the command has been acknowledged the number 
        /// of bytes for that acknowledged command will be subtracted.  This will keep a rough idea 
        /// of the number of bytes that have been buffered on the machine.  The size of the buffer
        /// on the machine has been entered in settings, thus we know the available space and can
        /// send additional bytes for future commands and have them ready for the machine.  These
        /// can then be queued so the machine doesn't have to wait for the next communications.
        /// </summary>
        int UnacknowledgedBytesSent { get; }

        /// <summary>
        /// Number of items in the queue, thread safe.
        /// </summary>
        int ToSendQueueCount { get; }

        /// <summary>
        /// Perform any additional tasks to initialize the machine, should be called as soon 
        /// as possible
        /// </summary>
        /// <returns></returns>
        Task InitAsync();

        /// <summary>
        /// Will be set as soon as machine initialization has been completed.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// The current XYZ position of the machien with respect to the specified origin of the physical machine (0,0,0)
        /// </summary>
        Vector3 MachinePosition { get; }

        /// <summary>
        /// Machine Position - Work Space Offset
        /// </summary>
        Vector3 NormalizedPosition { get; }

        /// <summary>
        /// Current position with respect to the set offset (either managed by firmware or application)
        /// </summary>
        Vector3 WorkspacePosition {get;}

        double ToolCommonZ { get;  }
        
        double LeftToolHeadRotate { get; }
        double RightToolHeadRotate { get; }


        bool TopLightOn { get; set; }
        bool BottomLightOn { get; set; }
        
        
        bool PuffPump { get; set; }
        bool VacuumSolendoid { get; set; }
        bool PuffSolenoid { get; set; }


        bool VacuumPump { get; set; }
        bool LeftVacuumPump { get; set; }
        bool RightVacuumPump { get; set; }

        ulong Vacuum { get; set; }
        ulong RightVacuum { get; set; }
        ulong LeftVacuum { get; set; }

        bool IsPnPMachine { get; }

        void SetToolHeadHeight(double height);

        void RotateToolHeadRelative(double angle);

        void RotateToolHeadAngleAbsolute(double angle);

        void ClearToolHeadAngle();

        Task<InvokeResult<long>> ReadVacuumAsync();
        Task<InvokeResult<long>> ReadLeftVacuumAsync();
        Task<InvokeResult<long>> ReadRightVacuumAsync();

        void Dwell(int ms);

        /// <summary>
        /// Current mode of the machine, such as Connected, Running a Job, etc....
        /// </summary>
        OperatingMode Mode { get; }

        /// <summary>
        /// Total number of messages in the message list to be displayed, primarily used to scroll the list.
        /// </summary>
        int MessageCount { get; }

        /// <summary>
        /// Type of view such as camera, tool1, tool2
        /// </summary>
        ViewTypes ViewType { get; set; }

        /// <summary>
        /// Method to set the view type and wait for it to be completed before continue.
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>
        Task SetViewTypeAsync(ViewTypes viewType);

        /// <summary>
        /// Mode in which GCode commands should be interpretted.  These are either absolute with repsect to
        /// the origin, or incremenental which should be added to the current position.
        /// </summary>
        ParseDistanceMode DistanceMode { get; set; }

        /// <summary>
        /// If the units are to be sent as inches or millimeters
        /// </summary>
        ParseUnit Unit { get; }

        /// <summary>
        /// Arch Plene, have to admit, I don't understand this yet...from original OpenCNCPilot, I'm 100% positive it's needed at some point though
        /// </summary>
        ArcPlane Plane { get; }

        /// <summary>
        /// Status as reported by machine.
        /// </summary>
        String Status { get; }

        /// <summary>
        /// Business logic to manage the sending of GCode files to the machine.
        /// </summary>


        bool Busy { get; }

        /// <summary>
        /// Is the machine currently connected
        /// </summary>
        bool Connected { get; }

        MachineToolHead CurrentMachineToolHead { get; set; }

        bool AreSettingsLocked { get; set; }
        
        /// <summary>
        /// Connect to the machine
        /// </summary>
        /// <param name="serialPort"></param>
        /// <returns></returns>
        Task ConnectAsync(SerialPort serialPort);
        void Connect(SerialPort serialPort);
        Task ConnectAsync(SimulatedSerialPort serialPort);


        void Connect(ISocketClient socketClient);


        /// <summary>
        /// Connect to a remote machine via a socket.
        /// </summary>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        Task ConnectAsync(ISocketClient socketClient);

        /// <summary>
        /// Disconnect from the machien
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();

        bool MotorsEnabled { get; set; }

        void SetRelativeMode();
        void SetAbsoluteMode();            

        /// <summary>
        /// Perform a soft reset
        /// </summary>
        void SoftReset();

        void FeedHold();

        void CycleStart();

        void ClearAlarm();

        void HomingCycle();
        void SetHomed();

        void HomeViaOrigin();

        void SetAbsoluteWorkSpaceHome();

        void SetFavorite1();

        void SetFavorite2();

        void GotoFavorite1();

        void GotoFavorite2();

        void SpindleOn();
        void SpindleOff();

        void LaserOn();

        void LaserOff();

        void GotoPoint(Point2D<double> point, bool rapidMove = true, bool relativeMove = false);

        void GotoPoint(double x, double y, bool rapidMove = true, bool relativeMove = false);

        void GotoPoint(double x, double y, double feedRate);

        void GotoPoint(double x, double y, double z, bool rapidMove = true);

        Task<InvokeResult> ResetMachineCoordinates(Point2D<double> point);

        void SetWorkspaceHome();
        void GotoWorkspaceHome();

        /// <summary>
        /// Send a message to the machine to immediately stop any motion operation
        /// </summary>
        void EmergencyStop();

        /// <summary>
        /// This method can be called to ensure that the machine can transition to the specified operating mode, if it can't a message will be added to the output and false will be returned.
        /// </summary>
        /// <param name="mode">The new desired transition mode.</param>
        /// <returns>True if you can transition into the mode, false if you can not.</returns>
        bool CanSetMode(OperatingMode mode);
    
        /// <summary>
        /// Transition the machine to the new mode.
        /// </summary>
        /// <param name="mode">The new mode</param>
        /// <returns>True if you can transition occurred, false if it did not, if it did not a warning message will be written to the message output.</returns>
        bool SetMode(OperatingMode mode);
        
        void DebugWriteLine(string message);

        /// <summary>
        /// Determine if there are enough bytes in the estimated machine buffer to send the next command based on the bytes required to send that command
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>True if there is space available to send bytes</returns>
        bool HasBufferSpaceAvailableForByteCount(int bytes);

        /// <summary>
        /// Send a free form comamdn to the machine.
        /// </summary>
        /// <param name="cmd">Text that represents the command</param>
        void SendCommand(String cmd);

        void SendSafeMoveHeight();

        bool LocationUpdateEnabled { get; set; }

        /// <summary>
        /// Add a message to be displayed to the user.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        void AddStatusMessage(StatusMessageTypes type, String message, MessageVerbosityLevels verbosity = MessageVerbosityLevels.Normal);

        /// <summary>
        /// Current settings as to be used by the machine.
        /// </summary>
        LagoVista.Manufacturing.Models.Machine Settings { get; set; }

        ObservableCollection<string> PendingQueue { get; }

        void ConfigureTopLight(bool on, byte power, byte red, byte green, byte blue);
        void ConfigureBottomLight(bool on, byte power, byte red, byte green, byte blue);

        byte TopRed { get; set; }
        byte BottomRed { get; set; }
        byte TopGreen { get; set; }
        byte BottomGreen { get; set; }
        byte TopBlue { get; set; }
        byte BottomBlue { get; set; }
        byte TopPower { get; set; }
        byte BottomPower { get; set; }

        void I2CSend(byte address, byte register);
        void I2CSend(byte address, byte[] buffer);
        Task<InvokeResult<byte>> I2CReadHexByte(byte address);

        void SetVisionProfile(CameraTypes cameraType, string profile);
        void SetVisionProfile(CameraTypes cameraType, VisionProfileSource sourceType, string sourceId, VisionProfile profile);

        Task MoveToToolHeadAsync(MachineToolHead toolHeadToMoveTo, bool verbose = false);
        Task MoveToCameraAsync(bool verbose = false);
        
        void MoveToToolHead(MachineToolHead toolHeadToMoveTo, bool verbose = false);
        void MoveToCamera(bool verbose = false);
        IImageCaptureService PositionImageCaptureService { get; set; }
        IImageCaptureService PartInspectionCaptureService { get; set; }

        bool WasMachineHomed { get; }
        bool WasMachinOriginCalibrated { get; set; }

        Task GoToPartInspectionCameraAsync(double? rotate = null);

        Task<InvokeResult<Point2D<double>>> GetCurrentLocationAsync(uint timeout = 2500, bool verbose = false);

        Task<InvokeResult> SpinUntilIdleAsync(uint timeout = 2500, bool verbose = false);

        ObservableCollection<Models.StatusMessage> Messages { get; }
        ObservableCollection<Models.StatusMessage> SentMessages { get; }

        bool IsLocating { get; set; }

        bool RegisterProbeCompletedHandler(IProbeCompletedHandler probeCompleted);
        bool UnregisterProbeCompletedHandler();

        void RegisterGCodeFileCommandHandler(IGCodeCommandHandler commandHandler);
    }
}
