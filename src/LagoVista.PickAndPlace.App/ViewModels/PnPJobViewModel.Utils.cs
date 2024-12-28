using LagoVista.PCB.Eagle.Models;
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

            foreach(var partByValue in SelectedBuildFlavor.Components.Where(prt=>prt.Included).GroupBy(prt=>prt.Value))
            {
                var value = partByValue.Key;
                bldr.Append($"{value}\t");
                foreach(var part in partByValue)
                    bldr.Append($"{part.Name}, ");

                bldr.AppendLine();
           }

            System.IO.File.WriteAllText(@$"X:\PartList {SelectedBuildFlavor.Name}.txt", bldr.ToString());
        }

        private void PopulateConfigurationParts()
        {
            ConfigurationParts.Clear();
            var commonParts = Job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());

            foreach (var entry in commonParts)
            {
                var part = new PlaceableParts()
                {
                    Count = entry.Count(),
                    Value = entry.First().Value.ToUpper(),
                    Package = entry.First().PackageName.ToUpper(),

                };

                part.Parts = new ObservableCollection<PcbComponent>();

                foreach (var specificPart in entry)
                {
                    var placedPart = Job.BoardRevision.PcbComponents.Where(cmp => cmp.Name == specificPart.Name && cmp.Key == specificPart.Key).FirstOrDefault();
                    if (placedPart != null)
                    {
                        part.Parts.Add(placedPart);
                    }
                }

                ConfigurationParts.Add(part);
            }

            if (_pnpMachine != null)
            {
                foreach (var part in ConfigurationParts)
                {
                    StripFeederVM.ResolvePart(part);
                }
            }

            InspectIndex = 0;
            PrevInspectCommand.RaiseCanExecuteChanged();
            NextInspectCommand.RaiseCanExecuteChanged();
            FirstInspectCommand.RaiseCanExecuteChanged();
        }

        private void SetNewHome()
        {
            //Machine.SendCommand($"G92 X{Machine.Settings.MachineFiducial.X} Y{Machine.Settings.MachineFiducial.Y}");
            Machine.SendCommand($"G92 X0 Y0");
            Machine.SendCommand(SafeHeightGCodeGCode());
            var gcode = $"G1 X0 Y0 F{Machine.Settings.FastFeedRate}";
            Machine.SendCommand(gcode);           
            LocatorState = MVLocatorState.Default;
        }
    }
}
