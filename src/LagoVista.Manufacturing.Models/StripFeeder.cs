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
    public class StripFeeder : MfgModelBase, IValidateable, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIDEntity, IFormConditionalFields, IFormAdditionalActions, IFormDescriptorBottom
    {
        public const string FeederOrientation_Horizontal = "horizontal";
        public const string FeederOrientation_Vertical = "vertical";

        public const string FeedDirection_Forwards = "forwards";
        public const string FeedDirection_Backwards = "backwards";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-left";

        private int _currentPartIndex;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_CurrentPartindex, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int CurrentPartIndex
        {
            get => _currentPartIndex;
            set => Set(ref _currentPartIndex, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Color, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string Color
        {
            get; set;
        } = "#000000";

        private bool _installed;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Installed, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool Installed
        {
            get => _installed;
            set => Set(ref _installed, value);
        }

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

        private double _pickHeight = 10;

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_PickHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }

        private decimal? _angleOffset;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_AngleOffset, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? AngleOffset
        {
            get { return _angleOffset; }
            set => Set(ref _angleOffset, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_StagingPlateColumn, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string StagingPlateColumn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_StagingPlate, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader StagingPlate { get; set; }

        private Point2D<double> _origin;
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

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Machine, WaterMark: ManufacturingResources.Names.Feeder_Machine_Select, EntityHeaderPickerUrl: "/api/mfg/machines", FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Machine { get; set; }

        private double _length;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Length, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        public double _rowWidth;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowWidth
        {
            get => _rowWidth;
            set => Set(ref _rowWidth, value);
        }

        public double _rowCount;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowCount, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowCount
        {
            get => _rowCount;
            set => Set(ref _rowCount, value);
        }

        public double _feederWidth;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_FeederWidth, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederWidth
        {
            get => _feederWidth;
            set => Set(ref _feederWidth, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Rows, FactoryUrl: "/api/mfg/stripfeeder/row/factory", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<StripFeederRow> Rows { get; set; } = new List<StripFeederRow>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(CurrentPartIndex),
                nameof(Description),
                nameof(Machine),
                nameof(Installed),
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
                nameof(PickHeight),
                nameof(StagingPlate),
                nameof(StagingPlateColumn),
                nameof(Origin),
                nameof(RowCount),
                nameof(Length),
                nameof(FeederWidth),
                nameof(RowWidth),
                nameof(AngleOffset),
                nameof(Color)
            };
        }

        [CustomValidator()]
        public void Validate(ValidationResult validationResult)
        {
            if (Installed)
            {
                if (Origin == null) validationResult.AddUserError("Origin is required for installed strip feeder.");
            }
            else
            {
                Origin = null;
            }
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(Origin) },
                Conditionals = new List<FormConditional>()
                {
                     new FormConditional()
                     {
                         RequiredFields = new List<string>() { nameof(Origin) },
                         VisibleFields =  new List<string>() { nameof(Origin) },
                         Field = nameof(Installed),
                         Value = "true",
                     }
                }
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

        private Point2D<double> _refHoleOffset;

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
