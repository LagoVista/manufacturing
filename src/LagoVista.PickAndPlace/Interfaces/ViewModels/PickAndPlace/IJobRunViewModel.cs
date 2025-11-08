// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7a9cf58c3e055b90e91bd82dd905347487575f019bfc5a34d433865d686a25ab
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IJobRunViewModel
    {
        Task<InvokeResult> CreateJobRunAsync();
        PickAndPlaceJobRun Current { get; }
        Task LoadJobRunAsync(string jobRunid);
        Task SaveJobRunAsync();
    }
}
