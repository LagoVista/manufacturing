using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace
{
    public class MachinesRepo : IMachiensRepo
    {
        IRestClient _restClient;

        public MachinesRepo()
        {
            _restClient = SLWIOC.Get<IRestClient>();
        }

        public const string FileName = "Machines.json";

        public string CurrentMachineId { get; set; }
        public List<LagoVista.Manufacturing.Models.MachineSummary> Machines { get; set; }


        public async static Task<MachinesRepo> LoadAsync()
        {
            try
            {
                var machines = await Services.Storage.GetAsync<MachinesRepo>(MachinesRepo.FileName);

                if (machines == null)
                {
                    machines = MachinesRepo.Default;
                }

                return machines;
            }
            catch (Exception)
            {
                return MachinesRepo.Default;
            }
        }


        public async Task SaveAsync()
        {
            await Services.Storage.StoreAsync(this, MachinesRepo.FileName);
        }

    }
}
