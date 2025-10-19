// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7d4d29dbd32f32dcae062b5e6d0ec480804332d6509fd6037065b1d9de0c75da
// IndexVersion: 0
// --- END CODE INDEX META ---
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
