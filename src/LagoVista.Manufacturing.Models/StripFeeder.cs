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
        }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-left";


        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string Color
        {
            get; set;
        } = "#444444";

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_BaseColor, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string BaseColor
        {
            get; set;
        } = "#222222";

        private bool _tapeHolesOnTop = true;

        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_TapeHolesOnTop, HelpResource: ManufacturingResources.Names.StripFeeder_TapeHolesOnTop_Help,
            FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool TapeHolesOnTop
        {
            get => _tapeHolesOnTop;
            set => Set(ref _tapeHolesOnTop, value);
        }


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


        private Point2D<double> _bottomLeftRow1Margin = new Point2D<double>(0,0);
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_BottomLeftRow1Margin, HelpResource: ManufacturingResources.Names.StripFeeder_BottomLeftRow1Margin_Help, FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> BottomLeftRow1Margin
        {
            get => _bottomLeftRow1Margin;
            set => Set(ref _bottomLeftRow1Margin, value);
        }

        Point2D<double> _tapeReferenceHoleOffset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_TapeReferenceHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_TapeReferenceHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> TapeReferenceHoleOffset
        {
            get => _tapeReferenceHoleOffset;
            set => Set(ref _tapeReferenceHoleOffset, value);
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

        private Point2D<double> _origin;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Origin, 
            HelpResource: ManufacturingResources.Names.StripFeeder_Origin_Help, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin
        {
            get => _origin;
            set => Set(ref _origin, value);
        }

        private Point2D<double> _originOffset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_OriginOffset,
            HelpResource: ManufacturingResources.Names.StripFeeder_OriginOffset_Help, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
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
        
        private double _height;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Height, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        private double _width;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        private double _length;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Length, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }
      
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_Rows, OpenByDefault:true, ChildListDisplayMembers:"rowIndex,component.text,status.text", CanAddRows: false, FactoryUrl: "/api/mfg/stripfeeder/row/factory", FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public List<StripFeederRow> Rows { get; set; } = new List<StripFeederRow>();

        public EntityHeader _referenceHoleColumn;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Col, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Col_Help, IsUserEditable: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleColumn 
        {
            get => _referenceHoleColumn;
            set => Set(ref _referenceHoleColumn, value);
        }

        public EntityHeader _referenceHoleRow;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Row, HelpResource: ManufacturingResources.Names.StripFeeder_ReferenceHole_Row_Help, IsUserEditable: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleRow
        {
            get => _referenceHoleRow;
            set => Set(ref _referenceHoleRow, value);
        }

        Point2D<double> _mountingHoleOffset = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_MountingHoleOffset, HelpResource: ManufacturingResources.Names.StripFeeder_MountingHoleOffset_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> MountingHoleOffset
        {
            get => _mountingHoleOffset;
            set => Set(ref _mountingHoleOffset, value);
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
                Machine = Machine?.Text,
                MachineId = Machine?.Id,
                TapeSizeId = TapeSize.Id,
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
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


        public List<string> GetAdvancedFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Machine),
                nameof(TapeSize),
                nameof(TapeHolesOnTop),
                nameof(Orientation),
                nameof(FeedDirection),
                nameof(Color),
                nameof(BaseColor),
                nameof(Description),
                nameof(PickHeight),
                nameof(Origin),
                nameof(OriginOffset),
                nameof(BottomLeftRow1Margin),
                nameof(TapeReferenceHoleOffset),
            };
        }

        public List<string> GetAdvancedFieldsCol2()
        {
            return new List<string>()
            {
                nameof(ReferenceHoleColumn),
                nameof(ReferenceHoleRow),
                nameof(MountingHoleOffset),
                nameof(RowCount),
                nameof(Width),
                nameof(Length),
                nameof(Height),
                nameof(RowWidth),                
                nameof(AngleOffset),
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
            ManufacturingResources.Names.StripFeeder_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/stripfeeder", GetUrl: "/api/mfg/stripfeeder/{id}", GetListUrl: "/api/mfg/stripfeeders", FactoryUrl: "/api/stripfeeder/factory",
            DeleteUrl: "/api/mfg/stripfeeder/{id}", ListUIUrl: "/mfg/stripfeeders", EditUIUrl: "/mfg/stripfeeder/{id}", CreateUIUrl: "/mfg/stripfeeder/add")]
    public class StripFeederSummary : SummaryData
    {
        public string TapeSize { get; set; }
        public string TapeSizeId { get; set; }
    
        public string MachineId { get; set; }
        public string Machine { get; set; }
    }

    public enum StripFeederRowStatuses
    {
        [EnumLabel(StripFeederRow.StripFeederRowStatus_None, ManufacturingResources.Names.StripFeederRowStatus_None, typeof(ManufacturingResources))]
        None,
        [EnumLabel(StripFeederRow.StripFeederRowStatus_Planned, ManufacturingResources.Names.StripFeederRowStatus_Planned, typeof(ManufacturingResources))]
        Planned,
        [EnumLabel(StripFeederRow.StripFeederRowStatus_Ready, ManufacturingResources.Names.StripFeederRowStatus_Ready, typeof(ManufacturingResources))]
        Ready,
        [EnumLabel(StripFeederRow.StripFeederRowStatus_Empty, ManufacturingResources.Names.StripFeederRowStatus_Empty, typeof(ManufacturingResources))]
        Empty
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.StripFeederRow_Title, ManufacturingResources.Names.StripFeederRow_Description,
               ManufacturingResources.Names.StripFeederRow_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), 
                Icon: "icon-fo-left", Cloneable: true, FactoryUrl: "/api/mfg/stripfeeder/row/factory")]
    public class StripFeederRow : ModelBase, IFormDescriptor, IFormAdditionalActions
    {
        public const string StripFeederRowStatus_None = "none";
        public const string StripFeederRowStatus_Planned = "planned";
        public const string StripFeederRowStatus_Ready = "ready";
        public const string StripFeederRowStatus_Empty = "empty";

        public StripFeederRow()
        {
            Id = Guid.NewGuid().ToId();
            Status = EntityHeader<StripFeederRowStatuses>.Create(StripFeederRowStatuses.None);
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

        private int _currentPartIndex;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeederRow_CurrentPartIndex, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]        
        public int CurrentPartIndex
        {
            get => _currentPartIndex;
            set => Set(ref _currentPartIndex, value);
        }

        private EntityHeader<StripFeederRowStatuses> _status;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Status, WaterMark:ManufacturingResources.Names.Common_Status_Select, EnumType: typeof(StripFeederRowStatuses), FieldType: FieldTypes.Picker, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<StripFeederRowStatuses> Status 
        {
            get => _status;
            set => Set(ref _status, value);
        }

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

        private int _partCapacity;
        [FormField(LabelResource: ManufacturingResources.Names.StripFeeder_PartCapacity, HelpResource: ManufacturingResources.Names.StripFeeder_PartCapacity_Help, FieldType: FieldTypes.Integer, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int PartCapacity
        {
            get => _partCapacity;
            set => Set(ref _partCapacity, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Component),
                nameof(Status),
                nameof(RowIndex),
                nameof(CurrentPartIndex),
                nameof(PartCapacity),
                nameof(FirstTapeHoleOffset),
                nameof(LastTapeHoleOffset),
                nameof(Notes),
            };
        }
    }
}
