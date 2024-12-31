using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PartsViewModel : ViewModelBase, IPartsViewModel, INotifyPropertyChanged
    {
        StripFeederLocatorService _sfLocator;        
        ObservableCollection<StripFeeder> _stripFeeders;
        ObservableCollection<AutoFeeder> _autoFeeders;

        private readonly IRestClient _restClient;
        private readonly IMachineRepo  _machineRepo;
        private readonly ILogger _logger;
        private readonly ILocatorViewModel _locatorViewModel;

        PickAndPlaceJob _job;

        public PartsViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, IRestClient restClient, ILogger logger)
        {
             _machineRepo = machineRepo;
            _machineRepo.MachineChanged += _machineRepo_MachineChanged;
            _restClient = restClient;
            _locatorViewModel = locatorViewModel;
            _sfLocator = new StripFeederLocatorService(machineRepo.CurrentMachine.Settings);
            _logger = logger;

            RefreshBoardCommand = new RelayCommand(() => RefreshAsync());
            RefreshConfigurationPartsCommand = new RelayCommand(RefreshConfigurationParts);
        }

        private async void _machineRepo_MachineChanged(object sender, IMachine e)
        {
            if (e.Settings.Id != Machine?.Settings.Id)
            {
                Machine = e;
                await InitAsync();
            }
        }

        public async new Task<InvokeResult> InitAsync()
        {
            if (_machineRepo.HasValidMachine)
            {
                var autoFeeders = await _restClient.GetListResponseAsync<AutoFeeder>($"/api/mfg/machine/{_machineRepo.CurrentMachine.Settings.Id}/autofeeders?loadcomponents=true");
                var stripFeeders = await _restClient.GetListResponseAsync<StripFeeder>($"/api/mfg/machine/{_machineRepo.CurrentMachine.Settings.Id}/stripfeeders?loadcomponents=true");

                _stripFeeders = new ObservableCollection<StripFeeder>(stripFeeders.Model);
                _autoFeeders = new ObservableCollection<AutoFeeder>(autoFeeders.Model);

                RaisePropertyChanged(nameof(StripFeeders));
                RaisePropertyChanged(nameof(AutoFeeders));
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> SaveCurrentFeederAsync()
        {
            if (_currentAutoFeeder != null)
                return await _restClient.PutAsync("/api/mfg/stripfeeder", CurrentStripFeeder);
            else if (_currentAutoFeeder != null)
                return await _restClient.PutAsync("/api/mfg/autofeeder", CurrentAutoFeeder);

            return InvokeResult.FromError("No current feeder.");
        }

        public Task<InvokeResult> RefreshAsync()
        {
            return InitAsync();
        }

        public ObservableCollection<MachineStagingPlate> StagingPlates { get =>  _machineRepo.CurrentMachine.Settings.StagingPlates; }
        public ObservableCollection<MachineFeederRail> FeederRails { get =>  _machineRepo.CurrentMachine.Settings.FeederRails; }

        public ObservableCollection<StripFeeder> StripFeeders { get => _stripFeeders; }
        public ObservableCollection<AutoFeeder> AutoFeeders { get => _autoFeeders; }

        public InvokeResult<PlaceableParts> ResolvePart(Manufacturing.Models.Component part)
        {
            return InvokeResult<PlaceableParts>.Create(null);
        }

        StripFeeder _currentStripFeeder;
        public StripFeeder CurrentStripFeeder { get => _currentStripFeeder; }

        AutoFeeder _currentAutoFeeder;
        public AutoFeeder CurrentAutoFeeder { get => _currentAutoFeeder; }

        public ObservableCollection<PlaceableParts> ConfigurationParts { get; private set; } = new ObservableCollection<PlaceableParts>();


        ComponentPackage _currentPackage;
        public ComponentPackage CurrentPackage { get => _currentPackage; }

        PlaceableParts _currentPlaceableParts;
        public PlaceableParts CurrentPlaceableParts { get => _currentPlaceableParts; }

        Manufacturing.Models.Component _componentToBePlaced;
        public Manufacturing.Models.Component CurrentComponentToBePlaced { get => _componentToBePlaced; }

       
        public bool CanMoveToNextInTape()
        {
            return false;
        }

        public bool CanMoveToPrevInTape()
        {
            return false;
        }
        public bool CanMoveToReferenceHoleInTape()
        {
            return false;
        }

        public bool CanMoveToFirstPartInTape()
        {
            return false;
        }

        public bool CanMoveToCurrentPartInTape()
        {
            return false;
        }

        private IMachine _machine;
        public IMachine Machine
        {
            get => _machine;
            set => Set(ref _machine, value);
        }

        public void RefreshConfigurationParts()
        {
            ConfigurationParts.Clear();
            var commonParts = _job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());

            foreach (var entry in commonParts)
            {
                var part = new PlaceableParts()
                {
                    Value = entry.First().Value.ToUpper(),
                    PackageName = entry.First().PackageName.ToUpper(),
                };

                part.Parts = new ObservableCollection<PcbComponent>();

                foreach (var specificPart in entry)
                {
                    var placedPart = _job.BoardRevision.PcbComponents.Where(cmp => cmp.Name == specificPart.Name && cmp.Key == specificPart.Key).FirstOrDefault();
                    if (placedPart != null)
                    {
                        part.Parts.Add(placedPart);
                    }
                }

                ConfigurationParts.Add(part);
            }
        }

        public RelayCommand RefreshBoardCommand { get; }
        public RelayCommand RefreshConfigurationPartsCommand { get; }

    }
}
