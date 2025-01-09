using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJob_Title, ManufacturingResources.Names.PickAndPlaceJob_Description,
    ManufacturingResources.Names.PickAndPlaceJob_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
    SaveUrl: "/api/mfg/pnpjob", GetUrl: "/api/mfg/pnpjob/{id}", GetListUrl: "/api/mfg/pnpjobs", FactoryUrl: "/api/mfg/pnpjob/factory", DeleteUrl: "/api/mfg/pnpjob/{id}",
    ListUIUrl: "/mfg/pnpjobs", EditUIUrl: "/mfg/pnpjob/{id}", CreateUIUrl: "/mfg/pnpjob/add")]
    public class PickAndPlaceJob : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory, IIDEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-control-panel";

        public PickAndPlaceJobSummary CreateSummary()
        {
            return new PickAndPlaceJobSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                Description = Description,
                Key = Key,
                IsPublic = IsPublic
            };
        }

        private Point2D<double> _defaultWorkOrigin = new Point2D<double>(0, 0);
        public Point2D<double> DefaultBoardOrigin
        {
            get { return _defaultWorkOrigin; }
            set { Set(ref _defaultWorkOrigin, value); }
        }

        private Point2D<double> _actualWorkOrigin = new Point2D<double>(0, 0);
        public Point2D<double> ActualBoardOrigin
        {
            get { return _actualWorkOrigin; }
            set { Set(ref _actualWorkOrigin, value); }
        }

        public List<BoardFiducial> BoardFiducials { get; set; } = new List<BoardFiducial>();

        public double BoardAngle { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_CurrentSerialNumber, FieldType: FieldTypes.Integer,  ResourceType: typeof(ManufacturingResources))]
        public int CurrentSerialNumber { get; set; } = 1000;

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Cost, IsUserEditable: true, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public double Cost { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Extneded, IsUserEditable: true, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public double Extended { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Board, IsUserEditable:false, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Board { get; set; }

        public CircuitBoardRevision BoardRevision { get; set; }

        public List<PickAndPlaceJobPart> Parts { get; set; } = new List<PickAndPlaceJobPart>();

        public ObservableCollection<ErrorMessage> ErrorMessages { get; set; } = new ObservableCollection<ErrorMessage>()

        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Board),
                nameof(Cost),
                nameof(Extended),
                nameof(Description)
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJobs_Title, ManufacturingResources.Names.PickAndPlaceJob_Description,
        ManufacturingResources.Names.PartPack_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
        SaveUrl: "/api/mfg/pnpjob", GetUrl: "/api/mfg/pnpjob/{id}", GetListUrl: "/api/mfg/pnpjobs", FactoryUrl: "/api/mfg/pnpjob/factory", DeleteUrl: "/api/mfg/pnpjob/{id}",
        ListUIUrl: "/mfg/pnpjobs", EditUIUrl: "/mfg/pnpjob/{id}", CreateUIUrl: "/mfg/pnpjob/add")]
    public class PickAndPlaceJobSummary : SummaryData
    {

    }

    public class BoardFiducial
    {
        public Point2D<double> Expected { get; set; }
        public Point2D<double> Actual { get; set; }
    }

    public class PickAndPlaceJobPart
    {
        public EntityHeader AutoFeeder { get; set; }
        public EntityHeader StripFeeder { get; set; }
        public EntityHeader Component { get; set; }
        public EntityHeader ComponentPackage { get; set; }
        public string Errors { get; set; }
        public List<PickAndPlaceJobPlacement> Placements { get; set; }
    }

    public class PickAndPlaceJobPlacement
    {
        public string Name { get; set; }
        public Point2D<double> PCBLocation { get; set; }
        public double Rotation { get; set; }
    }
}
