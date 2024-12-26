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
        ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-creative", Cloneable: true,
        FactoryUrl: "/api/mfg/machine/nozzletip/factory")]
    public class ToolNozzleTip : ModelBase, IFormDescriptor
    {
        public ToolNozzleTip()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]

        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Key, FieldType: FieldTypes.Key, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Key { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-control-panel";


        private double _safeMoveHeight;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_SafeMoveHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double SafeMoveHeight 
        {
            get => _safeMoveHeight; 
            set => Set(ref _safeMoveHeight, value);
        }

        private double _placeHeight;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_PickHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight
        {
            get => _placeHeight;
            set => Set(ref _placeHeight, value);
        }

        private double _boardHeight;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_BoardHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double BoardHeight
        {
            get => _boardHeight;
            set => Set(ref _boardHeight, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_InnerDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int InnerDiameter { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_OuterDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int OuterDiameter { get; set; }

        private Point3D<decimal> _toolRackLocation;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_ToolRackLocation, FieldType: FieldTypes.Point3D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<decimal> ToolRackLocation
        {
            get => _toolRackLocation;
            set => Set(ref _toolRackLocation, value);
        }

        private decimal _idleVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_IdleVacuum, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal IdleVacuum
        {
            get => _idleVacuum;
            set => Set(ref _idleVacuum, value);
        }

        private decimal _partPickedVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_PartPickedVacuum, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal PartPickedVacuum
        {
            get => _partPickedVacuum;
            set => Set(ref _partPickedVacuum, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(SafeMoveHeight),
                nameof(PickHeight),
                nameof(BoardHeight),
                nameof(InnerDiameter),
                nameof(OuterDiameter),
                nameof(ToolRackLocation),
                nameof(IdleVacuum),
                nameof(PartPickedVacuum),
            };
        }
    }
}
