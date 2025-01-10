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


        Point2D<double> _rowOneRefHoleOffset = new Point2D<double>(3,3);
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> RowOneRefHoleOffset 
        {
            get => _rowOneRefHoleOffset;
            set => Set(ref _rowOneRefHoleOffset, value);
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

        private double _rowCount;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowCount, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowCount
        {
            get => _rowCount;
            set => Set(ref _rowCount, value);
        }

        private Point3D<double> _size = new Point3D<double>(40, 120, 12);
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Size, HelpResource: ManufacturingResources.Names.StripFeeder_FeederSize_Help, FieldType: FieldTypes.Point3DSize, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<double> Size 
        {
            get => _size;
            set => Set(ref _size, value);
        }

        Point2D<double> _referenceHoleLocation = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset, 
            HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ReferenceHoleOffset
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
                nameof(Size),
                nameof(RowWidth),
                nameof(RowCount),                
                nameof(ReferenceHoleOffset),
                nameof(RowOneRefHoleOffset),
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
