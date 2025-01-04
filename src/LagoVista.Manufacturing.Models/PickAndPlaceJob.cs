using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using System.Collections.Generic;

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

        private Point2D<double> _boardOrigin = new Point2D<double>(0, 0);
        public Point2D<double> BoardOrigin
        {
            get { return _boardOrigin; }
            set { Set(ref _boardOrigin, value); }
        }

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_CurrentSerialNumber, FieldType: FieldTypes.Integer,  ResourceType: typeof(ManufacturingResources))]
        public int CurrentSerialNumber { get; set; } = 1000;


        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Board, IsUserEditable:false, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Board { get; set; }

        public CircuitBoardRevision BoardRevision { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Board),
                nameof(Description)
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJob_Title, ManufacturingResources.Names.PickAndPlaceJob_Description,
        ManufacturingResources.Names.PartPack_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
        SaveUrl: "/api/mfg/pnpjob", GetUrl: "/api/mfg/pnpjob/{id}", GetListUrl: "/api/mfg/pnpjobs", FactoryUrl: "/api/mfg/pnpjob/factory", DeleteUrl: "/api/mfg/pnpjob/{id}",
        ListUIUrl: "/mfg/pnpjobs", EditUIUrl: "/mfg/pnpjob/{id}", CreateUIUrl: "/mfg/pnpjob/add")]
    public class PickAndPlaceJobSummary : SummaryData
    {

    }
}
