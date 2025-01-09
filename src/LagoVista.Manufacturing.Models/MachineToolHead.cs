using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    public enum ToolHeadTypes
    {
        [EnumLabel(MachineToolHead.MachineToolHead_Type_PartNozzle, ManufacturingResources.Names.MachineToolHead_Type_PartNozzle, typeof(ManufacturingResources))]
        PartNozzle,
        [EnumLabel(MachineToolHead.MachineToolHead_Type_Spindle, ManufacturingResources.Names.MachineToolHead_Type_Spindle, typeof(ManufacturingResources))]
        Spindle,
        [EnumLabel(MachineToolHead.MachineToolHead_Type_PartNozzle, ManufacturingResources.Names.MachineToolHead_Type_PartNozzle, typeof(ManufacturingResources))]
        Laser,
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.MachineToolHead_Title, ManufacturingResources.Names.MachineToolHead_Description,
    ManufacturingResources.Names.MachineToolHead_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources),
    Icon: "icon-fo-maintenance-3", Cloneable: true,
    FactoryUrl: "/api/mfg/machine/toolhead/factory")]
    public class MachineToolHead : ModelBase, IIDEntity, INamedEntity, IIconEntity, IValidateable, IFormDescriptor
    {
        public const string MachineToolHead_Type_Laser = "laser";
        public const string MachineToolHead_Type_Spindle = "spindle";
        public const string MachineToolHead_Type_PartNozzle = "partnozzle";

        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Key, FieldType: FieldTypes.Key, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Key { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-maintenance-3";

        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Color { get; set; } = "#000000";

        [FormField(LabelResource: ManufacturingResources.Names.MachineToolHead_CurrentNozzle, WaterMark: ManufacturingResources.Names.MachineToolHead_CurrentNozzle_Select,
            FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, EntityHeaderPickerUrl: "/api/mfg/pnp/nozzletips    ", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<ToolNozzleTip> CurrentNozzle { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.MachineToolHead_Type, WaterMark: ManufacturingResources.Names.MachineToolHead_Type_Select, EnumType: typeof(ToolHeadTypes), FieldType: FieldTypes.Picker, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<ToolHeadTypes> ToolHeadType { get; set; }

        private Point2D<double> _offset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.Common_Offset, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Offset
        {
            get => _offset;
            set => Set(ref _offset, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.MachineToolHead_PickHeight, HelpResource: ManufacturingResources.Names.MachineToolHead_PickHeight_Help, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.MachineToolHead_PlaceHeight, HelpResource: ManufacturingResources.Names.MachineToolHead_PlaceHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PlaceHeight { get; set; }

        /// <summary>
        /// As the nozzle rotates through 0-360 degrees the point of the nozzle tip will not be exaclty centered.  Sinze we are attempting to acheive 0.1mm 
        /// accuracy we need to account for this.  Think about a pencil if it was perfectly vertical and you rotate it, it will always poinat 
        /// at the same location.  If it' soff by a couple of degrees from vetical, it will point at a different location as you rotate it.  This is the same thing.
        /// </summary>
        public List<RotationOffset> RotationOffsets { get; set; } = new List<RotationOffset>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(ToolHeadType),
                nameof(Color),
                nameof(CurrentNozzle),
                nameof(Offset),
                nameof(PickHeight),
                nameof(PlaceHeight),
            };
        }
    }

    public class RotationOffset
    {
        public double Angle { get; set; }
        public Point2D<double> Offset { get; set; }
    }
}
