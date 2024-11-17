using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Pcb_Title, ManufacturingResources.Names.Pcb_Description,
        ManufacturingResources.Names.Machine_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-landing-page", Cloneable: true,
        SaveUrl: "/api/mfg/pcb", GetUrl: "/api/mfg/pcb/{id}", GetListUrl: "/api/mfg/pcb", FactoryUrl: "/api/mfg/pcb/factory",
        DeleteUrl: "/api/mfg/pcb/{id}", ListUIUrl: "/mfg/pcbs", EditUIUrl: "/mfg/pcb/{id}", CreateUIUrl: "/mfg/pcb/add")]

    public class CircuitBoard : MfgModelBase, IFormDescriptor, ISummaryFactory
    {
        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Sku, IsRequired:true, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Sku { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-landing-page";

        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revisions, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/pcb/revision/factory", 
                ResourceType: typeof(ManufacturingResources))]
        public List<CircuitBoardRevision> Revisions { get; set; } = new List<CircuitBoardRevision>();

        public CircuitBoardSummary CreateSummary()
        {
            return new CircuitBoardSummary()
            {
                Id = Id,
                Icon = Icon,
                Key = Key,
                Description = Description,
                Name = Name,
                Sku = Sku,
                IsPublic = IsPublic
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Sku),
                nameof(Revisions),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Pcbs_Title, ManufacturingResources.Names.Pcb_Description,
        ManufacturingResources.Names.Machine_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-landing-page", Cloneable: true,
        SaveUrl: "/api/mfg/pcb", GetUrl: "/api/mfg/pcb/{id}", GetListUrl: "/api/mfg/pcb", FactoryUrl: "/api/mfg/pcb/factory",
        DeleteUrl: "/api/mfg/pcb/{id}", ListUIUrl: "/mfg/pcbs", EditUIUrl: "/mfg/pcb/{id}", CreateUIUrl: "/mfg/pcb/add")]
    public class CircuitBoardSummary : SummaryData
    {
        public string Sku { get; set; }
    }
}
