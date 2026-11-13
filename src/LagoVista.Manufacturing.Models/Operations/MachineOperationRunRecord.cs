using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.Manufacturing.Models.Resources;
using System;

namespace LagoVista.Manufacturing.Models
{
    /// <summary>
    /// Durable evidence for one strongly typed machine operation. This is
    /// NOT another source of feeder offsets or other machine configuration.
    /// </summary>
    [EntityDescription(ManufacutringDomain.Manufacturing,
        ManufacturingResources.Names.PickAndPlaceJobRun_Title,
        ManufacturingResources.Names.PickAndPlaceJobRun_Description,
        ManufacturingResources.Names.PickAndPlaceJobRun_Description,
        EntityDescriptionAttribute.EntityTypes.Manufacturing,
        ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: false)]
    public class MachineOperationRunRecord : EntityBase, ISummaryFactory
    {
        public int RecordSchemaVersion { get; set; } = 1;
        public MachineOperationRunSnapshot Snapshot { get; set; }
        public MachineOperationDefinition Definition { get; set; }

        // Query projections; never used to command the machine.
        public string MachineId { get; set; }
        public string FeederId { get; set; }
        public int? RowIndex { get; set; }
        public DateTimeOffset RunUpdatedAtUtc { get; set; }

        public MachineOperationRunSummary CreateSummary()
        {
            return new MachineOperationRunSummary
            {
                Id = Id,
                Name = Name,
                Key = Key,
                MachineId = MachineId,
                FeederId = FeederId,
                RowIndex = RowIndex,
                DefinitionKey = Snapshot?.DefinitionKey,
                DefinitionVersion = Snapshot?.DefinitionVersion ?? 0,
                Status = Snapshot?.Status.ToString(),
                ActiveStepKey = Snapshot?.ActiveStepKey,
                RunUpdatedAtUtc = RunUpdatedAtUtc,
                Error = Snapshot?.LastError,
                StepCount = Snapshot?.Steps?.Count ?? 0
            };
        }

        ISummaryData ISummaryFactory.CreateSummary() => CreateSummary();
    }

    public class MachineOperationRunSummary : SummaryData
    {
        public string MachineId { get; set; }
        public string FeederId { get; set; }
        public int? RowIndex { get; set; }
        public string DefinitionKey { get; set; }
        public int DefinitionVersion { get; set; }
        public string Status { get; set; }
        public string ActiveStepKey { get; set; }
        public DateTimeOffset RunUpdatedAtUtc { get; set; }
        public string Error { get; set; }
        public int StepCount { get; set; }
    }
}
