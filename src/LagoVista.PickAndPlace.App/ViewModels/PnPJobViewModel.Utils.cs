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
            LoadingMask = false;

            await ToolAlignmentVM.InitAsync();
            StartCapture();
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
            //Machine.SendCommand($"G92 X{Machine.Settings.MachineFiducial.X} Y{Machine.Settings.MachineFiducial.Y}");
            Machine.SendCommand($"G92 X0 Y0");
            Machine.SendCommand(SafeHeightGCodeGCode());
            var gcode = $"G1 X0 Y0 F{Machine.Settings.FastFeedRate}";
            Machine.SendCommand(gcode);           
            LocatorVM.LocatorState = MVLocatorState.Default;
        }
    }
}
