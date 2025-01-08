using LagoVista.Client.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public abstract class FeederViewModel : MachineViewModelBase, IFeederViewModel
    {

        private readonly IRestClient _restClient;

        protected FeederViewModel(IMachineRepo machineRepo, IRestClient resteClient) : base(machineRepo)
        {
            _restClient = resteClient ?? throw new ArgumentNullException(nameof(resteClient));
        }

        public override async Task InitAsync()
        {
            var componentCategories = await _restClient.GetListResponseAsync<EntityBase>("/api/categories/component");
            ComponentCategories = new ObservableCollection<EntityHeader>(componentCategories.Model.Select(c => EntityHeader.Create(c.Id, c.Key, c.Name)));

            await base.InitAsync();
        }

        public async void LoadComponent(string componentId)
        {
            var component = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Component>>($"/api/mfg/component/{componentId}");
            SelectedComponent = component.Result.Model;
        }

        public async void LoadComponentsByCategory(string categoryKey)
        {
            var components = await _restClient.GetListResponseAsync<ComponentSummary>($"/api/mfg/components?componentType={categoryKey}");
            Components = new ObservableCollection<ComponentSummary>(components.Model);
        }


        ObservableCollection<EntityHeader> _componentCategories;
        public ObservableCollection<EntityHeader> ComponentCategories
        {
            get => _componentCategories;
            set => Set(ref _componentCategories, value);
        }

        string _selectedCategoryKey;
        public string SelectedCategoryKey
        {
            get => _selectedCategoryKey;
            set
            {
                if (value == _selectedCategoryKey) return;
                {
                    Set(ref _selectedCategoryKey, value);
                    if (!string.IsNullOrEmpty(value))
                        LoadComponentsByCategory(value);
                }
            }
        }

        string _selectedComponentSummaryId;
        public string SelectedComponentSummaryId
        {
            get => _selectedComponentSummaryId;
            set
            {
                if (value != _selectedComponentSummaryId)
                {
                    Set(ref _selectedComponentSummaryId, value);
                    if (!string.IsNullOrEmpty(value))
                        LoadComponent(_selectedComponentSummaryId);
                    else
                        SelectedComponent = null;
                }
            }
        }
   
        Manufacturing.Models.Component _selectedComponent;
        public Manufacturing.Models.Component SelectedComponent
        {
            get => _selectedComponent;
            set => Set(ref _selectedComponent, value);
        }


        ObservableCollection<ComponentSummary> _components;
        public ObservableCollection<ComponentSummary> Components
        {
            get => _components;
            set => Set(ref _components, value);
        }


        Component _currentComponent;
        public Component CurrentComponent
        {
            get => _currentComponent;
            set => Set(ref _currentComponent, value);
        }

        ComponentPackage _currentComponentPackage;       
        public ComponentPackage CurrentComponentPackage
        {
            get => _currentComponentPackage;
            set => Set(ref _currentComponentPackage, value);
        }


        protected EntityHeader<TapeSizes> _tapeSize;
        public EntityHeader<TapeSizes> TapeSize
        {
            get => _tapeSize;
        }

        protected EntityHeader<TapePitches> _tapePitch;
        public EntityHeader<TapePitches> TapePitch
        {
            get => _tapePitch;
        }


        private bool _useCalculted;
        public bool UseCalculated
        {
            get => _useCalculted;
            set => Set(ref _useCalculted, value);
        }


    }
}
