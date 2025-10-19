// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e2eec81633a1471aa13b10d1c0473de8e29bf0fbda1bd84d7310172566629608
// IndexVersion: 0
// --- END CODE INDEX META ---
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

        Point3D<double> _originOffset = new Point3D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_OriginOffset, HelpResource: ManufacturingResources.Names.Feeder_OriginOffset_Help,
            FieldType: FieldTypes.Point3D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<double> OriginOffset
        {
            get => _originOffset;
            set => Set(ref _originOffset, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Protocol, FieldType: FieldTypes.Picker, EnumType: typeof(FeederProtocols),
            WaterMark: ManufacturingResources.Names.Feeder_Protocol_Select, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<FeederProtocols> Protocol { get; set; }


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

        Size3D _size = new Size3D(135, 76, 15);
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_Size, HelpResource: ManufacturingResources.Names.AutoFeeder_Size_Help, FieldType: FieldTypes.Point3DSize, ResourceType: typeof(ManufacturingResources))]
        public Size3D Size
        {
            get => _size;
            set => Set(ref _size, value);
        }


        Point2D<double> _pickOffset;
        [FormField(LabelResource: ManufacturingResources.Names.AutoFeeder_PickOffsetFromFiducial, FieldType: FieldTypes.Point2D,
           HelpResource: ManufacturingResources.Names.AutoFeeder_PickOffsetFromFiducial_Help, ResourceType: typeof(ManufacturingResources))]
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
                nameof(PickHeight),
                nameof(TapeSize),
                nameof(Protocol),
                nameof(Size),
                nameof(FiducialOffset),
                nameof(PickOffset),
                nameof(OriginOffset),
                nameof(AdvanceGCode)
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
