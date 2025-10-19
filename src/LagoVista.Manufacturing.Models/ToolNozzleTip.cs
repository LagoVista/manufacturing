// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6b9ddb1501785e0b678cb38ddd80e48e5ded88fa3bcdb182c6a734feed1b87a2
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.NozzleTip_Title, ManufacturingResources.Names.NozzleTip_Description,
        ManufacturingResources.Names.NozzleTip_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-creative", Cloneable: true,
        FactoryUrl: "/api/mfg/machine/nozzletip/factory")]
    public class ToolNozzleTip : ModelBase, IFormDescriptor
    {
        public ToolNozzleTip()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

       
        EntityHeader<PnPMachineNozzleTip> _nozzleTip;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_Title, FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl: "/api/mfg/pnp/nozzletips", IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<PnPMachineNozzleTip> NozzleTip
        {
            get => _nozzleTip;
            set => Set(ref _nozzleTip, value);
        }

        private Point3D<decimal> _toolRackLocation;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_ToolRackLocation, FieldType: FieldTypes.Point3D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point3D<decimal> ToolRackLocation
        {
            get => _toolRackLocation;
            set => Set(ref _toolRackLocation, value);
        }

        private decimal? _idleVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_IdleVacuum, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public decimal? IdleVacuum
        {
            get => _idleVacuum;
            set => Set(ref _idleVacuum, value);
        }

        private decimal? _partPickedVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_PartPickedVacuum, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public decimal? PartPickedVacuum
        {
            get => _partPickedVacuum;
            set => Set(ref _partPickedVacuum, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(NozzleTip),
                nameof(ToolRackLocation),
                nameof(IdleVacuum),
                nameof(PartPickedVacuum),
            };
        }
    }
}
