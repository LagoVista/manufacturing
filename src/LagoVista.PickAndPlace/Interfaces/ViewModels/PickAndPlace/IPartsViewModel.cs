using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPartsViewModel : IMachineViewModelBase
    {
        public IStripFeederViewModel StripFeederViewModel { get; }
        public IAutoFeederViewModel AutoFeederViewModel { get; }
    }
}
