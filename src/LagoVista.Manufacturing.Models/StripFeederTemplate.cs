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
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeederTemplate_Title, ManufacturingResources.Names.StripFeederTemplate_Description,
                 ManufacturingResources.Names.StripFeederTemplate_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-left", Cloneable: true,
                 SaveUrl: "/api/mfg/stripfeeder/template", GetUrl: "/api/mfg/stripfeeder/template/{id}", GetListUrl: "/api/mfg/stripfeeder/templates", FactoryUrl: "/api/mfg/stripfeeder/template/factory",
                 DeleteUrl: "/api/mfg/stripfeeder/template/{id}", ListUIUrl: "/mfg/stripfeedertemplatess", EditUIUrl: "/mfg/stripfeeder/template/{id}", CreateUIUrl: "/mfg/stripfeeder/template/add")]
    public class StripFeederTemplate : MfgModelBase, ISummaryFactory, IFormDescriptor
    {
        public StripFeederTemplateSummary CreateSummary()
        {
            return new StripFeederTemplateSummary()
            {
                Id = Id,
                Key = Key,
                Name = Name,
                Description = Description,
                IsPublic = IsPublic,
            };
        }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-left";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string Color
        {
            get; set;
        } = "#000000";

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> RowOneRefHoleOffset { get; set; }

        private double _pickHeight = 10;

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_PickHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }

        private EntityHeader<TapeSizes> _tapeSize;
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSize, FieldType: FieldTypes.Picker, EnumType: typeof(TapeSizes),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeSize_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeSizes> TapeSize
        {
            get => _tapeSize;
            set => Set(ref _tapeSize, value);
        }

        private double _rowWidth;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowWidth,
            HelpResource: ManufacturingResources.Names.StripFeeder_RowWidth_Help, FieldType:
            FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowWidth
        {
            get => _rowWidth;
            set => Set(ref _rowWidth, value);
        }

        private double _rowCount;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowCount, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowCount
        {
            get => _rowCount;
            set => Set(ref _rowCount, value);
        }

        private double _feederWidth;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Width, HelpResource: ManufacturingResources.Names.StripFeeder_FeederWidth_Help, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederWidth
        {
            get => _feederWidth;
            set => Set(ref _feederWidth, value);
        }

        private double _feederLength = 120;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Length, HelpResource: ManufacturingResources.Names.StripFeeder_FeederLength_Help, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederLength
        {
            get => _feederLength;
            set => Set(ref _feederLength, value);
        }

        private double _feederHeight = 12;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Height, HelpResource: ManufacturingResources.Names.StripFeeder_FeederHeight_Help, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederHeight
        {
            get => _feederHeight;
            set => Set(ref _feederHeight, value);
        }

        Point2D<double> _referenceHoleLocation = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset, 
            HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ReferenceHoleLocation
        {
            get => _referenceHoleLocation;
            set => Set(ref _referenceHoleLocation, value);
        }


        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(TapeSize),
                nameof(Color),
                nameof(RowWidth),
                nameof(RowCount),
                nameof(FeederWidth),
                nameof(FeederLength),
                nameof(FeederHeight),
                nameof(PickHeight),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeederTemplates_Title, ManufacturingResources.Names.StripFeederTemplate_Description,
                 ManufacturingResources.Names.StripFeederTemplate_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-left", Cloneable: true,
                 SaveUrl: "/api/mfg/stripfeeder/template", GetUrl: "/api/mfg/stripfeeder/template/{id}", GetListUrl: "/api/mfg/stripfeeder/templates", FactoryUrl: "/api/mfg/stripfeeder/template/factory",
                 DeleteUrl: "/api/mfg/stripfeeder/template/{id}", ListUIUrl: "/mfg/stripfeedertemplatess", EditUIUrl: "/mfg/stripfeeder/template/{id}", CreateUIUrl: "/mfg/stripfeeder/template/add")]
    public class StripFeederTemplateSummary : SummaryData
    {
    }
}
