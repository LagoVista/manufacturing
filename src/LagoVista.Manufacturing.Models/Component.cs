using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Component_Title, ManufacturingResources.Names.Component_Description,
        ManufacturingResources.Names.Component_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
        SaveUrl: "/api/mfg/compoent", GetUrl: "/api/mfg/compoent/{id}", GetListUrl: "/api/mfg/compoents", FactoryUrl: "/api/mfg/compoent/factory", DeleteUrl: "/api/mfg/compoent/{id}",
        ListUIUrl: "/mfg/components", EditUIUrl: "/mfg/component/{id}", CreateUIUrl: "/mfg/component/add")]
    public class Component : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Component_ComponentType, FieldType: FieldTypes.Category, CustomCategoryType:"component_type", WaterMark:ManufacturingResources.Names.Component_ComponentType_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ComponentType { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Room, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Room { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_ShelfUnit, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string ShelfUnit { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_ShelfUnit, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Shelf { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Bin, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Bin { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-core-1";


        [FormField(LabelResource: ManufacturingResources.Names.Component_PackageType, FieldType: FieldTypes.Picker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ComponentPackage { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_VendorLink, FieldType: FieldTypes.WebLink, ResourceType: typeof(ManufacturingResources))]
        public string VendorLink { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_MfgPartNumb, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string ManufacturePartNumber { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_DataSheet, FieldType: FieldTypes.WebLink, ResourceType: typeof(ManufacturingResources))]
        public string DataSheet { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Value, FieldType: FieldTypes.WebLink, ResourceType: typeof(ManufacturingResources))]
        public string Value { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Attr1, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Attr1 { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Attr2, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Attr2 { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_QuantityOnHand, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityOnHand { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_QuantityOnOrder, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityOnOrder { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Cost, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public decimal Cost { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_ExtendedPrice, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public decimal ExtendedPrice { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_PartNumber, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string PartNumber { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_PartPack, FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl:"/api/mfg/partpacks", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader PartPack { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Feeder, WaterMark: ManufacturingResources.Names.Component_Feeder_Select, 
            FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl: "/api/mfg/feeders", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Feeder { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_Row, FieldType: FieldTypes.Integer, ResourceType: typeof(ManufacturingResources))]
        public int? Row { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPurchase_Title, FactoryUrl: "/api/mfg/component/purchase/factory", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<ComponentPurchase> Purchases { get; set; } = new List<ComponentPurchase>();


        public ComponentSummary CreateSummary()
        {
            return new ComponentSummary()
            {
                Id = Id,
                Name = Name,
                Key = Key,
                PartNumber = PartNumber,
                QuantityOnHand = QuantityOnHand,
                QuantityOnOrder = QuantityOnOrder,
                IsPublic = IsPublic,
                Icon = Icon,
                Description = Description
            };
        }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(PartNumber),
                nameof(ComponentType),
                nameof(ComponentPackage),
                nameof(Value),
                nameof(Attr1),
                nameof(Attr2),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(ManufacturePartNumber),
                nameof(VendorLink),
                nameof(DataSheet),
                nameof(ExtendedPrice),
                nameof(Cost),
                nameof(ShelfUnit),
                nameof(Shelf),
                nameof(Bin),
                nameof(PartPack),
                nameof(Row),
                nameof(Feeder),
                nameof(Purchases),
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Component_Title, ManufacturingResources.Names.Component_Title,
        ManufacturingResources.Names.Component_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
        SaveUrl: "/api/compoent", GetUrl: "/api/compoent/{id}", GetListUrl: "/api/compoents", FactoryUrl: "/api/compoent/factory", DeleteUrl: "/api/compoent/{id}",
        ListUIUrl: "/mfg/components", EditUIUrl: "/mfg/component/{id}", CreateUIUrl: "/mfg/component/add")]
    public class ComponentSummary : SummaryData
    {
        public string PartNumber { get; set; }
        public decimal QuantityOnOrder { get; set; }
        public decimal QuantityOnHand{ get; set; }
    }
}
