using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Core.Managers;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.MediaServices.Interfaces;
using System.Xml.Linq;
using System.IO;
using System.Linq;

namespace LagoVista.Manufacturing.Managers
{
    public class CircuitBoardManager : ManagerBase, ICircuitBoardManager
    {
        private readonly ICircuitBoardRepo _circuitBoardRepo;
        private readonly IMediaServicesManager _mediaServicesManager;

        public CircuitBoardManager(ICircuitBoardRepo circuitBoardRepo, IMediaServicesManager mediaServicesManager,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _circuitBoardRepo = circuitBoardRepo ?? throw new ArgumentNullException(nameof(circuitBoardRepo));
            _mediaServicesManager = mediaServicesManager ?? throw new ArgumentNullException(nameof(mediaServicesManager));
        }
        public async Task<InvokeResult> AddCircuitBoardAsync(CircuitBoard pcb, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(pcb, AuthorizeActions.Create, user, org);
            ValidationCheck(pcb, Actions.Create);            

            foreach (var revision in pcb.Revisions)
            {
                if (!EntityHeader.IsNullOrEmpty(revision.BoardFile) && !revision.PcbComponents.Any())
                {
                    var result = await PopulateComponents(revision, org, user);
                    if (!result.Successful)
                    {
                        return result.ToInvokeResult();
                    }
                }

                if (revision.Variants.Count == 0)
                {
                    revision.Variants.Add(new CircuitBoardVariant()
                    {
                        PartName = "Default",
                        PcbComponents = revision.PcbComponents
                    });
                }
            }

            await _circuitBoardRepo.AddCircuitBoardAsync(pcb);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _circuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult<CircuitBoardRevision>> PopulateComponents(CircuitBoardRevision revistion, EntityHeader org, EntityHeader user)
        {
            var media = await _mediaServicesManager.GetResourceMediaAsync(revistion.BoardFile.Id, org, user);
            using (var ms = new MemoryStream(media.ImageBytes))
            {
                var doc = XDocument.Load(ms);
                var pcb = EagleParser.ReadPCB(doc);
                revistion.PcbComponents = pcb.Components;
                revistion.Width = pcb.Width;
                revistion.Height = pcb.Height;
            }

            return InvokeResult<CircuitBoardRevision>.Create(revistion);
        }

        public async Task<InvokeResult> DeleteCircuitBoardAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _circuitBoardRepo.GetCircuitBoardAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _circuitBoardRepo.DeleteCircuitBoardAsync(id);
            return InvokeResult.Success;
        }

        public async Task<CircuitBoard> GetCircuitBoardAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _circuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }

        public async Task<ListResponse<CircuitBoardSummary>> GetCircuitBoardsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(CircuitBoard));
            return await _circuitBoardRepo.GetCircuitBoardSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateCircuitBoardAsync(CircuitBoard pcb, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(pcb, AuthorizeActions.Update, user, org);
            ValidationCheck(pcb, Actions.Update);

            foreach (var revision in pcb.Revisions)
            {
                if (!EntityHeader.IsNullOrEmpty(revision.BoardFile) && !revision.PcbComponents.Any())
                {
                    var result = await PopulateComponents(revision, org, user);
                    if (!result.Successful)
                    {
                        return result.ToInvokeResult();
                    }
                }

                if (revision.Variants.Count == 0)
                {
                    revision.Variants.Add(new CircuitBoardVariant()
                    {
                        PartName = "Default",
                        PcbComponents = revision.PcbComponents
                    });
                }
            }

            await _circuitBoardRepo.UpdateCircuitBoardAsync(pcb);
            return InvokeResult.Success;
        }
    }
}