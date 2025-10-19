// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6291217c49fbf6d404e33aa98e263a7d83f2a4c4b3243fd93b4843348c08dcfb
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab
{
    public interface IPcbRepoProxy
    {
        Task<IEnumerable<CircuitBoard>> GetBoardsAsync();

        Task<CircuitBoard> GetBoardAsync(string id);
    }
}
