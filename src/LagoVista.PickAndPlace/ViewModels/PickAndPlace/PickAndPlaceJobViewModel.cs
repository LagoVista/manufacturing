using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PickAndPlaceJobViewModel : ViewModelBase, IPickAndPlaceJobViewModel
    {
        private readonly IRestClient _restClient;
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        private readonly IPartsViewModel _partsViewModel;

        public PickAndPlaceJobViewModel(IRestClient restClient, IStorageService storage, ILogger logger, IPartsViewModel partsViewModel)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageService = storage ?? throw new ArgumentNullException(nameof(storage));
            _partsViewModel = partsViewModel ?? throw new ArgumentNullException(nameof(partsViewModel));
            ReloadJobCommand = new RelayCommand(async () => await RefreshJob(), () => { return Job != null; });
            RefreshConfigurationPartsCommand = new RelayCommand(RefreshConfigurationParts);
            SaveCommand = new RelayCommand(async () => await SaveJobAsync(), () => { return Job != null; });
            _partsViewModel = partsViewModel;
        }

        public override async Task InitAsync()
        {
            var result = await _restClient.GetListResponseAsync<PickAndPlaceJobSummary>("/api/mfg/pnpjobs");
            if (result.Successful)
            {
                Jobs = new ObservableCollection<PickAndPlaceJobSummary>(result.Model.ToList());
                var lastJobId = await _storageService.GetKVPAsync<string>("last-job-id");
                if (!String.IsNullOrEmpty(lastJobId))
                {
                    await LoadJobAsync(lastJobId);
                }
            }

            await base.InitAsync();
        }

        private async Task SaveJobAsync()
        {
            await _restClient.PutAsync("/api/mfg/pnpjob", Job);
        }

        private async Task RefreshJob()
        {
            await LoadJobAsync(this.Job.Id);
        }

        private async Task LoadJobAsync(string jobId)
        {
            var jobLoadReslut = await _restClient.GetAsync<DetailResponse<PickAndPlaceJob>>($"/api/mfg/pnpjob/{jobId}");
            if (jobLoadReslut.Successful)
            {
                Job = jobLoadReslut.Result.Model;
                Board = Job.BoardRevision;
                await _storageService.StoreKVP<string>("last-job-id", jobId);
                RefreshConfigurationParts();
            }
        }

        public void RefreshConfigurationParts()
        {
            ConfigurationParts.Clear();
            var commonParts = _job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());

            foreach (var entry in commonParts)
            {
                var part = new PlaceableParts()
                {
                    Value = entry.First().Value.ToUpper(),
                    PackageName = entry.First().PackageName.ToUpper(),
                };

                part.Parts = new ObservableCollection<PcbComponent>();

                foreach (var specificPart in entry)
                {
                    var placedPart = _job.BoardRevision.PcbComponents.Where(cmp => cmp.Name == specificPart.Name && cmp.Key == specificPart.Key).FirstOrDefault();
                    if (placedPart != null)
                    {
                        part.Parts.Add(placedPart);
                    }
                }

                ConfigurationParts.Add(part);
            }
        }

        public ObservableCollection<PlaceableParts> ConfigurationParts { get; private set; } = new ObservableCollection<PlaceableParts>();

        PlaceableParts _placeablePart;
        public PlaceableParts PlaceablePart
        {
            get => _placeablePart;
            set => Set(ref _placeablePart, value);
        }

        private async void LoadJob(string jobId)
        {
            await LoadJobAsync(jobId);
        }

        ObservableCollection<PickAndPlaceJobSummary> _jobs;
        public ObservableCollection<PickAndPlaceJobSummary> Jobs
        {
            get => _jobs;
            set => Set(ref _jobs, value);
        }

        private PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get => _job;
            set => Set(ref _job, value);
        }

        PickAndPlaceJobSummary _selectedJob;
        public PickAndPlaceJobSummary SelectedJob
        {
            get => _selectedJob;
            set
            {                
                RaisePropertyChanged(nameof(SelectedJob));
                if(value != null && _selectedJob?.Id != value?.Id)
                {
                    LoadJob(value?.Id);
                }

                Set(ref _selectedJob, value);
            }        
        }

        CircuitBoardRevision _board;
        public CircuitBoardRevision Board
        {
            get => _board;
            set => Set(ref _board, value);
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand ReloadJobCommand { get; }
        public RelayCommand RefreshConfigurationPartsCommand { get; }

    }
}
