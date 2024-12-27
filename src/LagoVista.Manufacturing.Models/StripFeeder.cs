using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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
               DeleteUrl: "/api/mfg/stripfeeder/{id}", ListUIUrl: "/mfg/stripfeeders", EditUIUrl: "/mfg/stripfeeder/{id}", CreateUIUrl: "/mfg/stripfeeder/add")]
    public class StripFeeder : MfgModelBase, IValidateable, IFormDescriptorAdvanced, IFormDescriptorAdvancedCol2, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity, IFormAdditionalActions, IFormDescriptorBottom
    {
        public const string FeederOrientation_Horizontal = "horizontal";
        public const string FeederOrientation_Vertical = "vertical";

        public const string FeedDirection_Forwards = "forwards";
        public const string FeedDirection_Backwards = "backwards";


        public StripFeeder()
        {
            TapeSize = EntityHeader<TapeSizes>.Create(TapeSizes.EightMM);
            RowCount = 1;
            RowWidth = 10;
            FeederWidth = RowWidth * RowCount;
            Orientation = EntityHeader<FeederOrientations>.Create(FeederOrientations.Horizontal);
            FeedDirection = EntityHeader<FeedDirections>.Create(FeedDirections.Forward);
            RowOneRefHoleOffset = new Point2D<double>() { X = 3, Y = 3 };
        }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-left";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string Color
        {
            get; set;
        } = "#000000";


        private EntityHeader<FeederOrientations> _orientation;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Orientation, FieldType: FieldTypes.Picker, EnumType: typeof(FeederOrientations), WaterMark: ManufacturingResources.Names.StripFeeder_Orientation_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederOrientations> Orientation
        {
            get => _orientation;
            set => Set(ref _orientation, value);
        }

        private EntityHeader<FeedDirections> _feedDirection;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Direction, FieldType: FieldTypes.Picker, EnumType: typeof(FeedDirections), WaterMark: ManufacturingResources.Names.StripFeeder_Direction_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeedDirections> FeedDirection
        {
            get => _feedDirection;
            set => Set(ref _feedDirection, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset, HelpResource:ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> RowOneRefHoleOffset { get; set; }

        private double _pickHeight = 10;

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_PickHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }

        private decimal _angleOffset = 0;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_AngleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_AngleOffset_Help, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal AngleOffset
        {
            get { return _angleOffset; }
            set => Set(ref _angleOffset, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_StagingPlate, IsUserEditable:false, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader StagingPlate { get; set; }

        private Point2D<double> _origin = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.Common_Origin, HelpResource: ManufacturingResources.Names.Common_Origin_Help, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin
        {
            get => _origin;
            set => Set(ref _origin, value);
        }

        private EntityHeader<TapeSizes> _tapeSize;
        [FormField(LabelResource: ManufacturingResources.Names.ComponentPackage_TapeSize, FieldType: FieldTypes.Picker, EnumType: typeof(TapeSizes),
            WaterMark: ManufacturingResources.Names.ComponentPackage_TapeSize_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<TapeSizes> TapeSize
        {
            get => _tapeSize;
            set => Set(ref _tapeSize, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Machine, WaterMark: ManufacturingResources.Names.Feeder_Machine_Select, EntityHeaderPickerUrl: "/api/mfg/machines", IsUserEditable:false, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Machine { get; set; }

        private double _feederLength = 120;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_FeederLength, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederLength
        {
            get => _feederLength;
            set => Set(ref _feederLength, value);
        }

        public double _rowWidth;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
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
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_FeederWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederWidth
        {
            get => _feederWidth;
            set => Set(ref _feederWidth, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Rows, ChildListDisplayMembers:"rowIndex,component.text", FactoryUrl: "/api/mfg/stripfeeder/row/factory", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<StripFeederRow> Rows { get; set; } = new List<StripFeederRow>();


        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Col, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Col_Help, IsUserEditable: false, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleColumn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Row, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Row_Help, IsUserEditable: false, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleRow { get; set; }

        Point2D<double> _referenceHoleLocation = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ReferenceHoleLocation
        {
            get => _referenceHoleLocation;
            set => Set(ref _referenceHoleLocation, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),                
                nameof(TapeSize),
                nameof(Orientation),
                nameof(FeedDirection),
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
                TapeSizeId = TapeSize.Id,
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
                nameof(RowCount),
                nameof(FeederLength),
                nameof(FeederWidth),
                nameof(RowWidth),
            };
        }

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()             {
                new FormAdditionalAction()
                {
                    Icon = "fa fa-centercode",
                    Key="resolvelocation",
                    Title = "Resolve Location",
                    ForCreate = true,
                    ForEdit = true,
                }
            };
        }

        public List<string> GetFormFieldsBottom()
        {
            return new List<string>()
            {
                nameof(Rows)
            };
        }

        public List<string> GetAdvancedFieldsCol2()
        {
            return new List<string>()
            {
                nameof(ReferenceHoleColumn),
                nameof(ReferenceHoleRow),
                nameof(ReferenceHoleLocation),
                nameof(RowCount),
                nameof(FeederLength),
                nameof(FeederWidth),
                nameof(RowWidth),
                nameof(AngleOffset),

            };
        }

        public List<string> GetAdvancedFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Machine),
                nameof(StagingPlate),
                nameof(TapeSize),
                nameof(Orientation),
                nameof(FeedDirection),
                nameof(Color),
                nameof(Description),
                nameof(PickHeight),
                nameof(RowOneRefHoleOffset),
                nameof(Origin),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeeders_Title, ManufacturingResources.Names.StripFeeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/stripfeeder", GetUrl: "/api/mfg/stripfeeder/{id}", GetListUrl: "/api/mfg/stripfeeders", FactoryUrl: "/api/stripfeeder/factory",
            DeleteUrl: "/api/mfg/stripfeeder/{id}", ListUIUrl: "/mfg/stripfeeders", EditUIUrl: "/mfg/stripfeeder/{id}", CreateUIUrl: "/mfg/stripfeeder/add")]
    public class StripFeederSummary : SummaryData
    {
        public string TapeSize { get; set; }
        public string TapeSizeId { get; set; }

        public string ComponentId { get; set; }
        public string ComponentKey { get; set; }
        public string Component { get; set; }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeederRow_Title, ManufacturingResources.Names.StripFeederRow_Description,
               ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), 
                Icon: "icon-fo-left", Cloneable: true, FactoryUrl: "/api/mfg/stripfeeder/row/factory")]
    public class StripFeederRow : ModelBase, IFormDescriptor, IFormAdditionalActions
    {
        public StripFeederRow()
        {
            Id = Guid.NewGuid().ToId();            
        }

        public string Id { get; set; }

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()             {
                new FormAdditionalAction()
                {
                    Icon = "fa fa-tally",
                    Key = "resetindex",
                    Title = "Reset Part Index",
                    ForCreate = true,
                    ForEdit = true,
                }
            };
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeederRow_CurrentPartIndex, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int CurrentPartIndex { get; set; }

        private int _rowIndex = 1;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeederRow_RowIndex, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int RowIndex
        {
            get => _rowIndex;
            set => Set(ref _rowIndex, value);
        }

        private Point2D<double> _refHoleOffset = new Point2D<double>(5, 5);

        [FormField(LabelResource: ManufacturingResources.Names.StripFeederRow_Offset, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> RefHoleOffset
        {
            get => _refHoleOffset;
            set => Set(ref _refHoleOffset, value);
        }

        private EntityHeader<Component> _component;
        [FormField(LabelResource: ManufacturingResources.Names.Component_Title, FieldType: FieldTypes.Custom, WaterMark: ManufacturingResources.Names.Feeder_Component_Select,
            CustomFieldType: "componentpicker", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<Component> Component
        {
            get => _component;
            set => Set(ref _component, value);
        }

        private string _notes;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Notes, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Notes
        {
            get => _notes;
            set => Set(ref _notes, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Component),
                nameof(RowIndex),
                nameof(CurrentPartIndex),
                nameof(RefHoleOffset),
                nameof(Notes),
            };
        }
    }
}
