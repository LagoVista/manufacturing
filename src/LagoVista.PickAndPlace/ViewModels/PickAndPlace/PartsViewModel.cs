using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PartsViewModel : MachineViewModelBase, IPartsViewModel
    {
        PickAndPlaceJob _job;

        public PartsViewModel(IMachineRepo machineRepo, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, ILogger logger) : base(machineRepo)
        {
            AutoFeederViewModel = autoFeederViewModel ?? throw new ArgumentNullException(nameof(autoFeederViewModel));
            StripFeederViewModel = stripFeederViewModel ?? throw new ArgumentNullException(nameof(stripFeederViewModel));

            RefreshConfigurationPartsCommand = new RelayCommand(RefreshConfigurationParts);
        }
        

        PlaceableParts _currentPlaceableParts;
        public PlaceableParts CurrentPlaceableParts { get => _currentPlaceableParts; }

        Component _currentComponent;
        public Component CurrentComponent
        {
            get => _currentComponent;
        }

        ComponentPackage _currentComponentPackage;
        public ComponentPackage CurrentComponentPackage
        {
            get => _currentComponentPackage;
        }        

        public InvokeResult<PlaceableParts> ResolvePart(Manufacturing.Models.Component part)
        {
            return InvokeResult<PlaceableParts>.Create(null);
        }

        public ObservableCollection<PlaceableParts> ConfigurationParts { get; private set; } = new ObservableCollection<PlaceableParts>();


        Manufacturing.Models.Component _componentToBePlaced;
        public Manufacturing.Models.Component CurrentComponentToBePlaced { get => _componentToBePlaced; }
       
        public void RefreshConfigurationParts()
        {
            ConfigurationParts.Clear();
            var commonParts = _job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());

            foreach (var entry in commonParts)
            {
                var part = new PlaceableParts()
                {
                    Value = entry.First().Value.ToUpper(),
                    PackageId = entry.First().Package?.Id,
                    PackageName = entry.First().PackageName.ToUpper(),
                    ComponentName = entry.First().Component?.Text,
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

        public IStripFeederViewModel StripFeederViewModel { get; }
        public IAutoFeederViewModel AutoFeederViewModel { get; }

        public RelayCommand RefreshBoardCommand { get; }
        public RelayCommand RefreshConfigurationPartsCommand { get; }

    }
}
