// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ed2779916eb40df2cd5b6def549d418009b0020286e21acb727c79c6baecafb4
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PartsViewModel : MachineViewModelBase, IPartsViewModel
    {
        public PartsViewModel(IMachineRepo machineRepo, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, ILogger logger) : base(machineRepo)
        {
            AutoFeederViewModel = autoFeederViewModel ?? throw new ArgumentNullException(nameof(autoFeederViewModel));
            StripFeederViewModel = stripFeederViewModel ?? throw new ArgumentNullException(nameof(stripFeederViewModel));
            RefreshAvailablePartsCommand = CreateCommand(RefreshAvailableParts);
        }        

        public void RefreshAvailableParts()
        {
            AvailableParts.Clear();

            foreach(var af in AutoFeederViewModel.Feeders)
            {
                if(af.Component != null)
                {
                    var ap = new AvailablePart()
                    {
                        AutoFeeder = af.ToEntityHeader(),
                        Component = af.Component,
                        AvailableCount = af.PartCount,
                    };

                    AvailableParts.Add(ap);
                }
            }

            foreach(var sf in StripFeederViewModel.Feeders)
            {
                foreach(var row in sf.Rows)
                {
                    if(row.Component != null)
                    {
                        var ap = new AvailablePart()
                        {
                            StripFeeder = sf.ToEntityHeader(),
                            StripFeederRow = row.ToEntityHeader(),
                            Component = row.Component,
                            AvailableCount = row.PartCapacity,
                        };

                        AvailableParts.Add(ap);
                    }
                }
            }
        }       

        //Manufacturing.Models.Component _componentToBePlaced;
        //public Manufacturing.Models.Component CurrentComponentToBePlaced { get => _componentToBePlaced; }
       

        public ObservableCollection<AvailablePart> AvailableParts { get; } = new ObservableCollection<AvailablePart>();

        public IStripFeederViewModel StripFeederViewModel { get; }
        public IAutoFeederViewModel AutoFeederViewModel { get; }

        public RelayCommand RefreshAvailablePartsCommand { get; }
    }

    public class AvailablePart : ModelBase
    {
        EntityHeader _component;
        public EntityHeader Component
        { 
            get => _component;
            set => Set(ref _component, value);
        }

        EntityHeader _autoFeeder;
        public EntityHeader AutoFeeder 
        {
            get => _autoFeeder;
            set => Set(ref _autoFeeder, value);
        }

        EntityHeader _stripFeeder;
        public EntityHeader StripFeeder 
        {
            get => _stripFeeder;
            set => Set(ref _stripFeeder, value);
        }


        EntityHeader _stripFeederRow;
        public EntityHeader StripFeederRow 
        {
            get => _stripFeederRow;
            set => Set(ref _stripFeederRow, value);
        }

        int _availableCount;
        public int AvailableCount
        { 
            get => _availableCount;
            set => Set(ref _availableCount, value);
        }
    }
}
