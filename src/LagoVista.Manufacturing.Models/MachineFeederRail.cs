using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Machine_FeederRail_Title, ManufacturingResources.Names.Machine_FeederRail_Description,
        ManufacturingResources.Names.Machine_FeederRail_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: true,
        FactoryUrl: "/api/mfg/machine/feederrail/factory")]
    public class MachineFeederRail : ModelBase, IFormDescriptor, IFormDescriptorCol2
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Key, FieldType: FieldTypes.Key, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Key { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-control-panel";

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_NumberSlots, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int NumberSlots { get; set; } = 25;

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_SlotWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double SlotWidth { get; set; } = 10;

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_StartSlotIndex, HelpResource: ManufacturingResources.Names.Machine_FeederRail_StartSlotIndex_Help, 
            FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int SlotStartIndex { get; set; } = 1;

        private EntityHeader<FeederRotations> _rotation = EntityHeader<FeederRotations>.Create(FeederRotations.Zero);
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Rotation, FieldType: FieldTypes.Picker, EnumType: typeof(FeederRotations), IsRequired: true,
            WaterMark: ManufacturingResources.Names.Feeder_Rotation_Select, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederRotations> Rotation
        {
            get => _rotation;
            set => Set(ref _rotation, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_FirstFeederOrigin, IsRequired: true, HelpResource: ManufacturingResources.Names.Machine_FeederRail_FirstFeederOrigin_Help,
            FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FirstFeederOrigin { get; set; } = new Point2D<double>(0, 0);

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RailWidth { get; set; } = 600;

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Height, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RailHeight { get; set; } = 20;


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(FirstFeederOrigin),
                nameof(SlotStartIndex),
                nameof(NumberSlots),
                nameof(SlotWidth),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Rotation),
                nameof(RailWidth),
                nameof(RailHeight),
            };
        }
    }
}
