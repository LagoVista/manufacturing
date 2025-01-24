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
