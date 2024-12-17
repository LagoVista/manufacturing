using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public enum FeederOrientations
    {
        [EnumLabel(StripFeeder.FeederOrientation_Horizontal, ManufacturingResources.Names.FeederOrientation_Horizontal, typeof(ManufacturingResources))]
        Horizontal,
        [EnumLabel(StripFeeder.FeederOrientation_Vertical, ManufacturingResources.Names.FeederOrientation_Vertical, typeof(ManufacturingResources))]
        Vertical,
    }

    public enum FeedDirections
    {
        [EnumLabel(StripFeeder.FeedDirection_Forwards, ManufacturingResources.Names.FeedDirection_Forwards, typeof(ManufacturingResources))]
        Forward,
        [EnumLabel(StripFeeder.FeedDirection_Backwards, ManufacturingResources.Names.FeedDirection_Backwards, typeof(ManufacturingResources))]
        Backwards,
    }


    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeeder_Title, ManufacturingResources.Names.StripFeeder_Description,
               ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-left", Cloneable: true,
               SaveUrl: "/api/mfg/stripfeeder", GetUrl: "/api/mfg/stripfeeder/{id}", GetListUrl: "/api/mfg/stripfeeders", FactoryUrl: "/api/mfg/stripfeeder/factory",
               DeleteUrl: "/api/mfg/stripfeeder/{id}", ListUIUrl: "/mfg/Feeder/s", EditUIUrl: "/mfg/stripfeeder/{id}", CreateUIUrl: "/mfg/stripfeeder/add")]
    public class StripFeeder : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity, IFormConditionalFields
    {
        public const string FeederOrientation_Horizontal = "horizontal";
        public const string FeederOrientation_Vertical = "vertical";
        
        public const string FeedDirection_Forwards = "forwards";
        public const string FeedDirection_Backwards = "backwards";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-left";


        private EntityHeader<Component> _component;
        public EntityHeader<Component> Component
        {
            get => _component;
            set => Set(ref _component, value);
        }

        private EntityHeader<ComponentPackage> _package;
        public EntityHeader<ComponentPackage> Package
        {
            get => _package;
            set => Set(ref _package, value);
        }

        private EntityHeader<PcbComponent> _pcbComponent;
        public EntityHeader<PcbComponent> PcbComponent
        {
            get => _pcbComponent;
            set => Set(ref _pcbComponent, value);
        }

        private int _currentPartIndex;
        public int CurrentPartIndex
        {
            get => _currentPartIndex;
            set => Set(ref _currentPartIndex, value);
        }

        private bool _installed;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Installed, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool Installed
        {
            get => _installed;
            set => Set(ref _installed, value);
        }
        
        private EntityHeader<FeederOrientations> _orientation;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Orientation, FieldType: FieldTypes.Picker, EnumType: typeof(FeederOrientations), WaterMark: ManufacturingResources.Names.StripFeeder_Orientation_Select, IsRequired:true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederOrientations> Orientation
        {
            get => _orientation;
            set => Set(ref _orientation, value);
        }

        private EntityHeader<FeedDirections> _feedDirection;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Direction, FieldType: FieldTypes.Picker, EnumType:typeof(FeedDirections), WaterMark:ManufacturingResources.Names.StripFeeder_Direction_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeedDirections> FeedDirection
        {
            get => _feedDirection;
            set => Set(ref _feedDirection, value);
        }

        private double _pickHeight = 10;

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_PickHeight, FieldType: FieldTypes.Decimal,IsRequired:true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }


        private decimal? _angleOffset;
        private decimal? AngleOffset
        {
            get { return _angleOffset; }
            set => Set(ref _angleOffset, value);    
        }

        private decimal? _bottomY;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_BottomY, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? BottomY
        {
            get => _bottomY;
            set => Set(ref _bottomY, value);        
        }

        private decimal? _leftX;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_FirstHole_X, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? LeftX
        {
            get => _leftX;
            set => Set(ref _leftX, value);
        }

        private decimal? _firstHoleY;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_FirstHole_Y, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? FirstHoleY
        {
            get => _firstHoleY;
            set => Set(ref _firstHoleY, value);
        }

        private decimal? _firstHoleX;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_FirstHole_X, FieldType: FieldTypes.Decimal,  ResourceType: typeof(ManufacturingResources))]
        public decimal? FirstHoleX
        {
            get => _firstHoleX;
            set => Set(ref _firstHoleX, value);
        }

        private EntityHeader<TapeSizes> _tapeSize; 
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSize, FieldType: FieldTypes.Picker, EnumType: typeof(TapeSizes),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeSize_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeSizes> TapeSize
        {
            get => _tapeSize;
            set => Set(ref _tapeSize, value);
        }

        private double _length;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Length, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        public double _width;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        private double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Description),
                nameof(Installed),
                nameof(TapeSize),
                nameof(Orientation),
                nameof(FeedDirection),
                nameof(PickHeight),
            };
        }


        public StripFeederSummary CreateSummary()
        {
            return new StripFeederSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                Description = Description,
                Key = Key,
                IsPublic = IsPublic,
                TapeSize = TapeSize.Text,
                TapeSizeId = TapeSize.Id
            };
        }


        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(PickHeight),
                nameof(LeftX),
                nameof(BottomY),
                nameof(Width),
                nameof(Length),
                nameof(FirstHoleX),
                nameof(FirstHoleY)
            };            
        }

        [CustomValidator()]
        public void Validate(ValidationResult validationResult)
        {
            if (Installed)
            {
                if (!LeftX.HasValue) validationResult.AddUserError("Left X is required for installed strip feeder.");
                if (!BottomY.HasValue) validationResult.AddUserError("Bottom Y is required for installed strip feeder.");
                if (!FirstHoleX.HasValue) validationResult.AddUserError("First Hole X is required for installed strip feeder.");
                if (!FirstHoleY.HasValue) validationResult.AddUserError("First Hole Y is required for installed strip feeder.");
            }
            else
            {
                LeftX = null;
                BottomY = null;
                FirstHoleX = null;
                FirstHoleY = null;
            }
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(LeftX), nameof(BottomY), nameof(FirstHoleX), nameof(FirstHoleY) },
                Conditionals = new List<FormConditional>()
                {
                     new FormConditional()
                     {
                         RequiredFields = new List<string>() { nameof(LeftX), nameof(BottomY), nameof(FirstHoleX), nameof(FirstHoleY) },
                         VisibleFields =  new List<string>() { nameof(LeftX), nameof(BottomY), nameof(FirstHoleX), nameof(FirstHoleY) },
                         Field = nameof(Installed),
                         Value = "true",
                     }
                }
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeeders_Title, ManufacturingResources.Names.StripFeeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/feeder", GetUrl: "/api/mfg/feeder/{id}", GetListUrl: "/api/mfg/feeder", FactoryUrl: "/api/feeder/factory",
            DeleteUrl: "/api/mfg/feeder/{id}", ListUIUrl: "/mfg/feeders", EditUIUrl: "/mfg/feeder/{id}", CreateUIUrl: "/mfg/feeder/add")]
    public class StripFeederSummary : SummaryData
    {
        public string TapeSize { get; set; }
        public string TapeSizeId { get; set; }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeederRow_Title, ManufacturingResources.Names.StripFeederRow_Description,
               ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-left", Cloneable: true,
               SaveUrl: "/api/mfg/Feeder/", GetUrl: "/api/mfg/Feeder//{id}", GetListUrl: "/api/mfg/Feeder/s", FactoryUrl: "/api/mfg/strip/row/factory")]
    public class StripFeederRow : ModelBase
    {
        public StripFeederRow()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        private int _index;
        public int Index
        {
            get => _index;
            set => Set(ref _index, value);
        }

        private double? _refHoleXOffset;
        private double _refHoleYOffset;

        public double? RefHoleXOffset
        {
            get => _refHoleXOffset;
            set => Set(ref _refHoleXOffset, value);
        }

        public double RefHoleYOffset
        {
            get => _refHoleYOffset;
            set => Set(ref _refHoleYOffset, value);
        }        
    }
}
