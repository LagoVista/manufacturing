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
        ManufacturingResources.Names.Machine_FeederRail_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources),
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

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_NumberSlots, FieldType: FieldTypes.Integer, ResourceType: typeof(ManufacturingResources))]
        public int NumberSlots { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_SlotWidth, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double SlotWidth { get; set; }

        private EntityHeader<FeederRotations> _rotation;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Rotation, FieldType: FieldTypes.Picker, EnumType: typeof(FeederRotations), IsRequired: true,
            WaterMark: ManufacturingResources.Names.Feeder_Rotation_Select, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederRotations> Rotation
        {
            get => _rotation;
            set => Set(ref _rotation, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_FirstFeederOffset, HelpResource:ManufacturingResources.Names.Machine_FeederRail_FirstFeederOffset_Help, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double FirstFeederOffset { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Origin, HelpResource: ManufacturingResources.Names.Machine_FeederRail_Origin_Help, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Width, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double RailWidth { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Height, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double RailHeight { get; set; }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                  nameof(Name),
                  nameof(Key),
                  nameof(Icon),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Origin),
                nameof(RailWidth),
                nameof(RailHeight),
                nameof(FirstFeederOffset),
                nameof(NumberSlots),
                nameof(SlotWidth),
            };
        }
    }
}
