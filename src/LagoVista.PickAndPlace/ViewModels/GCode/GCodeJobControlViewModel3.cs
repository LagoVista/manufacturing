
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public partial class GCodeJobControlViewModel
    {
        public bool IsCreatingHeightMap { get { return _machineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return _machineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return _machineRepo.CurrentMachine.Mode == OperatingMode.SendingGCodeFile; } }
    }
}
