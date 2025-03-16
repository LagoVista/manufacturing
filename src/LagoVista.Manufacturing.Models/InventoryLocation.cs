using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.InventoryLocation_Title, ManufacturingResources.Names.InventoryLocation_Description,
            ManufacturingResources.Names.InventoryLocation_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
            Icon: "icon-pz-searching-2", Cloneable: true,
            SaveUrl: "/api/mfg/inventory/location", GetUrl: "/api/mfg/inventory/location/{id}", GetListUrl: "/api/mfg/inventory/locations", FactoryUrl: "/api/mfg/inventory/location/factory",
            DeleteUrl: "/api/mfg/inventory/location/{id}", ListUIUrl: "/mfg/inventorylocations", EditUIUrl: "/mfg/inventorylocation/{id}", CreateUIUrl: "/mfg/inventorylocation/add")]

    public class InventoryLocation : MfgModelBase, IFormDescriptor, IValidateable, ISummaryFactory
    {
        [FormField(LabelResource: ManufacturingResources.Names.InventoryLocation_Address1, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Address1 { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.InventoryLocation_Address2, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Address2 { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.InventoryLocation_City, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string City { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.InventoryLocation_State, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string State { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.InventoryLocation_PostalCode, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string PostalCode { get; set; }

        public List<InventoryLocationRoom> Rooms { get; set; } = new List<InventoryLocationRoom>();

        public InventoryLocationSummary CreateSummary()
        {
            return new InventoryLocationSummary()
            {
                Id = Id,
                Description = Description,
                IsDeleted = IsDeleted ?? false,
                IsPublic = IsPublic,
                Key = Key,
                Name = Name,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Address1),
                nameof(Address2),
                nameof(City),
                nameof(State),
                nameof(PostalCode),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public class InventoryLocationRoom
    {
        public InventoryLocationRoom()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public string Key { get; set; }

        public string Description { get; set; }
        public List<InventoryLocationShelfUnit> ShelfUnits { get; set; } = new List<InventoryLocationShelfUnit>();
    }

    public class InventoryLocationShelfUnit
    {
        public InventoryLocationShelfUnit()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public List<InventoryLocationShelf> Shelves { get; set; } = new List<InventoryLocationShelf>();
    }

    public class InventoryLocationShelf
    {
        public InventoryLocationShelf()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public List<InventoryLocationColumn> Columns { get; set; } = new List<InventoryLocationColumn>();
    }

    public class InventoryLocationColumn
    {
        public InventoryLocationColumn()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public List<InventoryLocationBin> Bins { get; set; } = new List<InventoryLocationBin>();
    }


    public class InventoryLocationBin
    {
        public InventoryLocationBin()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
    }


    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.InventoryLocation_Title, ManufacturingResources.Names.InventoryLocation_Description,
              ManufacturingResources.Names.InventoryLocation_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
              Icon: "icon-pz-searching-2", Cloneable: true,
              SaveUrl: "/api/mfg/inventory/location", GetUrl: "/api/mfg/inventory/location/{id}", GetListUrl: "/api/mfg/inventory/locations", FactoryUrl: "/api/mfg/inventory/location/factory",
              DeleteUrl: "/api/mfg/inventory/location/{id}", ListUIUrl: "/mfg/inventorylocations", EditUIUrl: "/mfg/inventorylocation/{id}", CreateUIUrl: "/mfg/inventorylocation/add")]

    public class InventoryLocationSummary : SummaryData
    {

    }
}
