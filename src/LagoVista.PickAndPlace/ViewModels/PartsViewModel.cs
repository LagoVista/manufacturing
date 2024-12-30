using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class PartsViewModel : ViewModelBase, IPartsViewModel, INotifyPropertyChanged
    {
        StripFeederLocatorService _sfLocator;        
        ObservableCollection<StripFeeder> _stripFeeders;
        ObservableCollection<AutoFeeder> _autoFeeders;

        IRestClient _restClient;
        IMachine _machine;
        ILocatorViewModel _locatorViewModel;

        public PartsViewModel(IMachine machine, ILocatorViewModel locatorViewModel, IRestClient restClient)
        {
            _machine = machine;
            _restClient = restClient;
            _locatorViewModel = locatorViewModel;
            _sfLocator = new StripFeederLocatorService(machine.Settings);
        }

        public async new Task<InvokeResult> InitAsync()
        {
            var stripFeeders = await _restClient.GetListResponseAsync<StripFeeder>($"/api/mfg/machine/{_machine.Settings.Id}/autofeeders?loadcomponents=true");
            var autoFeeders = await _restClient.GetListResponseAsync<AutoFeeder>($"/api/mfg/machine/{_machine.Settings.Id}/stripfeeders?loadcomponents=true");

            _stripFeeders = new ObservableCollection<StripFeeder>(stripFeeders.Model);
            _autoFeeders = new ObservableCollection<AutoFeeder>(autoFeeders.Model);

            RaisePropertyChanged(nameof(StripFeeders));
            RaisePropertyChanged(nameof(AutoFeeders));

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

        public ObservableCollection<MachineStagingPlate> StagingPlates { get => _machine.Settings.StagingPlates; }
        public ObservableCollection<MachineFeederRail> FeederRails { get => _machine.Settings.FeederRails; }

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

    }
}
