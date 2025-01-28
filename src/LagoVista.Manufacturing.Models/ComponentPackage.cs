using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LagoVista.Manufacturing.Models
{
    public enum PackageTypes
    {
        [EnumLabel(ComponentPackage.PartType_ThroughHole, ManufacturingResources.Names.PartType_ThroughHole, typeof(ManufacturingResources))]
        ThroughHole,
        [EnumLabel(ComponentPackage.PartType_SurfaceMount, ManufacturingResources.Names.PartType_SurfaceMount, typeof(ManufacturingResources))]
        SurfaceMount,
        [EnumLabel(ComponentPackage.PartType_Hardware, ManufacturingResources.Names.PartType_Hardware, typeof(ManufacturingResources))]
        Hardware
    }

    public enum TapeSizes
    {
        [EnumLabel(ComponentPackage.TapeSize8, ManufacturingResources.Names.TapeSize_8, typeof(ManufacturingResources))]
        EightMM,
        [EnumLabel(ComponentPackage.TapeSize12, ManufacturingResources.Names.TapeSize_12, typeof(ManufacturingResources))]
        TwelveMM,
        [EnumLabel(ComponentPackage.TapeSize16, ManufacturingResources.Names.TapeSize_16, typeof(ManufacturingResources))]
        SixteenMM,
        [EnumLabel(ComponentPackage.TapeSize20, ManufacturingResources.Names.TapeSize_20, typeof(ManufacturingResources))]
        TwentyMM,
        [EnumLabel(ComponentPackage.TapeSize24, ManufacturingResources.Names.TapeSize_24, typeof(ManufacturingResources))]
        TwentyFourMM,
        [EnumLabel(ComponentPackage.TapeSize32, ManufacturingResources.Names.TapeSize_32, typeof(ManufacturingResources))]
        ThirtyTwoMM,
        [EnumLabel(ComponentPackage.TapeSize44, ManufacturingResources.Names.TapeSize_44, typeof(ManufacturingResources))]
        FortyFourMM,
    }

    public enum TapePitches
    {
        [EnumLabel(ComponentPackage.TapePitch2, ManufacturingResources.Names.TapePitch_2, typeof(ManufacturingResources))]
        TwoMM,
        [EnumLabel(ComponentPackage.TapePitch4, ManufacturingResources.Names.TapePitch_4, typeof(ManufacturingResources))]
        FourMM,
        [EnumLabel(ComponentPackage.TapePitch8, ManufacturingResources.Names.TapePitch_8, typeof(ManufacturingResources))]
        EightMM,
        [EnumLabel(ComponentPackage.TapePitch12, ManufacturingResources.Names.TapePitch_12, typeof(ManufacturingResources))]
        TwelveMM,
        [EnumLabel(ComponentPackage.TapePitch16, ManufacturingResources.Names.TapePitch_16, typeof(ManufacturingResources))]
        SixteenMM,
        [EnumLabel(ComponentPackage.TapePitch20, ManufacturingResources.Names.TapePitch_20, typeof(ManufacturingResources))]
        TwentyMM,
        [EnumLabel(ComponentPackage.TapePitch24, ManufacturingResources.Names.TapePitch_24, typeof(ManufacturingResources))]
        TwentyFourMM,
        [EnumLabel(ComponentPackage.TapePitch28, ManufacturingResources.Names.TapePitch_28, typeof(ManufacturingResources))]
        TwentyEightMM,
        [EnumLabel(ComponentPackage.TapePitch32, ManufacturingResources.Names.TapePitch_32, typeof(ManufacturingResources))]
        ThirtyTwoMM,
    }

    public enum TapeColors
    {
        [EnumLabel(ComponentPackage.ComponentPackage_TapeColor_Black, ManufacturingResources.Names.ComponentPackage_TapeColor_Black, typeof(ManufacturingResources))]
        Black,

        [EnumLabel(ComponentPackage.ComponentPackage_TapeColor_White, ManufacturingResources.Names.ComponentPackage_TapeColor_White, typeof(ManufacturingResources))]
        White,

        [EnumLabel(ComponentPackage.ComponentPackage_TapeColor_Clear, ManufacturingResources.Names.ComponentPackage_TapeColor_Clear, typeof(ManufacturingResources))]
        Clear,
    }

    public enum TapeMaterialTypes
    {
        [EnumLabel(ComponentPackage.ComponentPackage_MaterialType_Paper, ManufacturingResources.Names.ComponentPackage_MaterialType_Paper, typeof(ManufacturingResources))]
        Paper,

        [EnumLabel(ComponentPackage.ComponentPackage_MaterialType_Plastic, ManufacturingResources.Names.ComponentPackage_MaterialType_Plastic, typeof(ManufacturingResources))]
        Plastic,
    }

    public enum TapeRotations
    {
        [EnumLabel(ComponentPackage.TapeRotation0, ManufacturingResources.Names.TapeRotation_0, typeof(ManufacturingResources))]
        Zero,
        [EnumLabel(ComponentPackage.TapeRotation90, ManufacturingResources.Names.TapeRotation_90, typeof(ManufacturingResources))]
        Ninety,
        [EnumLabel(ComponentPackage.TapeRotationMinus90, ManufacturingResources.Names.TapeRotation_Minus90, typeof(ManufacturingResources))]
        MinusNinety,
        [EnumLabel(ComponentPackage.TapeRotation180, ManufacturingResources.Names.TapeRotation_180, typeof(ManufacturingResources))]
        OneEighty
    }
    
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentPackage_TItle, ManufacturingResources.Names.ComponentPackage_Description,
            ManufacturingResources.Names.ComponentPackage_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/component/package", GetUrl: "/api/mfg/component/package/{id}", GetListUrl: "/api/mfg/component/packages", FactoryUrl: "/api/mfg/component/package/factory", DeleteUrl: "/api/mfg/component/package/{id}",
            ListUIUrl: "/mfg/component/packages", EditUIUrl: "/mfg/component/package/{id}", CreateUIUrl: "/mfg/component/package/add", CanExport:true, CanImport:true)]
    public class ComponentPackage : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, IFormAdditionalActions, ISummaryFactory, IIDEntity, IFormConditionalFields
    {
        public const string PartType_ThroughHole = "throughhole";
        public const string PartType_SurfaceMount = "surfacemount";
        public const string PartType_Hardware = "hardware";

        public const string TapeSize8 = "eight";
        public const string TapeSize12 = "twelve";
        public const string TapeSize16 = "sixteen";
        public const string TapeSize20 = "twenty";
        public const string TapeSize24= "twentyfour";
        public const string TapeSize32 = "thirtytwo";
        public const string TapeSize44 = "fortyfour";

        public const string TapePitch2 = "twomm";
        public const string TapePitch4 = "fourmm";
        public const string TapePitch8 = "eightmm";
        public const string TapePitch12 = "twelvemm";
        public const string TapePitch16 = "sixteenmm";
        public const string TapePitch20 = "twentymm";
        public const string TapePitch24 = "twentyfourmm";
        public const string TapePitch28 = "twentyeightmm";
        public const string TapePitch32 = "thirtytwomm";

        public const string TapeRotation0 = "zero";
        public const string TapeRotation90 = "ninety";
        public const string TapeRotationMinus90 = "minusninety";
        public const string TapeRotation180 = "oneeighty";

        public const string ComponentPackage_TapeColor_Black = "black";
        public const string ComponentPackage_TapeColor_White = "white";
        public const string ComponentPackage_TapeColor_Clear = "clear";

        public const string ComponentPackage_MaterialType_Plastic = "plastic";
        public const string ComponentPackage_MaterialType_Paper = "paper";

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeMaterialType, FieldType: FieldTypes.Picker, EnumType: typeof(TapeMaterialTypes),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeMaterialType_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeMaterialTypes> TapeMaterialType {get; set;} = EntityHeader<TapeMaterialTypes>.Create(TapeMaterialTypes.Paper);

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeColor, FieldType: FieldTypes.Picker, EnumType: typeof(TapeColors),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeColor_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeColors> TapeColor { get; set; } = EntityHeader<TapeColors>.Create(TapeColors.White);

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PackageId, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string PackageId { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-stamp-2";


        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Width { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartLength, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Length { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Height { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_XOffsetReferenceHole, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public double? XOffsetFromReferenceHole { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_YOffsetReferenceHole, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public double? YOffsetFromReferenceHole { get; set; }

        public PcbPackage Layout { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSize, FieldType: FieldTypes.Picker, EnumType:typeof(TapeSizes), 
            WaterMark:ManufacturingResources.Names.ComponentPackage_TapeSize_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeSizes> TapeSize { get; set; } 

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeRotation, FieldType: FieldTypes.Picker, EnumType: typeof(TapeRotations),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeRotation_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeRotations> TapeRotation { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapePitch, FieldType: FieldTypes.Picker, EnumType: typeof(TapePitches),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapePitch_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapePitches> TapePitch { get; set; }

        private bool _dualHoles = false;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_DualHoles, HelpResource: ManufacturingResources.Names.StripFeeder_DualHoles_Help,
            FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool DualHoles
        {
            get => _dualHoles;
            set => Set(ref _dualHoles, value);
        }



        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeAndReelSpecImage, FieldType: FieldTypes.FileUpload, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader TapeAndReelSpecImage { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeAndReelActualImage, FieldType: FieldTypes.FileUpload, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader TapeAndReelActualImage { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_Verified, HelpResource:ManufacturingResources.Names.ComponentPackage_Verified_Help, FieldType: FieldTypes.CheckBox, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public bool Verified { get; set; }

        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_SpacingX, FieldType: FieldTypes.Decimal, IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public double? SpacingX { get
            {
                if (EntityHeader.IsNullOrEmpty(TapePitch))
                    return null;

                switch (TapePitch.Value)
                {
                    case TapePitches.TwoMM: return 2;
                    case TapePitches.FourMM: return 4;
                    case TapePitches.EightMM: return 8;
                    case TapePitches.TwelveMM: return 12;
                    case TapePitches.SixteenMM: return 16;
                    case TapePitches.TwentyMM: return 20;
                    case TapePitches.TwentyFourMM: return 24;
                    case TapePitches.TwentyEightMM: return 28;
                    case TapePitches.ThirtyTwoMM: return 32;
                }
                return null;
            }
        }


        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_HoleSpacing, FieldType: FieldTypes.Decimal,
            IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public double HoleSpacing { get { return 4; } }


        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_CenterX, 
                HelpResource: ManufacturingResources.Names.ComponentPackage_CenterX_Help, FieldType: FieldTypes.Decimal,
                IsRequired: false, IsUserEditable:false, 
                ResourceType: typeof(ManufacturingResources))]
        public double? CenterX
        {
            get
            {
                return (SpacingX.HasValue) ? SpacingX + 2 : null;
            }
        }

        [JsonIgnore]
        public Point2D<double> PickLocation
        {
            get
            {
                if (EntityHeader.IsNullOrEmpty(TapeSize))
                    return null;

                return new Point2D<double>(CenterX.Value, CenterY.Value);
            }
        }

        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_CenterY, 
            HelpResource: ManufacturingResources.Names.ComponentPackage_CenterY_Help, 
            FieldType: FieldTypes.Decimal, IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public double? CenterY 
        { 
            get
            {
                if (EntityHeader.IsNullOrEmpty(TapeSize))
                    return null;

                switch (TapeSize.Value)
                {
                    case TapeSizes.EightMM: return 1.75 + 3.5;
                    case TapeSizes.TwelveMM: return 1.75 + 5.5;
                    case TapeSizes.SixteenMM: return 1.75 + 7.5;
                    case TapeSizes.TwentyMM: return 1.75 + 9.5;
                    case TapeSizes.TwentyFourMM: return 1.75 + 11.5;
                    case TapeSizes.ThirtyTwoMM: return 1.75 + 15.5;
                    case TapeSizes.FortyFourMM: return 1.75 + 21.5;
                }

                return null;
            }
        }

        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeWidth, 
             FieldType: FieldTypes.Decimal, IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public double? TapeWidth
        {
            get
            {
                if (EntityHeader.IsNullOrEmpty(TapeSize))
                    return null;

                switch (TapeSize.Value)
                {
                    case TapeSizes.EightMM: return 8;
                    case TapeSizes.TwelveMM: return 12;
                    case TapeSizes.SixteenMM: return 16;
                    case TapeSizes.TwentyMM: return 20;
                    case TapeSizes.TwentyFourMM: return 24;
                    case TapeSizes.ThirtyTwoMM: return 32;
                    case TapeSizes.FortyFourMM: return 44;
                }

                return null;
            }
        }


        EntityHeader _nozzleTip;
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_NozzleTip, HelpResource:ManufacturingResources.Names.ComponentPackage_NozzleTip_Help, FieldType: FieldTypes.EntityHeaderPicker, 
            EntityHeaderPickerUrl: "/api/mfg/pnp/nozzletips", IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader NozzleTip
        {
            get => _nozzleTip;
            set => Set(ref _nozzleTip, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_SpecificationPage, FieldType: FieldTypes.WebLink, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string SpecificationPage { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSpecificationPage, FieldType: FieldTypes.WebLink, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string TapeSpecificationPage { get; set; }


        private EntityHeader<PackageTypes> _packageType;
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartType, EnumType:typeof(PackageTypes), WaterMark: ManufacturingResources.Names.ComponentPackage_PartType_Select, FieldType: FieldTypes.Picker, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<PackageTypes> PackageType 
        {
            get => _packageType;
            set => Set(ref _packageType, value);
        }

        private double? _pickVacuumLevel;
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PickVacuumLevel, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public double? PickVacuumLevel
        {
            get => _pickVacuumLevel;
            set => Set(ref _pickVacuumLevel, value);
        }

        public ComponentPackageSummary CreateSummary()
        {
            return new ComponentPackageSummary()
            {
                Description = Description,
                Icon = Icon,
                Name = Name,
                Key = Key,
                Id = Id,
                IsPublic = IsPublic,
                PackageId = PackageId,
                Verified = Verified,
                IsSurfaceMount = PackageType.Value == PackageTypes.SurfaceMount,
                HasLayout = Layout != null
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Verified),
                nameof(PackageId),
                nameof(Width),
                nameof(Length),
                nameof(Height),
                nameof(PackageType),
                nameof(TapeAndReelSpecImage),
                nameof(TapeAndReelActualImage)
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(SpecificationPage),
                nameof(TapeSpecificationPage),
                nameof(TapeSize),
                nameof(TapePitch),
                nameof(TapeRotation),
                nameof(TapeColor),
                nameof(TapeMaterialType),
                nameof(DualHoles),
                nameof(PickVacuumLevel),
                nameof(XOffsetFromReferenceHole),
                nameof(YOffsetFromReferenceHole),
                nameof(NozzleTip),
            };
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(TapeSize), nameof(TapePitch), nameof(TapeRotation), nameof(TapeMaterialType), nameof(TapeColor), nameof(Width), nameof(Height), nameof(Length) },
                Conditionals = new List<FormConditional>()
                {
                    new FormConditional()
                    {
                        Field = nameof(PackageType),
                        Value = PartType_SurfaceMount,
                        RequiredFields = new List<string>() {nameof(TapeSize), nameof(TapePitch), nameof(TapeRotation), nameof(TapeMaterialType), nameof(TapeColor), nameof(Width), nameof(Height), nameof(Length)},
                        VisibleFields = new List<string>() {nameof(TapeSize), nameof(TapePitch), nameof(TapeRotation), nameof(TapeMaterialType), nameof(TapeColor), nameof(Width), nameof(Height), nameof(Length) }
                    }
                }
            };
        }

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()
            {
                new FormAdditionalAction()
                {
                    Title = "Map",
                     Key = "map",
                     Icon = "fa fa-search-plus"
                }
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentPackage_TItle, ManufacturingResources.Names.ComponentPackage_Description,
            ManufacturingResources.Names.ComponentPackage_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/component/package", GetUrl: "/api/component/package/{id}", GetListUrl: "/api/component/packages", FactoryUrl: "/api/component/package/factory", DeleteUrl: "/api/component/package/{id}",
            ListUIUrl: "/mfg/component/packages", EditUIUrl: "/mfg/component/package/{id}", CreateUIUrl: "/mfg/component/package/add")]
    public class ComponentPackageSummary : SummaryData
    {
        public string PackageId { get; set; }
        public bool Verified { get; set; }
        public bool HasLayout { get; set; }
        public bool IsSurfaceMount { get; set; }
    }

    // using System.Xml.Serialization;
    // XmlSerializer serializer = new XmlSerializer(typeof(OpenpnpPackages));
    // using (StringReader reader = new StringReader(xml))
    // {
    //    var test = (OpenpnpPackages)serializer.Deserialize(reader);
    // }

    [XmlRoot(ElementName = "pad")]
    public class OpenPnPPad
    {

        [XmlAttribute(AttributeName = "name")]
        public int Name { get; set; }

        [XmlAttribute(AttributeName = "x")]
        public decimal X { get; set; }

        [XmlAttribute(AttributeName = "y")]
        public decimal Y { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public decimal Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public decimal Height { get; set; }

        [XmlAttribute(AttributeName = "rotation")]
        public decimal Rotation { get; set; }

        [XmlAttribute(AttributeName = "roundness")]
        public decimal Roundness { get; set; }
    }

    [XmlRoot(ElementName = "footprint")]
    public class OpenPnPFootPrint
    {

        [XmlElement(ElementName = "pad")]
        public List<OpenPnPPad> Pad { get; set; }

        [XmlAttribute(AttributeName = "units")]
        public string Units { get; set; }

        [XmlAttribute(AttributeName = "body-width")]
        public double BodyWidth { get; set; }

        [XmlAttribute(AttributeName = "body-height")]
        public double BodyHeight { get; set; }

        [XmlAttribute(AttributeName = "outer-dimension")]
        public double OuterDimension { get; set; }

        [XmlAttribute(AttributeName = "inner-dimension")]
        public double InnerDimension { get; set; }

        [XmlAttribute(AttributeName = "pad-count")]
        public int PadCount { get; set; }

        [XmlAttribute(AttributeName = "pad-pitch")]
        public decimal PadPitch { get; set; }

        [XmlAttribute(AttributeName = "pad-across")]
        public decimal PadAcross { get; set; }

        [XmlAttribute(AttributeName = "pad-roundness")]
        public decimal PadRoundness { get; set; }
    }

    [XmlRoot(ElementName = "compatible-nozzle-tip-ids")]
    public class OpenPnPNozzleTip
    {

        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }

        [XmlElement(ElementName = "string")]
        public string String { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "package")]
    public class OpenPnPPackage
    {

        [XmlElement(ElementName = "footprint")]
        public OpenPnPFootPrint Footprint { get; set; }

        [XmlElement(ElementName = "compatible-nozzle-tip-ids")]
        public OpenPnPNozzleTip Compatiblenozzletipids { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public DateTime Version { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "pick-vacuum-level")]
        public decimal PickVacuumLevel { get; set; }

        [XmlAttribute(AttributeName = "place-blow-off-level")]
        public decimal PlaceBlowOffLevel { get; set; }

        [XmlAttribute(AttributeName = "description")]
        public string Description { get; set; }

        [XmlText]
        public string Text { get; set; }

        public static OpenPnPPackage Create(ComponentPackage package)
        {
            return new OpenPnPPackage()
            {
                Description = package.Description,
                Id = package.PackageId,
                Version = DateTime.Now,
                PickVacuumLevel = 0.5m,
                PlaceBlowOffLevel = 0.5m,
                Footprint = new OpenPnPFootPrint()
                {
                    Units = "mm",
                    BodyWidth = package.Width,
                    BodyHeight = package.Height,
                    OuterDimension = package.Length,
                    InnerDimension = package.Width,
                    PadCount = 2,
                    PadPitch = 2,
                    PadAcross = 2,
                    PadRoundness = 0m,
                    Pad = new List<OpenPnPPad>()
                    {
                        new OpenPnPPad()
                        {
                            Name = 1,
                            X = 0,
                            Y = 0,
                            Width = 1,
                            Height = 1,
                            Rotation = 0,
                            Roundness = 0
                        },
                        new OpenPnPPad()
                        {
                            Name = 2,
                            X = 1,
                            Y = 1,
                            Width = 1,
                            Height = 1,
                            Rotation = 0,
                            Roundness = 0
                        }
                    }
                }
            };
        }
    }

    [XmlRoot(ElementName = "openpnp-packages")]
    public class OpenPnPPackages
    {

        [XmlElement(ElementName = "package")]
        public List<OpenPnPPackage> Packages { get; set; }
    }


}
