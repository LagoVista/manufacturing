using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{

    public enum FeederRotations
    {
        [EnumLabel(AutoFeeder.FeederRotation0, ManufacturingResources.Names.TapeRotation_0, typeof(ManufacturingResources))]
        Zero,
        [EnumLabel(AutoFeeder.FeederRotation90, ManufacturingResources.Names.TapeRotation_90, typeof(ManufacturingResources))]
        Ninety,
        [EnumLabel(AutoFeeder.FeederRotationMinus90, ManufacturingResources.Names.TapeRotation_Minus90, typeof(ManufacturingResources))]
        MinusNinety,
        [EnumLabel(AutoFeeder.FeederRotation180, ManufacturingResources.Names.TapeRotation_180, typeof(ManufacturingResources))]
        OneEighty
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Feeder_Title, ManufacturingResources.Names.Feeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), 
            Icon: "icon-pz-searching-2", Cloneable: true,
            SaveUrl: "/api/mfg/feeder", GetUrl: "/api/mfg/Feeder/{id}", GetListUrl: "/api/mfg/feeders", FactoryUrl: "/api/mfg/feeder/factory",
            DeleteUrl: "/api/mfg/feeder/{id}", ListUIUrl: "/mfg/fFeeders", EditUIUrl: "/mfg/feeder/{id}", CreateUIUrl: "/mfg/feeder/add")]
    public class AutoFeeder : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory, IIDEntity
    {

        public const string FeederRotation0 = "zero";
        public const string FeederRotation90 = "ninety";
        public const string FeederRotationMinus90 = "minusninety";
        public const string FeederRotation180 = "oneeighty";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-searching-2";


        [FormField(LabelResource: ManufacturingResources.Names.Feeder_FeederId, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string FeederId { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Slot, FieldType: FieldTypes.Integer, ResourceType: typeof(ManufacturingResources))]
        public int Slot { get; set; }

        public FeederSummary CreateSummary()
        {
            return new FeederSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                Description = Description,
                Key = Key,
                IsPublic = IsPublic,
                Component = Component?.Text,
                ComponentId = Component?.Id,
                ComponentKey = Component?.Key
            };
        }

        private Point2D<double> _pickLocation;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickLocation, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> PickLocation
        {
            get => _pickLocation;
            set => Set(ref _pickLocation, value);
        }

        private double _pickHeight;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickLocation, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight 
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Machine, WaterMark: ManufacturingResources.Names.Feeder_Machine_Select, EntityHeaderPickerUrl: "/api/mfg/machines", FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Machine { get; set; }

        private EntityHeader<TapeSizes> _tapeSize;
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSize, FieldType: FieldTypes.Picker, EnumType: typeof(TapeSizes),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeSize_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeSizes> TapeSize
        {
            get => _tapeSize;
            set => Set(ref _tapeSize, value);
        }

        private EntityHeader<FeederRotations> _tapeAngle;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Rotation, FieldType: FieldTypes.Picker, EnumType:typeof(FeederRotations), IsRequired:true, 
            WaterMark:ManufacturingResources.Names.Feeder_Rotation_Select, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederRotations> Rotation
        {
            get => _tapeAngle;
            set => Set(ref _tapeAngle, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Slot),
                nameof(FeederId),
                nameof(Machine),
                nameof(TapeSize),
                nameof(PickLocation),
                nameof(Rotation),
                nameof(Component),
                nameof(Description)
            };
        }

        private EntityHeader<Component> _component;
        [FormField(LabelResource: ManufacturingResources.Names.Component_Title, FieldType: FieldTypes.Custom, CustomFieldType: "componentpicker", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<Component> Component
        {
            get => _component;
            set => Set(ref _component, value);
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Feeders_Title, ManufacturingResources.Names.Feeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/feeder", GetUrl: "/api/mfg/feeder/{id}", GetListUrl: "/api/mfg/feeders", FactoryUrl: "/api/mfg/feeder/factory",
            DeleteUrl: "/api/mfg/feeder/{id}", ListUIUrl: "/mfg/feeders", EditUIUrl: "/mfg/feeder/{id}", CreateUIUrl: "/mfg/feeder/add")]
    public class FeederSummary : SummaryData
    {
        public string ComponentId { get; set; }
        public string ComponentKey { get; set; }
        public string Component { get; set; }
    }
}
