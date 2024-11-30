using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
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

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.ComponentPackage_TItle, ManufacturingResources.Names.ComponentPackage_Description,
            ManufacturingResources.Names.ComponentPackage_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/component/package", GetUrl: "/api/mfg/component/package/{id}", GetListUrl: "/api/mfg/component/packages", FactoryUrl: "/api/mfg/component/package/factory", DeleteUrl: "/api/mfg/component/package/{id}",
            ListUIUrl: "/mfg/component/packages", EditUIUrl: "/mfg/component/package/{id}", CreateUIUrl: "/mfg/component/package/add")]
    public class ComponentPackage : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity
    {
        public const string PartType_ThroughHole = "throughhole";
        public const string PartType_SurfaceMount = "surfacemount";
        public const string PartType_Hardware = "hardware";




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

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal TapeWidth { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_Rotation, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int Rotation { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_SpacingX, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal SpacingX { get; set; } = 4;

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_HoleSpacing, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal HoleSpacing { get; set; } = 4;

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_CenterX, HelpResource: ManufacturingResources.Names.ComponentPackage_CenterX_Help, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal CenterX { get; set; } = 2;

        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_CenterY, HelpResource: ManufacturingResources.Names.ComponentPackage_CenterY_Help, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal CenterY { get; set; } = 6;


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
                nameof(Width),
                nameof(Length),
                nameof(Height),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(SpecificationPage),
                nameof(Rotation),
                nameof(TapeWidth),
                nameof(SpacingX),
                nameof(HoleSpacing),
                nameof(CenterX),
                nameof(CenterY),
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
