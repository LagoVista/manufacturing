using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentPurchase_Title, ManufacturingResources.Names.ComponentPurchase_Description,
        ManufacturingResources.Names.ComponentPurchase_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-device-model", Cloneable: true,
        FactoryUrl: "/api/mfg/component/purchase/factory")]
    public class ComponentPurchase : IIDEntity, INamedEntity, IFormDescriptor, IValidateable
    {
        public ComponentPurchase()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        public string OrderId { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_OrderNumber, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string OrderNumber { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_Vendor, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Vendor { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_QuantityOrdered, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityOrdered { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_QuantityReceived, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityReceived { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_QuantityBackOrdered, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityBackOrdered { get; set; }


        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentPurchase_OrderDate, FieldType:FieldTypes.Date, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string OrderDate { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(OrderNumber),
                nameof(Vendor),
                nameof(QuantityOrdered),
                nameof(QuantityReceived),
                nameof(QuantityBackOrdered),
                nameof(OrderDate)
            };
        }
    }
}
