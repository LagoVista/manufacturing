// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d45b5e6dc8399de98005946bd0505d7e8fca586d9622c04b50984dc6f1e0686e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Resources;
using Newtonsoft.Json.Linq;
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

        private int _numberSlots = 25;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_NumberSlots, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]        
        public int NumberSlots
        {
            get => _numberSlots;
            set => Set(ref _numberSlots, value);
        }

        private double _slotWidth = 15;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_SlotWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double SlotWidth
        {
            get => _slotWidth;
            set => Set(ref _slotWidth, value);
        }

        private int _slotStartIndex;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_StartSlotIndex, HelpResource: ManufacturingResources.Names.Machine_FeederRail_StartSlotIndex_Help, 
            FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int SlotStartIndex
        {
            get => _slotStartIndex;
            set => Set(ref _slotStartIndex, value);
        }

        private EntityHeader<FeederRotations> _rotation = EntityHeader<FeederRotations>.Create(FeederRotations.Zero);
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Rotation, FieldType: FieldTypes.Picker, EnumType: typeof(FeederRotations), IsRequired: true,
            WaterMark: ManufacturingResources.Names.Feeder_Rotation_Select, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederRotations> Rotation
        {
            get => _rotation;
            set => Set(ref _rotation, value);
        }


        Point2D<double> _firstFeederOrigin = new Point2D<double>(0,0);
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_FirstFeederOrigin, IsRequired: true, HelpResource: ManufacturingResources.Names.Machine_FeederRail_FirstFeederOrigin_Help,
            FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FirstFeederOrigin 
        {
            get => _firstFeederOrigin;
            set => Set(ref _firstFeederOrigin, value);
        }

        private double _railWidth = 600;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RailWidth
        {
            get => _railWidth;
            set => Set(ref _railWidth, value);
        }

        private double _railHeight = 20;
        [FormField(LabelResource: ManufacturingResources.Names.Machine_FeederRail_Height, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RailHeight 
        {
            get => _railHeight;
            set => Set(ref _railHeight, value);
        } 


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
