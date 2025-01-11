using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Component_Title, ManufacturingResources.Names.Component_Description,
        ManufacturingResources.Names.Component_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
        SaveUrl: "/api/mfg/component", GetUrl: "/api/mfg/component/{id}", GetListUrl: "/api/mfg/components", FactoryUrl: "/api/mfg/component/factory", DeleteUrl: "/api/mfg/component/{id}",
        ListUIUrl: "/mfg/components", EditUIUrl: "/mfg/component/{id}", CreateUIUrl: "/mfg/component/add")]
    public class Component : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity, IFormAdditionalActions
    {

        [FormField(LabelResource: ManufacturingResources.Names.Component_ComponentType, FieldType: FieldTypes.Category, WaterMark:ManufacturingResources.Names.Component_ComponentType_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ComponentType { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Room, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Room { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_ShelfUnit, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string ShelfUnit { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Shelf, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Shelf { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Column, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Column { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_Bin, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Bin { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-core-1";


        [FormField(LabelResource: ManufacturingResources.Names.Component_PackageType, FieldType: FieldTypes.Picker, FactoryUrl: "/api/mfg/component/package/factory", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<ComponentPackage> ComponentPackage { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_VendorLink, FieldType: FieldTypes.WebLink, ResourceType: typeof(ManufacturingResources))]
        public string VendorLink { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_MfgPartNumb, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string ManufacturePartNumber { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Supplier, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Supplier { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_SupplierPartNumb, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string SupplierPartNumber { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_DataSheet, FieldType: FieldTypes.WebLink, ResourceType: typeof(ManufacturingResources))]
        public string DataSheet { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Value, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Value { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Attr1, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Attr1 { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Attr2, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Attr2 { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_QuantityOnHand, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityOnHand { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_QuantityOnOrder, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityOnOrder { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_QuantityBackOrdered, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityBackOrdered { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_Cost, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public decimal Cost { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_ExtendedPrice, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public decimal ExtendedPrice { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_PartNumber, FieldType: FieldTypes.Text, IsRequired:true, ResourceType: typeof(ManufacturingResources))]
        public string PartNumber { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_PartPack, FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl:"/api/mfg/partpacks", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader PartPack { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Polarized, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool Polarized { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Component_Feeder, WaterMark: ManufacturingResources.Names.Component_Feeder_Select, 
            FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl: "/api/mfg/feeders", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Feeder { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Component_Row, FieldType: FieldTypes.Integer, ResourceType: typeof(ManufacturingResources))]
        public int? Row { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPurchase_Title, FactoryUrl: "/api/mfg/component/purchase/factory", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<ComponentPurchase> Purchases { get; set; } = new List<ComponentPurchase>();

        [FormField(LabelResource: ManufacturingResources.Names.Component_Attributes, FactoryUrl: "/api/mfg/component/attribute/factory", OpenByDefault:true, ChildListDisplayMembers:"attributeName,attributeValue", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<ComponentAttribute> Attributes { get; set; } = new List<ComponentAttribute>();


        public ComponentSummary CreateSummary()
        {
            return new ComponentSummary()
            {
                Id = Id,
                Name = Name,
                Key = Key,
                PartNumber = PartNumber,
                Supplier = Supplier,
                SupplierPartNumber = SupplierPartNumber,
                Package = ComponentPackage?.Text,
                QuantityOnHand = QuantityOnHand,
                QuantityOnOrder = QuantityOnOrder,
                IsPublic = IsPublic,
                Icon = Icon,
                Description = Description,
                Value = Value,
                Attr1 = Attr1,
                Attr2 = Attr2,
                ComponentType = ComponentType?.Text
            };
        }

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()
            {
                new FormAdditionalAction()
                {
                    ForCreate = true,
                    ForEdit = true,
                    Icon = "fa fa-globe-pointer",
                    Key = "digikey",
                    Title = "DigiKey Lookup",
                },
                new FormAdditionalAction()
                {
                     ForEdit = true,
                      Icon = "fa fa-print",
                      Key = "labels",
                      Title = ManufacturingResources.ComponentOrder_PrintLabels
                }
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
                nameof(Polarized),
                nameof(Value),
                nameof(Description),
                nameof(QuantityOnHand),
                nameof(QuantityOnOrder),
                nameof(QuantityBackOrdered),
                nameof(Attr1),
                nameof(Attr2),
                nameof(Attributes),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Supplier),
                nameof(SupplierPartNumber),
                nameof(ManufacturePartNumber),
                nameof(VendorLink),
                nameof(DataSheet),
                nameof(ExtendedPrice),
                nameof(Cost),
                nameof(Room),
                nameof(ShelfUnit),
                nameof(Shelf),
                nameof(Column),
                nameof(Bin),
                nameof(Purchases),
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Component_Title, ManufacturingResources.Names.Component_Title,
        ManufacturingResources.Names.Component_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
        SaveUrl: "/api/component", GetUrl: "/api/component/{id}", GetListUrl: "/api/compoents", FactoryUrl: "/api/mfg/component/factory", DeleteUrl: "/api/component/{id}",
        ListUIUrl: "/mfg/components", EditUIUrl: "/mfg/component/{id}", CreateUIUrl: "/mfg/component/add")]
    public class ComponentSummary : SummaryData
    {
        public string PartNumber { get; set; }
        public string Supplier { get; set; }
        public string SupplierPartNumber { get; set; }
        public decimal QuantityOnOrder { get; set; }
        public decimal QuantityOnHand{ get; set; }
        public string Package { get; set; }
        public string Value { get; set; }

        public string Attr1 { get; set; }
        public string Attr2 { get; set; }
        public string ComponentType { get; set; }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Component_Title, ManufacturingResources.Names.ComponentAttributes_Title,
        ManufacturingResources.Names.ComponentAttribute_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),FactoryUrl: "/api/mfg/component/attribute/factory")]
    public class ComponentAttribute : IFormDescriptor
    {
        public ComponentAttribute()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string AttributeName { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Value, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string AttributeValue { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(AttributeName),
                nameof(AttributeValue),
            };
        }
    }
}
