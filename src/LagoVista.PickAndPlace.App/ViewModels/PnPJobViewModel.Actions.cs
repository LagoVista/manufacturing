using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {

        public async Task SaveJobAsync()
        {
            var result = await _restClient.PutAsync("/api/mfg/pnpjob", _job);
            if (result.Successful)
            {
                SaveCommand.RaiseCanExecuteChanged();
                _isDirty = false;
            }
        }


        
        public void GotoWorkspaceHome()
        {
            _machine.SendCommand(SafeHeightGCodeGCode());
            _machine.GotoWorkspaceHome();
                        
            VisionManagerVM.SelectMVProfile("mchfiducual");
        }

        public void SetWorkComeViaVision()
        {
        }


        public void HomeViaOrigin()
        {
            _machine.SendCommand(SafeHeightGCodeGCode());
            _machine.HomeViaOrigin();
            VisionManagerVM.SelectMVProfile("mchfiducual");
       }

        public void GoToPCBOrigin()
        {
            _machine.SendCommand(SafeHeightGCodeGCode());
            _machine.GotoPoint(_machine.Settings.PCBOffset.X, _machine.Settings.PCBOffset.Y, _machine.Settings.FastFeedRate);
            VisionManagerVM.SelectMVProfile("brdfiducual");            
        }

        public async void Close()
        {
            await SaveJobAsync();
            CloseScreen();
        }

        public async void PrintManualPlace()
        {
            //var manualParts = Job SelectedBuildFlavor.Components.Where(prt => prt.ManualPlace);
            //var bldr = new StringBuilder();
            //foreach (var manualPart in manualParts)
            //    bldr.AppendLine($"{manualPart.Name}\t\t{manualPart.Value}");

            //var file = await Popups.ShowSaveFileAsync("txt");
            //if(!String.IsNullOrEmpty(file))
            //{
            //    System.IO.File.WriteAllText(file, bldr.ToString());
            //}
        }
    }
}
