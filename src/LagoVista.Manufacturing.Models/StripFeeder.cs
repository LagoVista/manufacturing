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

    public interface IStagingPlateLocatedObject
    {
        public EntityHeader StagingPlate { get; set; }
        public EntityHeader ReferenceHoleRow{ get; set; }
        public EntityHeader ReferenceHoleColumn { get; set; }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeeder_Title, ManufacturingResources.Names.StripFeeder_Description,
               ManufacturingResources.Names.StripFeeder_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-left", Cloneable: true,
               SaveUrl: "/api/mfg/stripfeeder", GetUrl: "/api/mfg/stripfeeder/{id}", GetListUrl: "/api/mfg/stripfeeders", FactoryUrl: "/api/mfg/stripfeeder/factory",
               DeleteUrl: "/api/mfg/stripfeeder/{id}", ListUIUrl: "/mfg/stripfeeders", EditUIUrl: "/mfg/stripfeeder/{id}", CreateUIUrl: "/mfg/stripfeeder/add")]
    public class StripFeeder : MfgModelBase, IValidateable, IFormDescriptorAdvanced, IFormDescriptorAdvancedCol2, IFormDescriptor,
                             IFormConditionalFields, ISummaryFactory, IIDEntity, IFormAdditionalActions, IFormDescriptorBottom, IStagingPlateLocatedObject
    {
        public const string FeederOrientation_Horizontal = "horizontal";
        public const string FeederOrientation_Vertical = "vertical";

        public const string FeedDirection_Forwards = "forwards";
        public const string FeedDirection_Backwards = "backwards";


        public StripFeeder()
        {
            TapeSize = EntityHeader<TapeSizes>.Create(TapeSizes.EightMM);
            RowCount = 1;
            RowWidth = 12;
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
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Direction, FieldType: FieldTypes.Picker, 
            EnumType: typeof(FeedDirections), WaterMark: ManufacturingResources.Names.StripFeeder_Direction_Select, 
            IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeedDirections> FeedDirection
        {
            get => _feedDirection;
            set => Set(ref _feedDirection, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_OriginalTemplate, FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, IsUserEditable: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader OriginalTemplate { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Feeder_FeederId, HelpResource:ManufacturingResources.Names.Feeder_FeederId_Help, 
            FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string FeederId { get; set; }

        private Point2D<double> _rowOneRefHoleOffset = new Point2D<double>(3, 3);
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset, HelpResource:ManufacturingResources.Names.StripFeeder_RowOneRefHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> RowOneRefHoleOffset 
        {
            get => _rowOneRefHoleOffset;
            set => Set(ref _rowOneRefHoleOffset, value);
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

        private double _pickHeight = 10;

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_PickHeight, 
            ManufacturingResources.Names.StripFeeder_PickHeight_Help, FieldType: FieldTypes.Decimal, 
            IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight
        {
            get => _pickHeight;
            set => Set(ref _pickHeight, value);
        }

        private decimal _angleOffset = 0;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_AngleOffset, 
            HelpResource: ManufacturingResources.Names.StripFeeder_AngleOffset_Help, FieldType: FieldTypes.Decimal, 
            ResourceType: typeof(ManufacturingResources))]
        public decimal AngleOffset
        {
            get { return _angleOffset; }
            set => Set(ref _angleOffset, value);
        }

        EntityHeader _stagingPlate;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_StagingPlate, WaterMark: ManufacturingResources.Names.StripFeeder_StagingPlate_Select,
            IsUserEditable:false, EntityHeaderPickerUrl:"/api/mfg/machine/{machine.id}/stagingplates", FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader StagingPlate 
        {
            get => _stagingPlate;
            set => Set(ref _stagingPlate, value);
        }

        private Point2D<double> _origin = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.Common_Origin, 
            HelpResource: ManufacturingResources.Names.Common_Origin_Help, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin
        {
            get => _origin;
            set => Set(ref _origin, value);
        }

        private Point2D<double> _originOffset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_OriginOffset,
            HelpResource: ManufacturingResources.Names.Feeder_OriginOffset_Help, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> OriginOffset
        {
            get => _originOffset;
            set => Set(ref _originOffset, value);
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

        private double _rowCount;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_RowCount, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double RowCount
        {
            get => _rowCount;
            set => Set(ref _rowCount, value);
        }

        /// <summary>
        /// Width = Y 
        /// Length = X
        /// Height = Z
        /// 
        ///  W -------------------------------------------------------
        ///  I                   ROW 2
        ///  D -------------------------------------------------------
        ///  T                   ROW 1
        ///  H -------------------------------------------------------
        ///   ORIGIN             Length
        /// 
        /// </summary>
        private Point3D<double> _size = new Point3D<double>(120, 40, 12);
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Size, HelpResource: ManufacturingResources.Names.StripFeeder_FeederSize_Help, FieldType: FieldTypes.Point3DSize, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<double> Size
        {
            get => _size;
            set => Set(ref _size, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Rows, OpenByDefault:true, ChildListDisplayMembers:"rowIndex,component.text", FactoryUrl: "/api/mfg/stripfeeder/row/factory", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<StripFeederRow> Rows { get; set; } = new List<StripFeederRow>();

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Col, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Col_Help, IsUserEditable: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleColumn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Row, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Row_Help, IsUserEditable: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleRow { get; set; }

        Point2D<double> _referenceHoleOffset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ReferenceHoleOffset
        {
            get => _referenceHoleOffset;
            set => Set(ref _referenceHoleOffset, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),                
                nameof(TapeSize),
                nameof(FeederId),
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
                nameof(ReferenceHoleOffset),
                nameof(RowOneRefHoleOffset),
                nameof(Size),
                nameof(PickHeight),
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
                nameof(ReferenceHoleOffset),
                nameof(RowCount),
                nameof(Size),
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
                nameof(TapeSize),
                nameof(Orientation),
                nameof(FeedDirection),
                nameof(Color),
                nameof(Description),
                nameof(PickHeight),
                nameof(RowOneRefHoleOffset),
                nameof(Origin),
                nameof(OriginOffset),
            };
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>()
                 {
                     nameof(StagingPlate),
                     nameof(ReferenceHoleColumn),
                     nameof(ReferenceHoleRow),
                     nameof(Origin),
                     nameof(AngleOffset),
                     nameof(OriginOffset),
                     nameof(Origin),
                 },
                 Conditionals = new List<FormConditional>()
                 {
                     new FormConditional()
                     {
                          Field = nameof(Machine),
                          Value = "*",
                          VisibleFields = new List<string>()
                          {
                              nameof(StagingPlate),
                              nameof(ReferenceHoleColumn),
                              nameof(ReferenceHoleRow),
                              nameof(Origin),
                              nameof(Orientation),
                              nameof(AngleOffset),
                              nameof(OriginOffset),
                          }
                     }
                 }
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeeders_Title, ManufacturingResources.Names.StripFeeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/stripfeeder", GetUrl: "/api/mfg/stripfeeder/{id}", GetListUrl: "/api/mfg/stripfeeders", FactoryUrl: "/api/stripfeeder/factory",
            DeleteUrl: "/api/mfg/stripfeeder/{id}", ListUIUrl: "/mfg/stripfeeders", EditUIUrl: "/mfg/stripfeeder/{id}", CreateUIUrl: "/mfg/stripfeeder/add")]
    public class StripFeederSummary : SummaryData
    {
        public string TapeSize { get; set; }
        public string TapeSizeId { get; set; }
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

        private Point2D<double> _firstTapeHoleOffset = new Point2D<double>(5, 5);

        [FormField(LabelResource: ManufacturingResources.Names.StripFeederRow_FirstTapeHoleOffset, HelpResource:ManufacturingResources.Names.StripFeederRow_FirstTapeHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FirstTapeHoleOffset
        {
            get => _firstTapeHoleOffset;
            set => Set(ref _firstTapeHoleOffset, value);
        }

        private Point2D<double> _lastTapeHoleOffset = new Point2D<double>(0, 0);
        [FormField(LabelResource: ManufacturingResources.Names.StripFeederRow_LastTapeHoleOffset, HelpResource:ManufacturingResources.Names.StripFeederRow_LastTapeHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> LastTapeHoleOffset
        {
            get => _lastTapeHoleOffset;
            set => Set(ref _lastTapeHoleOffset, value);
        }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, $"Row {RowIndex}");
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
                nameof(FirstTapeHoleOffset),
                nameof(LastTapeHoleOffset),
                nameof(Notes),
            };
        }
    }
}
