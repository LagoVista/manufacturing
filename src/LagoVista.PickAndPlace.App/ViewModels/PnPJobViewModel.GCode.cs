using LagoVista.GCode;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private async Task SendInstructionSequenceAsync(List<string> cmds)
        {
            var file = GCodeFile.FromList(cmds, Logger);
            _machine.SetFile(file);
            _machine.GCodeFileManager.StartJob();
            while (_machine.Mode == OperatingMode.SendingGCodeFile) await Task.Delay(1);
        }

        private string SafeHeightGCodeGCode()
        {
            return $"G0 Z{_machine.Settings.ToolSafeMoveHeight} F{_machine.Settings.FastFeedRate}";
        }

        private string PickHeightGCode()
        {
            return $"G0 Z{_machine.Settings.ToolPickHeight} F{_machine.Settings.FastFeedRate}";
        }

        private string PlaceHeightGCode(ComponentPackage package)
        {
            return $"G0 Z{_machine.Settings.ToolBoardHeight - package.Height} F{_machine.Settings.FastFeedRate}";
        }

        private string DwellGCode(int pauseMS)
        {
            return $"G4 P{pauseMS}";
        }

        private string RotationGCode(double cRotation)
        {
            if (cRotation == 360)
                cRotation = 0;
            else if (cRotation > 360)
                cRotation -= 360;
            else if (cRotation == 270)
                cRotation = -90;

            var scaledAngle = cRotation;
            return $"G0 E{scaledAngle} F10000";
        }

        private string WaitForComplete()
        {
            return "M400";
        }

        private string ProduceVacuumGCode(bool value)
        {
            switch (_machine.Settings.MachineType)
            {
                case FirmwareTypes.Repeteir_PnP: return $"M42 P27 S{(value ? 255 : 0)}";
                case FirmwareTypes.LagoVista_PnP: return $"M64 S{(value ? 255 : 0)}";
            }

            throw new Exception($"Can't produce vacuum GCode for machien type: {_machine.Settings.MachineType} .");
        }

        private string ProducePuffGCode(bool value)
        {
            switch (_machine.Settings.MachineType)
            {
                case FirmwareTypes.Repeteir_PnP:
                    return ($"M42 P23 S{(value ? 255 : 0)}\n");
            }

            throw new Exception($"Can't produce vacuum GCode for machien type: {_machine.Settings.MachineType} .");
        }
    }
}
