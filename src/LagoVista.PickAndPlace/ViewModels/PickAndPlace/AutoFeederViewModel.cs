﻿using Emgu.CV.XImgproc;
using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class AutoFeederViewModel : FeederViewModel, IAutoFeederViewModel
    {

        private bool _isEding = false;
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

            AddCommand = CreatedCommand(Add, () => MachineRepo.HasValidMachine && SelectedTemplateId.HasValidId() && CurrentPhotonFeeder != null);
            SaveCommand = CreatedCommand(Save, () => Current != null);
        }

        private void PhotonFeederViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IPhotonFeederViewModel.SelectedPhotonFeeder))
            {
                CurrentPhotonFeeder = PhotonFeederViewModel.SelectedPhotonFeeder;
                RaiseCanExecuteChanged();
            }
        }

        private async void Add()
        {
            if(PhotonFeederViewModel.SelectedPhotonFeeder == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Please select a feeder before attempting to add it.");
                return;
            }

            var feederId = PhotonFeederViewModel.SelectedPhotonFeeder.Address;
            var slot = PhotonFeederViewModel.SelectedPhotonFeeder.Slot;

            var existingFeeder = _autoFeeders.FirstOrDefault(fdr => fdr.FeederId == feederId);
            if(existingFeeder != null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"A feeder with that ID alread exists {existingFeeder.Name}.");
                return;
            }

            var rail = MachineConfiguration.FeederRails.FirstOrDefault(fdr => slot >= fdr.SlotStartIndex && slot <= (fdr.SlotStartIndex + fdr.NumberSlots));

            if (rail == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Invalid slot index {slot}");
                return;
            }

            var result = await _restClient.GetAsync<DetailResponse<AutoFeeder>>($"/api/mfg/autofeeder/template/{SelectedTemplateId}/factory");
           
            if (result.Successful)
            {
                Current = result.Result.Model;
                Current.FeederId = feederId;
                Current.Rotation = rail.Rotation;
                Current.Slot = slot;
                Current.Key = $"addr{feederId.ToLower()}";
                Current.Machine = MachineConfiguration.ToEntityHeader();
                _isEding = false;
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        public override async Task InitAsync()
        {
            await LoadTemplates();
            await base.InitAsync();
        }

        private async Task LoadTemplates()
        {
            var result = await _restClient.GetListResponseAsync<AutoFeederTemplate>("/api/mfg/autofeeder/templates");
            if (result.Successful)
            {
                var feeders = result.Model.ToList();
                feeders.Insert(0, new AutoFeederTemplate() { Id = StringExtensions.NotSelectedId, Name = "-select feeder template-" });
                Templates = new ObservableCollection<AutoFeederTemplate>(feeders);
            }

            SelectedTemplateId = StringExtensions.NotSelectedId;
        }

        protected async override void MachineChanged(IMachine machine)
        {
            if (MachineRepo.HasValidMachine)
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
            if (String.IsNullOrEmpty(Current.Name))
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Please ensure you have a name for your auto feeder.");
                return;
            }

            var result = this._isEding ? await _restClient.PutAsync("/api/mfg/autofeeder", Current) : await _restClient.PostAsync("/api/mfg/autofeeder", Current);
            if(result.Successful)
            {
                _autoFeeders.Add(Current);
                Current = null;
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }


        private async Task LoadFeeders()
        {
            var autoFeeders = await _restClient.GetListResponseAsync<AutoFeeder>($"/api/mfg/machine/{MachineRepo.CurrentMachine.Settings.Id}/autofeeders?loadcomponents=true");
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
                _isEding = true;
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

        PhotonFeeder _currentPhotonFeeder;
        public PhotonFeeder CurrentPhotonFeeder
        {
            get => _currentPhotonFeeder;
            set => Set(ref _currentPhotonFeeder, value);
        }

        private string _selectedTemplateId = StringExtensions.NotSelectedId;
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
        
        public ObservableCollection<MachineFeederRail> FeederRails { get => MachineRepo.CurrentMachine.Settings.FeederRails; }
        public ObservableCollection<AutoFeeder> Feeders { get => _autoFeeders; }


        public IPhotonFeederViewModel PhotonFeederViewModel { get; }

        public RelayCommand CreateAutoFeederFromTemplateCommand { get; }

        public RelayCommand SetPartPickLocationCommand { get; }
        public RelayCommand SetFeederFiducialLocationCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand AdvancePartCommand { get; }
        public RelayCommand GoToPickLocationCommand { get; }
        public RelayCommand GoToFiducialCommand { get; }

        public RelayCommand AddCommand { get; }

        public RelayCommand CancelCommand { get; }

        public RelayCommand RefreshTemplatesCommand { get; }
    }
}
