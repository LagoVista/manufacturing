// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f55c185ff450f5359c3a773f5353c06315df87b7cf6ed2887b071fae60ce6828
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels;
using System.Threading;

namespace LagoVista.PickAndPlace.Managers
{
    public enum ProbeStatus
    {
        Idle,
        Probing,
        Success,
        TimedOut,
        Cancelled,
    }



    public class ProbingManager : MachineViewModelBase, IProbingManager, IProbeCompletedHandler
    {
        Timer _timer;
        ProbeStatus _status = ProbeStatus.Idle;


        public ProbingManager(IMachineRepo machineRepo, ILogger logger) : base(machineRepo)
        {
            StartProbeCommand = CreateCommand(StartProbe, CanProbe);
            CancelProbeCommand = CreateCommand(CancelProbe, CanProbe);
        }

        protected override void MachineChanged(IMachine machine)
        {
            base.MachineChanged(machine);
            Machine.PropertyChanged += (e, a) => DispatcherServices.Invoke(() => RaiseCanExecuteChanged()); 
        }


        public void StartProbe()
        {
            if (Machine.RegisterProbeCompletedHandler(this))
            {
                _status = ProbeStatus.Probing;

                if (Machine.SetMode(OperatingMode.ProbingHeight))
                {
                    _timer = new Timer(Timer_Tick, null, Machine.Settings.ProbeTimeoutSeconds * 1000, 0);
                    Machine.SendCommand($"G38.3 Z-{Machine.Settings.ProbeMaxDepth.ToString("0.###", Constants.DecimalOutputFormat)} F{Machine.Settings.ProbeHeightMovementFeed.ToString("0.#", Constants.DecimalOutputFormat)}");
                }
            }
        }

        private void Timer_Tick(object state)
        {
            if (_timer != null)
            {

                Machine.SetMode(OperatingMode.Manual);
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Probing timed out after {Machine.Settings.ProbeTimeoutSeconds} sec.");

                _status = ProbeStatus.TimedOut;

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
                Machine.UnregisterProbeCompletedHandler();
            }
        }

        public void CancelProbe()
        {
            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Manually Cancelled");
            Machine.UnregisterProbeCompletedHandler();
            _status = ProbeStatus.Cancelled;

            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }



            Machine.SetMode(OperatingMode.Manual);
        }

        public bool CanProbe()
        {
            return Machine.IsInitialized && Machine.Connected && Machine.Mode == OperatingMode.Manual;
        }


        public void SetZAxis(double z)
        {
            Machine.SendCommand($"G92 Z{Machine.Settings.ProbeOffset.ToString("0.###", Constants.DecimalOutputFormat)}");
        }

        public void ProbeCompleted(Vector3 position)
        {
            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Completed Offset {position.Z}");

            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }

            _status = ProbeStatus.Success;

            SetZAxis(position.Z);
            Machine.SendCommand("G0 Z10");
            Machine.SetMode(OperatingMode.Manual);
            Machine.UnregisterProbeCompletedHandler();
        }

        public void ProbeFailed()
        {
            Machine.AddStatusMessage(StatusMessageTypes.Info, $"Probing Failed, Invalid Response");
            Machine.UnregisterProbeCompletedHandler();
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }

        public ProbeStatus Status { get { return _status; } }

        


        public RelayCommand StartProbeCommand { get; }
        public RelayCommand CancelProbeCommand { get; }
    }
}
