using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class AutoFeederViewModel : FeederViewModel, IAutoFeederViewModel
    {

        private readonly IRestClient _restClient;
        ObservableCollection<AutoFeeder> _autoFeeders;

        public AutoFeederViewModel(IMachineRepo machineRepo, IPhotonFeederViewModel photonFeederViewModel, IRestClient restClient) : base(machineRepo, restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            PhotonFeederViewModel = photonFeederViewModel ?? throw new ArgumentNullException(nameof(photonFeederViewModel));
            PhotonFeederViewModel.PropertyChanged += PhotonFeederViewModel_PropertyChanged;

            CreateAutoFeederFromTemplateCommand = CreatedCommand(CreateAutoFeederFromTemplateAsync, () => !string.IsNullOrEmpty(SelectedTemplateId));
            SetFeederFiducialLocationCommand = CreatedMachineConnectedSettingsCommand(SetFeederFiducialLocationAsync, () => Current != null);
            SetPartPickLocationCommand = CreatedMachineConnectedSettingsCommand(SetPickLocationCommand, () => Current != null);

            GoToFiducialCommand = CreatedMachineConnectedCommand(GoToFeederFiducial, () => Current != null);
            GoToPickLocationCommand = CreatedMachineConnectedCommand(GoToPickLocation, () => Current != null);

            AdvancePartCommand = CreatedMachineConnectedCommand(AdvancePart, () => Current != null);

            SaveCommand = CreatedCommand(Save, () => Current != null);
        }

        private void PhotonFeederViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IPhotonFeederViewModel.SelectedPhotonFeeder))
            {

            }
        }

        protected async override void MachineChanged(IMachine machine)
        {
            if (_machineRepo.HasValidMachine)
            {
                await LoadFeeders();
            }
            base.MachineChanged(machine);
        }


        public void GoToFeederFiducial()
        {
            
        }

        public void AdvancePart()
        {

        }

        public void GoToPickLocation()
        {

        }

        public void SetFeederFiducialLocationAsync()
        {

        }

        public void SetPickLocationCommand()
        {

        }

        public async void CreateAutoFeederFromTemplateAsync()
        {
            var result = await _restClient.GetAsync<DetailResponse<AutoFeeder>>($"/api/mfg/autofeeder/template/{SelectedTemplateId}/factory");
            if (result.Successful)
            {
                Current = result.Result.Model;
            }
        }

        public async void Save()
        {
            await _restClient.PutAsync("/api/mfg/autofeeder", Current);
        }


        private async Task LoadFeeders()
        {
            var autoFeeders = await _restClient.GetListResponseAsync<AutoFeeder>($"/api/mfg/machine/{_machineRepo.CurrentMachine.Settings.Id}/autofeeders?loadcomponents=true");
            _autoFeeders = new ObservableCollection<AutoFeeder>(autoFeeders.Model);

            RaisePropertyChanged(nameof(Feeders));
        }

        AutoFeeder _current;
        public AutoFeeder Current
        {
            get => _current;
            set
            {
                Set(ref _current, value);
                RaiseCanExecuteChanged();
                if (value != null)
                {
                    _tapeSize = value.TapeSize;
                }
                else
                {
                    _tapeSize = null;
                }

                RaisePropertyChanged(nameof(TapeSize));
            }
        }

        private string _selectedTemplateId;
        public string SelectedTemplateId
        {
            get => _selectedTemplateId;
            set
            {
                Set(ref _selectedTemplateId, value);
                RaiseCanExecuteChanged();
            }
        }

        ObservableCollection<AutoFeederTemplate> _templates;
        public ObservableCollection<AutoFeederTemplate> Templates 
        {
            get => _templates;
            private set => Set(ref _templates, value);
        } 
        
        public ObservableCollection<MachineFeederRail> FeederRails { get => _machineRepo.CurrentMachine.Settings.FeederRails; }
        public ObservableCollection<AutoFeeder> Feeders { get => _autoFeeders; }


        public IPhotonFeederViewModel PhotonFeederViewModel { get; }

        public RelayCommand CreateAutoFeederFromTemplateCommand { get; }

        public RelayCommand SetPartPickLocationCommand { get; }
        public RelayCommand SetFeederFiducialLocationCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand AdvancePartCommand { get; }
        public RelayCommand GoToPickLocationCommand { get; }
        public RelayCommand GoToFiducialCommand { get; }
    }
}
