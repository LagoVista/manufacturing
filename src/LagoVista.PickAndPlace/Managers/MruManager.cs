using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public class MruManager : IMruManager
    {
        private readonly IStorageService _storageService;

        public MruManager(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public List<string> PnPJobs { get; private set; }

        public List<string> GCodeFiles { get; private set; }

        public List<string> BoardFiles { get; private set; }

        public List<string> ProjectFiles { get; private set; }

        public async Task LoadAsync()
        {
            var mru = await _storageService.GetAsync<MruManager>("mrus.json");
            if(mru != null)
            {
                PnPJobs = mru.PnPJobs;
                GCodeFiles = mru.GCodeFiles;
                BoardFiles = mru.BoardFiles;
                ProjectFiles = mru.ProjectFiles;
            }
            else
            {
                PnPJobs = new List<string>();
                GCodeFiles = new List<string>();
                BoardFiles = new List<string>();
                ProjectFiles = new List<string>();
            }
        }

        public async Task SaveAsync()
        {
            await _storageService.StoreAsync(this, "mrus.json");
        }
    }
}
