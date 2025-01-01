using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

    public enum FeederProtocols
    {
        [EnumLabel(AutoFeeder.FeederProtocol_Phonton, ManufacturingResources.Names.FeederProtocol_Phonton, typeof(ManufacturingResources))]
        Phonton,

        [EnumLabel(AutoFeeder.FeederProtocol_Other, ManufacturingResources.Names.FeederProtocol_Other, typeof(ManufacturingResources))]
        Other
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Feeder_Title, ManufacturingResources.Names.Feeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), 
            Icon: "icon-pz-searching-2", Cloneable: true,
            SaveUrl: "/api/mfg/autofeeder", GetUrl: "/api/mfg/autoFeeder/{id}", GetListUrl: "/api/mfg/autofeeders", FactoryUrl: "/api/mfg/autofeeder/factory",
            DeleteUrl: "/api/mfg/autofeeder/{id}", ListUIUrl: "/mfg/autoFeeders", EditUIUrl: "/mfg/autofeeder/{id}", CreateUIUrl: "/mfg/autofeeder/add")]
    public class AutoFeeder : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity
    {

        public const string FeederRotation0 = "zero";
        public const string FeederRotation90 = "ninety";
        public const string FeederRotationMinus90 = "minusninety";
        public const string FeederRotation180 = "oneeighty";

        public const string FeederProtocol_Phonton = "photon";
        public const string FeederProtocol_Other = "other";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-searching-2";

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_FeederId, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string FeederId { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Slot, FieldType: FieldTypes.Integer, ResourceType: typeof(ManufacturingResources))]
        public int Slot { get; set; }

        public AutoFeederSummary CreateSummary()
        {
            return new AutoFeederSummary()
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
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickLocation, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> PickLocation
        {
            get => _pickLocation;
            set => Set(ref _pickLocation, value);
        }

        private double _pickHeight;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickHeight, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight 
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }

        private int _partCount;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PartCount, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public int PartCount
        {
            get => _partCount;
            set => Set(ref _partCount, value);
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

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Protocol, FieldType: FieldTypes.Picker, EnumType: typeof(FeederProtocols),
            WaterMark: ManufacturingResources.Names.Feeder_Protocol_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederProtocols> Protocol { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_AdvanceGCode, HelpResource: ManufacturingResources.Names.Feeder_AdvanceGCode_Help, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string AdvanceGCode { get; set; }

        private EntityHeader<FeederRotations> _tapeAngle;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Rotation, FieldType: FieldTypes.Picker, EnumType:typeof(FeederRotations), IsRequired:true, 
            WaterMark:ManufacturingResources.Names.Feeder_Rotation_Select, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederRotations> Rotation
        {
            get => _tapeAngle;
            set => Set(ref _tapeAngle, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string Color
        {
            get; set;
        } = "#000000";


        private double _feederWidth = 16;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederWidth
        {
            get => _feederWidth;
            set => Set(ref _feederWidth, value);
        }

        private double _feederLength = 120;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Length, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederLength
        {
            get => _feederLength;
            set => Set(ref _feederLength, value);
        }

        private double _feederHeight = 80;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Height, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederHeight
        {
            get => _feederHeight;
            set => Set(ref _feederHeight, value);
        }

        Point2D<double> _pickOffset;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickOffsetFromSlotOriign, FieldType: FieldTypes.Point2D,
           HelpResource: ManufacturingResources.Names.Feeder_PickOffsetFromSlotOriign_Help, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> PickOffset
        {
            get => _pickOffset;
            set => Set(ref _pickOffset, value);
        }

        Point2D<double> _fiducitalOffset;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_FiducialOffsetFromSlotOriign, FieldType: FieldTypes.Point2D,
           HelpResource: ManufacturingResources.Names.Feeder_FiducialOffsetFromSlotOriign_Help, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FiducialOffset
        {
            get => _fiducitalOffset;
            set => Set(ref _fiducitalOffset, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Component),
                nameof(PartCount),
                nameof(TapeSize),
                nameof(Protocol),
                nameof(Key),
                nameof(Color),
                nameof(Slot),
                nameof(FeederId),
                nameof(Machine),
                nameof(AdvanceGCode),
                nameof(Rotation),
                nameof(Protocol),
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

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(FeederWidth),
                nameof(FeederLength),
                nameof(FeederHeight),
                nameof(PickOffset),
                nameof(FiducialOffset),
                nameof(PickLocation),
                nameof(PickHeight),
              };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Feeders_Title, ManufacturingResources.Names.Feeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/feeder", GetUrl: "/api/mfg/feeder/{id}", GetListUrl: "/api/mfg/feeders", FactoryUrl: "/api/mfg/feeder/factory",
            DeleteUrl: "/api/mfg/feeder/{id}", ListUIUrl: "/mfg/feeders", EditUIUrl: "/mfg/feeder/{id}", CreateUIUrl: "/mfg/feeder/add")]
    public class AutoFeederSummary : SummaryData
    {
        public string ComponentId { get; set; }
        public string ComponentKey { get; set; }
        public string Component { get; set; }
    }
}
