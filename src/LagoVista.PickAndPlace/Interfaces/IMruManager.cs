using LagoVista.PickAndPlace.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IMruManager
    {
        MruFiles Files { get; }

        Task SaveAsync();        
    }
}
