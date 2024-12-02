using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
            ManufacturingResources.Names.ComponentPackage_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/component/package", GetUrl: "/api/mfg/component/package/{id}", GetListUrl: "/api/mfg/component/packages", FactoryUrl: "/api/mfg/component/package/factory", DeleteUrl: "/api/mfg/component/package/{id}",
            ListUIUrl: "/mfg/component/packages", EditUIUrl: "/mfg/component/package/{id}", CreateUIUrl: "/mfg/component/package/add", CanExport:true, CanImport:true)]
    public class ComponentPackage : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity, IFormConditionalFields
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
        public const string TapePitch4 = "twomm";
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

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PackageId, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string PackageId { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-stamp-2";


        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Width { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartLength, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Length { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal Height { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSize, FieldType: FieldTypes.Picker, EnumType:typeof(TapeSizes), 
            WaterMark:ManufacturingResources.Names.ComponentPackage_TapeSize_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeSizes> TapeSize { get; set; } 

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeRotation, FieldType: FieldTypes.Picker, EnumType: typeof(TapeRotations),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeRotation_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeRotations> TapeRotation { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapePitch, FieldType: FieldTypes.Picker, EnumType: typeof(TapePitches),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapePitch_Select, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapePitches> TapePitch { get; set; }

        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_SpacingX, FieldType: FieldTypes.Decimal, IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public decimal? SpacingX { get
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
        public decimal HoleSpacing { get { return 4; } }


        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_CenterX, 
                HelpResource: ManufacturingResources.Names.ComponentPackage_CenterX_Help, FieldType: FieldTypes.Decimal,
                IsRequired: false, IsUserEditable:false, 
                ResourceType: typeof(ManufacturingResources))]
        public decimal? CenterX
        {
            get
            {
                return (SpacingX.HasValue) ? SpacingX + 2 : null;
            }
        }

        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_CenterY, 
            HelpResource: ManufacturingResources.Names.ComponentPackage_CenterY_Help, 
            FieldType: FieldTypes.Decimal, IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public decimal? CenterY 
        { 
            get
            {
                if (EntityHeader.IsNullOrEmpty(TapeSize))
                    return null;

                switch (TapeSize.Value)
                {
                    case TapeSizes.EightMM: return 1.75m + 3.5m;
                    case TapeSizes.TwelveMM: return 1.75m + 5.5m;
                    case TapeSizes.SixteenMM: return 1.75m + 7.5m;
                    case TapeSizes.TwentyMM: return 1.75m + 9.5m;
                    case TapeSizes.TwentyFourMM: return 1.75m + 11.5m;
                    case TapeSizes.ThirtyTwoMM: return 1.75m + 15.5m;
                    case TapeSizes.FortyFourMM: return 1.75m + 21.5m;
                }

                return null;
            }
        }

        [JsonIgnore]
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeWidth, 
             FieldType: FieldTypes.Decimal, IsRequired: false, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public decimal? TapeWidth
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


        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_SpecificationPage, FieldType: FieldTypes.WebLink, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string SpecificationPage { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_PartType, EnumType:typeof(PackageTypes), WaterMark: ManufacturingResources.Names.ComponentPackage_PartType_Select, FieldType: FieldTypes.Picker, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<PackageTypes> PackageType { get; set; }

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
                nameof(PackageId),
                nameof(PackageType),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Width),
                nameof(Length),
                nameof(Height),
                nameof(SpecificationPage),
                nameof(TapeSize),
                nameof(TapePitch),
                nameof(TapeRotation),
            };
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(TapeSize), nameof(TapePitch), nameof(TapeRotation), nameof(Width), nameof(Height), nameof(Length) },
                Conditionals = new List<FormConditional>()
                {
                    new FormConditional()
                    {
                        Field = nameof(PackageType),
                        Value = PartType_SurfaceMount,
                        RequiredFields = new List<string>() {nameof(TapeSize), nameof(TapePitch), nameof(TapeRotation), nameof(Width), nameof(Height), nameof(Length)},
                        VisibleFields = new List<string>() {nameof(TapeSize), nameof(TapePitch), nameof(TapeRotation), nameof(Width), nameof(Height), nameof(Length) }
                    }
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
    }
}
