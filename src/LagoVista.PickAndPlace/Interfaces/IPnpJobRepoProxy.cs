using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPnpJobRepoProxy
    {
        Task<List<PnPJob>> GetPickAndPlaceJobsAsync();
        Task<PnPJob> GetPickAndPlaceJobAsync(string id);

    }
}
