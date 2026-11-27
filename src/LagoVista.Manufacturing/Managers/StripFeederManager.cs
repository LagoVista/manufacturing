// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0f548db6daef9049630ba30970311e9f1a5d921359396e128089562a7e0231f3
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using System.Collections.Generic;
using LagoVista.Core;
using System.Linq;

namespace LagoVista.Manufacturing.Managers
{
    public class StripFeederManager : ManagerBase, IStripFeederManager
    {
        private readonly IStripFeederRepo _stripFeederRepo;
        private readonly IStripFeederTemplateRepo _stripFeederTemplateRepo;
        private readonly IComponentManager _componentManager;
        private readonly IComponentPackageRepo _packageRepo;
        private readonly ICacheProvider _cacheProvider;

        public StripFeederManager(IStripFeederRepo stripFeederRepo, IStripFeederTemplateRepo stripFeederTemplateRepo, IComponentManager componentManager, IComponentPackageRepo packageRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security, ICacheProvider cacheProvider = null) :
            base(logger, appConfig, depmanager, security)
        {
            _cacheProvider = cacheProvider;
            _stripFeederRepo = stripFeederRepo ?? throw new ArgumentNullException(nameof(stripFeederRepo));
            _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
            _packageRepo = packageRepo ?? throw new ArgumentNullException(nameof(packageRepo));
            _stripFeederTemplateRepo = stripFeederTemplateRepo ?? throw new ArgumentNullException(nameof(stripFeederTemplateRepo));
        }
        public async Task<InvokeResult> AddStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Create, user, org);
            ValidationCheck(feeder, Actions.Create);
            if (feeder.OwnerOrganization?.Id != org.Id)
                return InvokeResult.FromError("The feeder must belong to the authenticated organization.");
            return await WithFeederIdLockAsync(org.Id, async () =>
            {
                var owned = (await _stripFeederRepo.GetOwnedStripFeedersAsync(org.Id)).ToList();
                var validation = ValidateFeederIdentity(feeder, null, owned);
                if (!validation.Successful) return validation;
                await _stripFeederRepo.AddStripFeederAsync(feeder);
                return InvokeResult.Success;
            }, message => InvokeResult.FromError(message));
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(feeder, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(feeder);
        }

        public async Task<StripFeeder> CreateFromTemplateAsync(string templateId, EntityHeader org, EntityHeader user)
        {
            var timeStamp = UtcTimestamp.Now;

            var template = await _stripFeederTemplateRepo.GetStripFeederTemplateAsync(templateId);
            await AuthorizeAsync(template, AuthorizeActions.Read, user, org);
            var feeder = new StripFeeder()
            {
                Id = Guid.NewGuid().ToId(),
                CreatedBy = user,
                LastUpdatedBy = user,
                CreationDate = timeStamp,
                LastUpdatedDate = timeStamp,
                TapeSize = template.TapeSize,
                Color = template.Color,
                OriginalTemplate = template.ToEntityHeader(),
                OwnerOrganization = org,
                BottomLeftRow1Margin = template.BottomLeftRow1Margin,
                Width = template.Width,
                Height = template.Height,
                Length = template.Length,
                TapeReferenceHoleOffset = template.TapeReferenceHoleOffset,
                RowCount = template.RowCount,
                RowWidth = template.RowWidth,
                RowSpacing = template.RowSpacing,
            };


            for(var idx = 0; idx < feeder.RowCount; ++idx)
            {

                var holesInTape = (feeder.Length / 4) - 1;
                var deltaX = holesInTape * 4;

                feeder.Rows.Add(new StripFeederRow()
                {
                    Id = Guid.NewGuid().ToId(),
                    RowIndex = idx + 1,
                    FirstTapeHoleOffset = new Core.Models.Drawing.Point2D<double>(template.TapeReferenceHoleOffset.X + template.BottomLeftRow1Margin.X, 
                        template.BottomLeftRow1Margin.Y + (idx * feeder.RowWidth) + (template.TapeReferenceHoleOffset.Y +(feeder.TapeSize.ToDouble() - 1.5))),
                    LastTapeHoleOffset = new Core.Models.Drawing.Point2D<double>(template.TapeReferenceHoleOffset.X + template.BottomLeftRow1Margin.X + deltaX, 
                        template.BottomLeftRow1Margin.Y + (idx * feeder.RowWidth) + (template.TapeReferenceHoleOffset.Y + (feeder.TapeSize.ToDouble() - 1.5))),
                    CurrentPartIndex = 1,
                });
            }

            return feeder;
        }

        public async Task<InvokeResult> DeleteStripFeederAsycn(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(feeder, AuthorizeActions.Delete, user, org);
            await _stripFeederRepo.DeleteStripFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteStripFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await ConfirmNoDepenenciesAsync(feeder);
            await AuthorizeAsync(feeder, AuthorizeActions.Delete, user, org);
            await _stripFeederRepo.DeleteStripFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<StripFeeder> GetStripFeederAsync(string id, bool loadComponent, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(feeder, AuthorizeActions.Read, user, org);
            foreach (var row in feeder.Rows)
            {
                if (!EntityHeader.IsNullOrEmpty(row.Component))
                {
                    row.Component.Value = await _componentManager.GetComponentAsync(row.Component.Id, true, org, user);
                    if (!EntityHeader.IsNullOrEmpty(row.Component.Value.ComponentPackage))
                    {
                        row.Component.Value.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(row.Component.Value.ComponentPackage.Id);
                    }
                }
            }

            return feeder;
        }

        public async Task<ListResponse<StripFeeder>> GetStripFeedersForMachineAsync(string machineId, bool loadComponent, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeeder));
            var response = await _stripFeederRepo.GetStripFeedersForMachineAsync(machineId);
            if (loadComponent)
            {
                var packageCache = new Dictionary<string, ComponentPackage>();

                foreach (var feeder in response.Model)
                {
                    foreach (var row in feeder.Rows)
                    {
                        if (!EntityHeader.IsNullOrEmpty(row.Component))
                        {
                            row.Component.Value = await _componentManager.GetComponentAsync(row.Component.Id, true, org, user);
                            if (!EntityHeader.IsNullOrEmpty(row.Component.Value.ComponentPackage))
                            {
                                if (packageCache.ContainsKey(row.Component.Value.ComponentPackage.Id))
                                {
                                    row.Component.Value.ComponentPackage.Value = packageCache[row.Component.Value.ComponentPackage.Id];
                                }
                                else
                                {
                                    row.Component.Value.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(row.Component.Value.ComponentPackage.Id);
                                    packageCache.Add(row.Component.Value.ComponentPackage.Id, row.Component.Value.ComponentPackage.Value);
                                }
                            }
                        }
                    }
                }
            }

            return response;
        }

        public async Task<ListResponse<StripFeederSummary>> GetStripFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeeder));
            return await _stripFeederRepo.GetStripFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<ListResponse<StripFeederSummary>> GetOwnedStripFeederSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeeder));
            return await _stripFeederRepo.GetOwnedStripFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult<StripFeederSummary>> AssignStripFeederIdAsync(string id, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeeder));
            return await WithFeederIdLockAsync(org.Id, async () =>
            {
                var owned = (await _stripFeederRepo.GetOwnedStripFeedersAsync(org.Id)).ToList();
                var feeder = owned.SingleOrDefault(f => String.Equals(f.Id.ToString(), id, StringComparison.OrdinalIgnoreCase));
                if (feeder == null)
                    return InvokeResult<StripFeederSummary>.FromError("Strip feeder not found in the authenticated organization.");
                await AuthorizeAsync(feeder, AuthorizeActions.Update, user, org);
                if (!StripFeederIdentity.TryNormalize(feeder.FeederId, out var existing))
                    return InvokeResult<StripFeederSummary>.FromError("The feeder has an invalid legacy ID. Correct the ID explicitly before printing.");
                if (existing != null)
                {
                    if (owned.Any(f => f.Id != feeder.Id && StripFeederIdentity.Matches(f.FeederId, existing)))
                        return InvokeResult<StripFeederSummary>.FromError("This feeder ID is already assigned to another feeder.");
                    return InvokeResult<StripFeederSummary>.Create(feeder.CreateSummary());
                }
                var next = StripFeederIdentity.FindAvailable(owned.Select(f => f.FeederId));
                if (next == null)
                    return InvokeResult<StripFeederSummary>.FromError("All feeder IDs are assigned.");
                feeder.FeederId = next;
                feeder.LastUpdatedBy = user;
                feeder.LastUpdatedDate = UtcTimestamp.Now;
                ValidationCheck(feeder, Actions.Update);
                await _stripFeederRepo.UpdateStripFeederAsync(feeder);
                return InvokeResult<StripFeederSummary>.Create(feeder.CreateSummary());
            }, message => InvokeResult<StripFeederSummary>.FromError(message));
        }

        private static InvokeResult ValidateFeederIdentity(StripFeeder feeder, StripFeeder existing, IEnumerable<StripFeeder> owned)
        {
            if (!StripFeederIdentity.TryNormalize(feeder.FeederId, out var normalized))
            {
                if (existing != null && String.Equals(existing.FeederId, feeder.FeederId, StringComparison.Ordinal))
                    return InvokeResult.Success;
                return InvokeResult.FromError("Feeder ID must be exactly four hexadecimal digits.");
            }
            if (existing != null && StripFeederIdentity.TryNormalize(existing.FeederId, out var previous)
                && previous != null && previous != normalized)
                return InvokeResult.FromError("An assigned feeder ID cannot be changed or cleared.");
            if (normalized != null && owned.Any(f => f.Id != feeder.Id && StripFeederIdentity.Matches(f.FeederId, normalized)))
                return InvokeResult.FromError("This feeder ID is already assigned to another feeder.");
            feeder.FeederId = normalized;
            return InvokeResult.Success;
        }

        private async Task<TResult> WithFeederIdLockAsync<TResult>(string orgId, Func<Task<TResult>> action, Func<string, TResult> error)
        {
            if (_cacheProvider == null)
                return error("Feeder identity lock provider is unavailable.");
            var key = $"manufacturing:strip-feeder-identity:{orgId}";
            var token = Guid.NewGuid().ToString("N");
            if (!await _cacheProvider.AttemptAcquireLockAsync(key, token, TimeSpan.FromMinutes(2)))
                return error("Another feeder update is in progress. Please retry.");
            try
            {
                return await action();
            }
            finally
            {
                await _cacheProvider.ReleaseLockAsync(key, token);
            }
        }

        public async Task<InvokeResult> UpdateStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Update, user, org);
            ValidationCheck(feeder, Actions.Update);
            if (feeder.OwnerOrganization?.Id != org.Id)
                return InvokeResult.FromError("The feeder must belong to the authenticated organization.");
            return await WithFeederIdLockAsync(org.Id, async () =>
            {
                var owned = (await _stripFeederRepo.GetOwnedStripFeedersAsync(org.Id)).ToList();
                var existing = owned.SingleOrDefault(f => f.Id == feeder.Id);
                if (existing == null) return InvokeResult.FromError("Strip feeder not found in the authenticated organization.");
                await AuthorizeAsync(existing, AuthorizeActions.Update, user, org);
                var validation = ValidateFeederIdentity(feeder, existing, owned);
                if (!validation.Successful) return validation;
                await _stripFeederRepo.UpdateStripFeederAsync(feeder);
                return InvokeResult.Success;
            }, message => InvokeResult.FromError(message));
        }
    }
}