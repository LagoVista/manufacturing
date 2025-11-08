// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a787b573d595d2cdfa9309e6e96c41edac468239d3df9c1687275462ae631ab0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPnpJobRepoProxy
    {
        Task<List<PickAndPlaceJob>> GetPickAndPlaceJobsAsync();
        Task<PickAndPlaceJob> GetPickAndPlaceJobAsync(string id);

    }
}
