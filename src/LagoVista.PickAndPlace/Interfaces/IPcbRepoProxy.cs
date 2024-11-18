using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPcbRepoProxy
    {
        Task<IEnumerable<CircuitBoard>> GetBoardsAsync();

        Task<CircuitBoard> GetBoardAsync(string id);
    }
}
