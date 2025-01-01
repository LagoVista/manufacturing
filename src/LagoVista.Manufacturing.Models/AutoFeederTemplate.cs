using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.AutoFeederTemplate_Title, ManufacturingResources.Names.AutoFeederTemplate_Description,
              ManufacturingResources.Names.AutoFeederTemplate_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
              Icon: "icon-pz-searching-2", Cloneable: true,
              SaveUrl: "/api/mfg/autofeeder/template", GetUrl: "/api/mfg/autoFeeder/template/{id}", GetListUrl: "/api/mfg/autofeeder/templates", FactoryUrl: "/api/mfg/autofeeder/template/factory",
              DeleteUrl: "/api/mfg/autofeeder/{id}", ListUIUrl: "/mfg/autoFeedertemplatess", EditUIUrl: "/mfg/autofeeder/template/{id}", CreateUIUrl: "/mfg/autofeeder/template/add")]
    public class AutoFeederTemplate : MfgModelBase, ISummaryFactory, IFormDescriptor
    {

        public AutoFeederTemplateSummary CreateSummary()
        {
            return new AutoFeederTemplateSummary()
            {
                Id = Id,
                Key = Key,
                Name = Name,
                Description = Description,
                IsPublic = IsPublic,
            };
        }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-searching-2";


        private Point2D<double> _pickLocation;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickLocation, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> PickLocation
        {
            get => _pickLocation;
            set => Set(ref _pickLocation, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, ResourceType: typeof(ManufacturingResources))]
        public string Color
        {
            get; set;
        } = "#000000";

        private double _pickHeight;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickHeight, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
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

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_AdvanceGCode, HelpResource: ManufacturingResources.Names.Feeder_AdvanceGCode_Help, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string AdvanceGCode { get; set; }

        private double _feederWidth = 16;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Width, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederWidth
        {
            get => _feederWidth;
            set => Set(ref _feederWidth, value);
        }

        private double _feederLength = 120;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Length, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederLength
        {
            get => _feederLength;
            set => Set(ref _feederLength, value);
        }

        private double _feederHeight = 80;
        [FormField(LabelResource: ManufacturingResources.Names.Common_Height, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeederHeight
        {
            get => _feederHeight;
            set => Set(ref _feederHeight, value);
        }

        Point2D<double> _pickOffset;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickOffsetFromSlotOriign, FieldType: FieldTypes.Point2D,
           HelpResource: ManufacturingResources.Names.Feeder_PickOffsetFromSlotOriign_Help, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> PickOffset
        {
            get => _pickOffset;
            set => Set(ref _pickOffset, value);
        }

        Point2D<double> _fiducitalOffset;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_FiducialOffsetFromSlotOriign, FieldType: FieldTypes.Point2D,
           HelpResource: ManufacturingResources.Names.Feeder_FiducialOffsetFromSlotOriign_Help, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FiducialOffset
        {
            get => _fiducitalOffset;
            set => Set(ref _fiducitalOffset, value);
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
                nameof(Color),
                nameof(FeederWidth),
                nameof(FeederLength),
                nameof(FeederHeight),
                nameof(FiducialOffset),
                nameof(PickOffset)
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.AutoFeederTemplates_Title, ManufacturingResources.Names.AutoFeederTemplate_Description,
          ManufacturingResources.Names.AutoFeederTemplate_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
          Icon: "icon-pz-searching-2", Cloneable: true,
          SaveUrl: "/api/mfg/autofeeder/template", GetUrl: "/api/mfg/autoFeeder/template/{id}", GetListUrl: "/api/mfg/autofeeder/templates", FactoryUrl: "/api/mfg/autofeeder/template/factory",
          DeleteUrl: "/api/mfg/autofeeder/{id}", ListUIUrl: "/mfg/autoFeedertemplatess", EditUIUrl: "/mfg/autofeeder/template/{id}", CreateUIUrl: "/mfg/autofeeder/template/add")]
    public class AutoFeederTemplateSummary : SummaryData
    {
    }
}
