using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IMachineRepo : IViewModel
    {
        IMachine CurrentMachine { get; }
        Task<List<LagoVista.Manufacturing.Models.MachineSummary>> GetMachinesAsync();
        Task<InvokeResult> LoadCurrentMachineAsync();
        Task<InvokeResult> LoadMachineAsync(string id);
        Task<InvokeResult> SaveCurrentMachineAsync();
        bool HasValidMachine { get; }

        MachineSummary SelectedMachine { get; set; }
    }
}
