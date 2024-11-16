using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentPurchase_Title, ManufacturingResources.Names.ComponentPurchase_Description,
        ManufacturingResources.Names.ComponentPurchase_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-device-model", Cloneable: true,
        FactoryUrl: "/api/mfg/component/purchase/factory")]
    public class ComponentPurchase : IIDEntity, INamedEntity, IFormDescriptor
    {
        public ComponentPurchase()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_Name, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_OrderNumber, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string OrderNumber { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_Vendor, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Vendor { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_Quantity, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int Qty { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_OrderDate, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string OrderDate { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(OrderNumber),
                nameof(Vendor),
                nameof(Qty),
                nameof(OrderDate)
            };
        }
    }
}
