using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public enum PickAndPlaceJobRunStates
    {
        [EnumLabel(PickAndPlaceJobRun.JobState_Creted, ManufacturingResources.Names.JobState_Creted, typeof(ManufacturingResources))]
        Created,
        [EnumLabel(PickAndPlaceJobRun.JobState_Running, ManufacturingResources.Names.JobState_Running, typeof(ManufacturingResources))]
        Running,
        [EnumLabel(PickAndPlaceJobRun.JobState_Completed, ManufacturingResources.Names.JobState_Completed, typeof(ManufacturingResources))]
        Completed,
        [EnumLabel(PickAndPlaceJobRun.JobState_Failed, ManufacturingResources.Names.JobState_Failed, typeof(ManufacturingResources))]
        Failed,
        [EnumLabel(PickAndPlaceJobRun.JobState_Aborted, ManufacturingResources.Names.JobState_Aborted, typeof(ManufacturingResources))]
        Aborted
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJobRun_Title, ManufacturingResources.Names.PickAndPlaceJobRun_Description,
        ManufacturingResources.Names.PickAndPlaceJobRun_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
        SaveUrl: "/api/mfg/pnpjob/run", GetUrl: "/api/mfg/pnpjob/run/{id}", GetListUrl: "/api/mfg/pnpjob/runs", FactoryUrl: "/api/mfg/pnpjob/{jobid}/run/factory", DeleteUrl: "/api/mfg/pnpjob/run/{id}",
        ListUIUrl: "/mfg/pnpjobruns", EditUIUrl: "/mfg/pnpjobrun/{id}", CreateUIUrl: "/mfg/pnpjobrun/add")]
    public class PickAndPlaceJobRun : EntityBase, ISummaryFactory, IValidateable
    {
        public const string JobState_Creted = "created";
        public const string JobState_Running = "running";
        public const string JobState_Completed = "completed";
        public const string JobState_Failed = "failed";
        public const string JobState_Aborted = "aborted";

        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public EntityHeader RanBy { get; set; }
        [FormField(IsRequired:true)]
        public EntityHeader Job { get; set; }
        public ObservableCollection<PickAndPlaceJobRunPlacement> Placements { get; set; } = new ObservableCollection<PickAndPlaceJobRunPlacement>();
        public double PercentCompleted { get; set; }
        public double PercentSuccess { get; set; }
        public int SerialNumber { get; set; }
        public double Cost { get; set; }
        public double Extended { get; set; }
        public bool Billed { get; set; }
        public EntityHeader Invoice { get; set; }
        public EntityHeader<JobPlacementStatuses> Status { get; set; } = EntityHeader<JobPlacementStatuses>.Create(JobPlacementStatuses.Pending);
        public PickAndPlaceJobRunSummary CreateSummary()
        {
            return new PickAndPlaceJobRunSummary()
            {
                Id = Id,
                Extended = Extended,
                Cost = Cost,
                SerialNumber = SerialNumber,
                PercentCompleted = PercentCompleted,
                PercentSuccess = PercentSuccess,
                JobName = Job.Text,
                RanBy = RanBy.Text,
                Status = Status.Text,
                StatusKey = Status.Id,
                StartTimeStamp = StartTime,
                EndTimeStamp = EndTime
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            throw new NotImplementedException();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJobRuns_Title, ManufacturingResources.Names.PickAndPlaceJobRun_Description,
      ManufacturingResources.Names.PickAndPlaceJobRun_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
      SaveUrl: "/api/mfg/pnpjob/run", GetUrl: "/api/mfg/pnpjob/run/{id}", GetListUrl: "/api/mfg/pnpjob/runs", FactoryUrl: "/api/mfg/pnpjob/run/factory", DeleteUrl: "/api/mfg/pnpjob/run/{id}",
      ListUIUrl: "/mfg/pnpjobruns", EditUIUrl: "/mfg/pnpjobrun/{id}", CreateUIUrl: "/mfg/pnpjobrun/add")]
    public class PickAndPlaceJobRunSummary : SummaryData
    {
        public string JobName { get; set; }
        public string RanBy { get; set; }
        public string Status { get; set; }
        public string StatusKey { get; set; }
        public string StartTimeStamp { get; set; }
        public string EndTimeStamp { get; set; }
        public double PercentCompleted { get; set; }
        public double PercentSuccess { get; set; }
        public int SerialNumber { get; set; }
        public double Cost { get; set; }
        public double Extended { get; set; }
        public bool Billed { get; set; }
        public string Invoice { get; set; }

    }

    public enum JobPlacementStatuses
    {
        [EnumLabel(PickAndPlaceJobRunPlacement.JobPlacementStatus_Pending, ManufacturingResources.Names.JobPlacementStatus_Pending, typeof(ManufacturingResources))]
        Pending,
        [EnumLabel(PickAndPlaceJobRunPlacement.JobPlacementStatus_InProcess, ManufacturingResources.Names.JobPlacementStatus_InProcess, typeof(ManufacturingResources))]
        InProcess,
        [EnumLabel(PickAndPlaceJobRunPlacement.JobPlacementStatus_Placed, ManufacturingResources.Names.JobPlacementStatus_Placed, typeof(ManufacturingResources))]
        Placed,
        [EnumLabel(PickAndPlaceJobRunPlacement.JobPlacementStatus_Failed, ManufacturingResources.Names.JobPlacementStatus_Failed, typeof(ManufacturingResources))]
        Failed
    }

    public class PickAndPlaceJobRunPlacement
    {
        public const string JobPlacementStatus_Pending = "Pending";
        public const string JobPlacementStatus_InProcess = "InProcess";
        public const string JobPlacementStatus_Placed = "Placed";
        public const string JobPlacementStatus_Failed = "Failed";

        public PickAndPlaceJobRunPlacement()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        public EntityHeader AutoFeeder { get; set; }
        public EntityHeader StripFeeder { get; set; }
        public EntityHeader<JobPlacementStatuses> Status { get; set; } = EntityHeader<JobPlacementStatuses>.Create(JobPlacementStatuses.Pending);

        public string Name { get; set; }
        public EntityHeader Component { get; set; }
        public EntityHeader ComponentPackage { get; set; }
        public Point2D<double> PCBLocation { get; set; }
        public double Rotation { get; set; }
        public double? VacuumPrePick { get; set; }
        public double? VacuumPostPick { get; set; }
        public double? VacuumPostPlace { get; set; }
        public double? DurationMS { get; set; }
        public string TimeStamp { get; set; }
        public string Errors { get; set; }

        public List<PlacementStateHistory> Transitions { get; set; } = new List<PlacementStateHistory>();
    }
}
