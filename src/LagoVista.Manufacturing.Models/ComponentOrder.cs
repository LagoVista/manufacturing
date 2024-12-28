using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models.Resources;
using NLog.LayoutRenderers.Wrappers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{

    public enum ComponentOrderStatusTypes
    {
        [EnumLabel(ComponentOrder.ComponentOrderStatusTypes_Pending, ManufacturingResources.Names.ComponentOrderStatusTypes_Pending, typeof(ManufacturingResources))]
        Pending,
        [EnumLabel(ComponentOrder.ComponentOrderStatusTypes_Ordered, ManufacturingResources.Names.ComponentOrderStatusTypes_Ordered, typeof(ManufacturingResources))]
        Ordered,
        [EnumLabel(ComponentOrder.ComponentOrderStatusTypes_Received, ManufacturingResources.Names.ComponentOrderStatusTypes_Received, typeof(ManufacturingResources))]
        Received,
        [EnumLabel(ComponentOrder.ComponentOrderStatusTypes_Stocked, ManufacturingResources.Names.ComponentOrderStatusTypes_Stocked, typeof(ManufacturingResources))]
        Stocked,
    }


    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentOrder_Title, ManufacturingResources.Names.ComponentOrder_Description,
    ManufacturingResources.Names.ComponentOrder_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-product-1", Cloneable: true,
        SaveUrl: "/api/mfg/order", GetUrl: "/api/mfg/order/{id}", GetListUrl: "/api/mfg/orders", FactoryUrl: "/api/mfg/order/factory", DeleteUrl: "/api/mfg/order/{id}",
        ListUIUrl: "/mfg/orders", EditUIUrl: "/mfg/orders/{id}", CreateUIUrl: "/mfg/orders/add")]
    public class ComponentOrder : MfgModelBase, ISummaryFactory, IFormDescriptor, IFormDescriptorCol2, IFormDescriptorBottom, IFormConditionalFields, IFormAdditionalActions
    {

        public const string ComponentOrderStatusTypes_Pending = "pending";
        public const string ComponentOrderStatusTypes_Ordered = "ordered";
        public const string ComponentOrderStatusTypes_Received = "received";
        public const string ComponentOrderStatusTypes_Stocked = "stocked";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-product-1";



        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_Supplier, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Supplier { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_SupplierOrderNumber, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string SupplierOrderNumber { get; set; }


        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_Status, WaterMark: Resources.ManufacturingResources.Names.Common_Status_Select, FieldType: FieldTypes.Picker, EnumType: typeof(ComponentOrderStatusTypes), IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<ComponentOrderStatusTypes> Status { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_OrderDate, FieldType: FieldTypes.Date, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string OrderDate { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_ReceiveDate, FieldType: FieldTypes.Date, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ReceivedDate { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_Tax, FieldType: FieldTypes.Money, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Tax { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_Tariff, FieldType: FieldTypes.Money, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Tariff { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_Shipping, FieldType: FieldTypes.Money, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Shipping { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_SubTotal, FieldType: FieldTypes.Money, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal SubTotal { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_Total, FieldType: FieldTypes.Money, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Total { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_LineItems, ChildListDisplayMembers: "Description,MfgPartNumber", FieldType: FieldTypes.ChildListInline, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public List<ComponentOrderLineItem> LineItems { get; set; } = new List<ComponentOrderLineItem>();

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrder_LineItemCsv, FieldType: FieldTypes.MultiLineText, Rows: 20, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string LineItemsCSV { get; set; }

        public ComponentOrderSummary CreateSummary()
        {
            return new ComponentOrderSummary()
            {
                Name = Name,
                Key = Key,
                Id = Id,
                Icon = Icon,
                IsPublic = IsPublic,
                Description = Description,
                Supplier = Supplier,
                SupplierOrderNumber = SupplierOrderNumber,
                Status = Status.Text,
                OrderDate = OrderDate,
                Total = Total
            };
        }

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()
            {
                new FormAdditionalAction()
                {
                     ForEdit = true,
                      Icon = "fa fa-print",
                      Key = "labels",
                      Title = ManufacturingResources.ComponentOrder_PrintLabels
                }
            };
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(LineItemsCSV) },
                Conditionals = new List<FormConditional>()
                 {
                     new FormConditional()
                     {
                          ForCreate = true,
                          ForUpdate = false,
                          VisibleFields = new List<string>(){nameof(LineItemsCSV) }
                     }
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
                nameof(Supplier),
                nameof(SupplierOrderNumber),
                nameof(OrderDate),
                nameof(ReceivedDate),
            };
        }

        public List<string> GetFormFieldsBottom()
        {
            return new List<string>()
            {
                nameof(LineItemsCSV),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Status),
                nameof(SubTotal),
                nameof(Tariff),
                nameof(Tax),
                nameof(Shipping),
                nameof(Total),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentOrders_Title, ManufacturingResources.Names.ComponentOrder_Description,
    ManufacturingResources.Names.ComponentOrder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
        SaveUrl: "/api/mfg/order", GetUrl: "/api/mfg/order/{id}", GetListUrl: "/api/mfg/orders", FactoryUrl: "/api/mfg/order/factory", DeleteUrl: "/api/mfg/order/{id}",
        ListUIUrl: "/mfg/orders", EditUIUrl: "/mfg/orders/{id}", CreateUIUrl: "/mfg/orders/add")]
    public class ComponentOrderSummary : SummaryData
    {
        public decimal Total { get; set; }
        public string Supplier { get; set; }
        public string SupplierOrderNumber { get; set; }
        public string Status { get; set; }
        public string OrderDate { get; set; }

    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentOrderLineItem_Title, ManufacturingResources.Names.ComponentOrderLineItem_Help,
    ManufacturingResources.Names.ComponentOrder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-product-1",
        FactoryUrl: "/api/mfg/order/lineitem")]
    public class ComponentOrderLineItem : IFormDescriptor
    {
        public ComponentOrderLineItem()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrderLineItem_Component, WaterMark: ManufacturingResources.Names.ComponentOrderLineItem_Component_Select,
            EntityHeaderPickerUrl: "/api/mfg/components", FieldType: FieldTypes.EntityHeaderPicker, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Component { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrderLineItem_SupplierPartNumber, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string SupplierPartNumber { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrderLineItem_MfgPartNumber, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string MfgPartNumber { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_Description, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Description { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_QuantityBackOrdered, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityBackOrdered { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_QuantityOrdered, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityOrdered { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_QuantityReceived, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal QuantityReceived { get; set; }


        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrderLineItem_UnitPrice, FieldType: FieldTypes.Money, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal UnitPrice { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.Common_Notes, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Notes { get; set; }

        [FormField(LabelResource: Resources.ManufacturingResources.Names.ComponentOrderLineItem_Received,
            HelpResource: ManufacturingResources.Names.ComponentOrderLineItem_Received_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool Received { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Description),
                nameof(Component),
                nameof(SupplierPartNumber),
                nameof(MfgPartNumber),
                nameof(Received),
                nameof(QuantityOrdered),
                nameof(QuantityBackOrdered),
                nameof(QuantityReceived),
                nameof(UnitPrice),
                nameof(Notes),
            };
        }

        public static ComponentOrderLineItem FromOrderLine(string[] parts)
        {
            foreach (var part in parts)
            {
                Console.WriteLine(part.Trim('"'));
            }

            return new ComponentOrderLineItem()
            {
                QuantityOrdered = decimal.Parse(parts[5].Trim('"', '$')),
                SupplierPartNumber = parts[1].Trim('"', '$'),
                MfgPartNumber = parts[2].Trim('"', '$'),
                Description = parts[3].Trim('"', '$'),
                UnitPrice = decimal.Parse(parts[7].Trim('"', '$')),
                QuantityBackOrdered = decimal.Parse(parts[6].Trim('"', '$'))
            };
        }
    }
}
