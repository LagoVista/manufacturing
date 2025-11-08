// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 738bc694e86ffd3a1ea692d6c8b3cd6cd2e2dfdb003438bf104e66974629605c
// IndexVersion: 2
// --- END CODE INDEX META ---
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


        private Point2D<double> _bottomLeftRow1Margin = new Point2D<double>(0, 0);
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_BottomLeftRow1Margin, HelpResource: ManufacturingResources.Names.StripFeeder_BottomLeftRow1Margin_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> BottomLeftRow1Margin
        {
            get => _bottomLeftRow1Margin;
            set => Set(ref _bottomLeftRow1Margin, value);
        }

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

        private decimal _rowSpacing = 0;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowSpacing, FieldType: FieldTypes.Decimal, IsRequired:true,
            ResourceType: typeof(ManufacturingResources))]
        public decimal RowSpacing
        {
            get { return _rowSpacing; }
            set => Set(ref _rowSpacing, value);
        }


        private double _rowCount;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowCount, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowCount
        {
            get => _rowCount;
            set => Set(ref _rowCount, value);
        }

        private double _height = 12;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Height, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        private double _width = 40;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        private double _length = 120;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Length, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        Point2D<double> _tapeReferenceHoleOffset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_TapeReferenceHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_TapeReferenceHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> TapeReferenceHoleOffset
        {
            get => _tapeReferenceHoleOffset;
            set => Set(ref _tapeReferenceHoleOffset, value);
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
                nameof(Length),
                nameof(Width),
                nameof(Height),
                nameof(RowWidth),
                nameof(RowSpacing),
                nameof(RowCount),                
                nameof(TapeReferenceHoleOffset),
                nameof(BottomLeftRow1Margin),
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
