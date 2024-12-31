using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.Vision;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get { return _job; }
            set
            {
                _isDirty = false;                
                Set(ref _job, value);
                RaisePropertyChanged(nameof(HasJob));
            }
        }        

        public bool HasJob { get { return Job != null; } }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { Set(ref _isDirty, value); }
        }


        public StripFeederViewModel StripFeederVM
        {
            get;
        }


        public IJobInspectionViewModel JobInspectionVM { get; }
        public IPartsViewModel PartsVM { get; }
        public ToolAlignmentViewModel ToolAlignmentVM { get; }
        public ILocatorViewModel LocatorVM { get; }
        public INozzleTipCalibrationViewModel NozzleTipCalibrationVM { get; }
        public VisionProfileManagerViewModel VisionManagerVM { get; }
        public HomingViewModel HomingVM { get; }
        public FiducialViewModel FiducialVM { get; }



        private void RefreshCanExecute()
        {
            SaveCommand.RaiseCanExecuteChanged();            
        }

    }
}
