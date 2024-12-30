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
             _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
             _machineRepo.CurrentMachine.GotoWorkspaceHome();
                        
            VisionManagerVM.SelectMVProfile("mchfiducual");
        }

        public void SetWorkComeViaVision()
        {
        }


        public void HomeViaOrigin()
        {
             _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
             _machineRepo.CurrentMachine.HomeViaOrigin();
            VisionManagerVM.SelectMVProfile("mchfiducual");
       }

        public void GoToPCBOrigin()
        {
             _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
             _machineRepo.CurrentMachine.GotoPoint( _machineRepo.CurrentMachine.Settings.PCBOffset.X,  _machineRepo.CurrentMachine.Settings.PCBOffset.Y,  _machineRepo.CurrentMachine.Settings.FastFeedRate);
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
