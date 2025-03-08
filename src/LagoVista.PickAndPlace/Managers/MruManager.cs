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
            LoadAsync();
        }


        private async void LoadAsync()
        {
            Files = await _storageService.GetAsync<MruFiles>("mrus.json");
        }

        public MruFiles Files { get; set; }

        public async Task SaveAsync()
        {
            await _storageService.StoreAsync(this.Files, "mrus.json");
        }
    }

    public class MruFiles
    {
        public List<string> PnPJobs { get; set; }

        public List<string> GCodeFiles { get; set; }

        public List<string> BoardFiles { get; set; }

        public List<string> ProjectFiles { get; set; }

    }
}
