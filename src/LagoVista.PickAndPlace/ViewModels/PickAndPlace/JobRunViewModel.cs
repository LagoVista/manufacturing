// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 863960b5ae4610f5d4342011629491c3c35a48c846457fafdfbb676abbca1b34
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class JobRunViewModel : MachineViewModelBase, IJobRunViewModel
    {
        IRestClient _restClient;
        IJobManagementViewModel _jobViewModel;

        public JobRunViewModel(IMachineRepo machineRepo, IRestClient restClient, IJobManagementViewModel jobViewModel) : base(machineRepo)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _jobViewModel = jobViewModel ?? throw new ArgumentNullException(nameof(jobViewModel));
        }

        public async Task<InvokeResult> CreateJobRunAsync()
        {
            if(_jobViewModel.Job == null)
            {
                return InvokeResult.FromError("Job is required.");
            }

            var jobRun = new PickAndPlaceJobRun()
            {
                Name = _jobViewModel.Job.Name,
                OwnerOrganization = _jobViewModel.Job.OwnerOrganization,
                Job = _jobViewModel.Job.ToEntityHeader(),
                Cost = _jobViewModel.Job.Cost,
                Extended = _jobViewModel.Job.Extended,
            };

            Current = jobRun;

            await _restClient.PostAsync("/api/mfg/pnpjob/run", jobRun);

            return InvokeResult.Success;
        }

        public async Task LoadJobRunAsync(string jobRunid)
        {
            var response = await _restClient.GetAsync<DetailResponse<PickAndPlaceJobRun>>($"/api/mfg/pnpjob/{jobRunid}");
            if (response.Successful)
                Current = response.Result.Model;
        }

        public async Task SaveJobRunAsync()
        {
            await _restClient.PutAsync("/api/mfg/pnpjob/run", Current);
        }

        PickAndPlaceJobRun _current;
        public PickAndPlaceJobRun Current
        {
            get => _current;
            set => Set(ref _current, value);
        }
    }
}
