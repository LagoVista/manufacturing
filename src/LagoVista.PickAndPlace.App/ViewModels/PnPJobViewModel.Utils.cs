using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        public override async Task InitAsync()
        {
            await base.InitAsync();
            await ToolAlignmentVM.InitAsync();
            IsBusy = true;
        }

        private void PopulateParts()
        {
        }

        public void ExportBOM()
        {
            var bldr = new StringBuilder();

           // foreach(var partByValue in SelectedBuildFlavor.Components.Where(prt=>prt.Included).GroupBy(prt=>prt.Value))
           // {
           //     var value = partByValue.Key;
           //     bldr.Append($"{value}\t");
           //     foreach(var part in partByValue)
           //         bldr.Append($"{part.Name}, ");

           //     bldr.AppendLine();
           //}

           // System.IO.File.WriteAllText(@$"X:\PartList {SelectedBuildFlavor.Name}.txt", bldr.ToString());
        }

      

        private void SetNewHome()
        {
            // _machineRepo.CurrentMachine.SendCommand($"G92 X{ _machineRepo.CurrentMachine.Settings. _machineRepo.CurrentMachineFiducial.X} Y{ _machineRepo.CurrentMachine.Settings. _machineRepo.CurrentMachineFiducial.Y}");
             _machineRepo.CurrentMachine.SendCommand($"G92 X0 Y0");
             _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
            var gcode = $"G1 X0 Y0 F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";
             _machineRepo.CurrentMachine.SendCommand(gcode);           
            LocatorVM.SetLocatorState(MVLocatorState.Default);
        }
    }
}
