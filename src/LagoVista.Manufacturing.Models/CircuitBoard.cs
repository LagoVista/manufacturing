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
        ManufacturingResources.Names.Machine_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-searching-2", Cloneable: true,
        SaveUrl: "/api/mfg/pcb", GetUrl: "/api/mfg/pcb/{id}", GetListUrl: "/api/mfg/pcb", FactoryUrl: "/api/mfg/pcb/factory",
        DeleteUrl: "/api/mfg/pcb/{id}", ListUIUrl: "/mfg/pcbs", EditUIUrl: "/mfg/pcb/{id}", CreateUIUrl: "/mfg/pcb/add")]

    public class CircuitBoard : MfgModelBase, ISummaryFactory
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-folders";

        public CircuitBoardSummary CreateSummary()
        {
            return new CircuitBoardSummary()
            {
                Id = Id,
                Icon = Icon,
                Key = Key,
                Description = Description,
                Name = Name,
                IsPublic = IsPublic
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public class CircuitBoardSummary : SummaryData
    {

    }
}
