using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IMruManager
    {
        List<String> PnPJobs { get; }
        List<String> GCodeFiles { get; }
        List<String> BoardFiles { get; }
        List<String> ProjectFiles { get;  }

        Task SaveAsync();

        Task LoadAsync();
    }
}
