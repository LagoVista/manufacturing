using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class JobInspectionViewModel : ViewModelBase, IJobInspectionViewModel
    {
        private int _inspectIndex;
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;
        private readonly ILocatorViewModel _locatorViewMoel;

        public JobInspectionViewModel(IMachineRepo machineRepo, ILogger logger, ILocatorViewModel locatorViewModel)
        {
            _locatorViewMoel = locatorViewModel;
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            NextInspectCommand = new RelayCommand(NextInspect, () => _inspectIndex < -1);
            PrevInspectCommand = new RelayCommand(PrevInspect, () => _inspectIndex > 0);
            FirstInspectCommand = new RelayCommand(InspectFirst, () => _inspectIndex > 0);
        }

        PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get => _job;
            set
            {
                Set(ref _job, value);
            }
        }

        PlaceableParts _current;
        public PlaceableParts Current
        {
            get => _current;
            set => Set(ref _current, value);
        }


        private void NextInspect()
        {
            //if (_selectedInspectPart == null)
            //{
            //    _selectedInspectPart = ConfigurationParts[InspectIndex];
            //}

            //if (SelectedInspectPart.Parts.Count == 1 || _isOnLast)
            //{
            //    if (_inspectIndex < ConfigurationParts.Count - 1)
            //    {
            //        InspectIndex++;
            //        _selectedInspectPart = ConfigurationParts[InspectIndex];
            //        RaisePropertyChanged(nameof(SelectedInspectPart));
            //        NextInspectCommand.RaiseCanExecuteChanged();
            //        PrevInspectCommand.RaiseCanExecuteChanged();
            //        FirstInspectCommand.RaiseCanExecuteChanged();

            //        GoToFirstPartInPartsToPlace();
            //    }
            //}
            //else
            //{
            //    GoToLastPartInPartsToPlace();
            //}
        }

        private void PrevInspect()
        {
            //if (_isOnLast)
            //{
            //    GoToFirstPartInPartsToPlace();
            //}
            //else
            //{
            //    if (InspectIndex > 0)
            //    {
            //        InspectIndex--;
            //        NextInspectCommand.RaiseCanExecuteChanged();
            //        PrevInspectCommand.RaiseCanExecuteChanged();
            //        FirstInspectCommand.RaiseCanExecuteChanged();

            //        _selectedInspectPart = ConfigurationParts[InspectIndex];
            //        RaisePropertyChanged(nameof(SelectedInspectPart));
            //        if (SelectedInspectPart.Parts.Count > 1)
            //        {
            //            GoToLastPartInPartsToPlace();
            //        }
            //        else
            //        {
            //            GoToFirstPartInPartsToPlace();
            //        }
            //    }
            //}
        }

        private void InspectFirst()
        {
            //InspectIndex = 0;
            //_selectedInspectPart = ConfigurationParts[InspectIndex];
            //RaisePropertyChanged(nameof(SelectedInspectPart));
            //GoToFirstPartInPartsToPlace();

        }

        PlaceableParts _selectedInspectPart;
        public PlaceableParts SelectedInspectPart
        {
            get => _selectedInspectPart;
            set
            {
                Set(ref _selectedInspectPart, value);
                //if (value != null)
                //{
                //    _inspectIndex = ConfigurationParts.IndexOf(value);
                //    GoToInspectPartRefHoleCommand.RaiseCanExecuteChanged();
                //    SetInspectPartRefHoleCommand.RaiseCanExecuteChanged();
                //    GoToInspectedPartCommand.RaiseCanExecuteChanged();
                //    GoToInspectPartRefHole();
                //}
            }
        }

        private string _status;
        public string Status
        {
            set => Set(ref _status, value);
            get => _status;
        }

        public int InspectIndex
        {
            get => _inspectIndex;
            set => Set(ref _inspectIndex, value);
        }

        private string _inspectMessage;
        public string InspectMessage
        {
            get => _inspectMessage;
            set => Set(ref _inspectMessage, value);
        }


        private void RefreshCanExecute()
        {
            PrevInspectCommand.RaiseCanExecuteChanged();
            NextInspectCommand.RaiseCanExecuteChanged();
            FirstInspectCommand.RaiseCanExecuteChanged();

        }


        public RelayCommand FirstInspectCommand { get; private set; }
        public RelayCommand NextInspectCommand { get; private set; }
        public RelayCommand PrevInspectCommand { get; private set; }

        public RelayCommand GoToInspectPartRefHoleCommand { get; private set; }
        public RelayCommand SetInspectPartRefHoleCommand { get; private set; }
        public RelayCommand GoToInspectedPartCommand { get; private set; }

    }
}
