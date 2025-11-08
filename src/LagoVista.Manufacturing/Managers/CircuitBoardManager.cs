// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3a0f42a84c8e8de4683bf265b83ce0797644e6d389b5ff667e1c627614ae6cca
// IndexVersion: 2
// --- END CODE INDEX META ---
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
            }

            await _circuitBoardRepo.AddCircuitBoardAsync(pcb);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var pcb = await _circuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(pcb, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(pcb);
        }

        public async Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            var pcb = await _circuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(pcb, AuthorizeActions.Delete, user, org);
            await _circuitBoardRepo.DeleteCircuitBoardAsync(id);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<CircuitBoardRevision>> PopulateComponents(CircuitBoardRevision revision, EntityHeader org, EntityHeader user)
        {
            var media = await _mediaServicesManager.GetResourceMediaAsync(revision.BoardFile.Id, org, user);
            using (var ms = new MemoryStream(media.ImageBytes))
            {
                if (media.FileName.EndsWith("brd"))
                {
                    var doc = XDocument.Load(ms);
                    var pcb = EagleParser.ReadPCB(doc);
                    revision.PcbComponents = pcb.Components;
                    revision.PhysicalPackages = pcb.Packages;
                    revision.Width = pcb.Width;
                    revision.Height = pcb.Height;
                    revision.Outline = pcb.Outline;
                    revision.TopWires = pcb.TopWires;
                    revision.BottomWires = pcb.BottomWires;
                    revision.Vias = pcb.Vias;
                    revision.Holes = pcb.Holes;
                }
                else if(media.FileName.EndsWith("kicad_pcb"))
                {
                    var pcb = KicadImport.ImportPCB(ms);
                    revision.PcbComponents = pcb.Components;
                    revision.Width = pcb.Width;
                    revision.PhysicalPackages = pcb.Packages;
                    revision.Height = pcb.Height;
                    revision.Outline = pcb.Outline;
                }
                else
                {
                    return InvokeResult<CircuitBoardRevision>.FromError($"Unsupported File Type - {media.FileName}");
                }
            }

            foreach(var cmp in revision.PcbComponents)
            {
                cmp.Package.Value = null;
            }

            return InvokeResult<CircuitBoardRevision>.Create(revision);
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
            var pcb = await _circuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(pcb, AuthorizeActions.Read, user, org);
            foreach (var revision in pcb.Revisions)
            {
                foreach(var cmp in revision.PcbComponents)
                {
                    if (cmp.Package != null)
                    {
                        var pcks = revision.PhysicalPackages.Where(c => c.Key == cmp.Package.Key);
                        if (!pcks.Any())
                            throw new InvalidOperationException($"Could not find package for: {cmp.Name}");

                        if (pcks.Count() > 1)
                            throw new Exception($"Found multiple for {cmp.Package.Text} - {cmp.Package.Key}");


                        cmp.Package.Value = pcks.Single(); 
                    }
                }
            }

            return pcb;
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

                foreach (var cmp in revision.PcbComponents)
                    cmp.Package.Value = null;
            }

            await _circuitBoardRepo.UpdateCircuitBoardAsync(pcb);
            return InvokeResult.Success;
        }
    }
}