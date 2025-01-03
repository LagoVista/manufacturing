using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models;
using System;
using LagoVista.Core;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartLayers;
using LagoVista.Core.Attributes;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.Core.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.Manufacturing.Models
{
    public enum CameraTypes
    {
        [EnumLabel(MachineCamera.MachineCamera_Type_Position, ManufacturingResources.Names.MachineCamera_Type_Position, typeof(ManufacturingResources))]
        Position,
        [EnumLabel(MachineCamera.MachineCamera_Type_PartInspection, ManufacturingResources.Names.MachineCamera_Type_PartInspection, typeof(ManufacturingResources))]
        PartInspection,
        [EnumLabel(MachineCamera.MachineCamera_Type_Observation, ManufacturingResources.Names.MachineCamera_Type_Observation, typeof(ManufacturingResources))]
        Observation
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.MachineCamera_Title, ManufacturingResources.Names.MachineCamera_Description,
        ManufacturingResources.Names.MachineCamera_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-camera", Cloneable: true,
        FactoryUrl: "/api/mfg/machine/camera/factory")]
    public class MachineCamera : ModelBase, IIconEntity, IIDEntity, INamedEntity, IFormDescriptor
    {
        public const string MachineCamera_Type_Position = "position";
        public const string MachineCamera_Type_PartInspection = "partinspection";
        public const string MachineCamera_Type_Observation = "observation";

        public MachineCamera()
        {
            Id = Guid.NewGuid().ToId();
        }

        public String Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]

        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Key, FieldType: FieldTypes.Key, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Key { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-camera";

        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_DeviceId, FieldType: FieldTypes.Text, IsUserEditable: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader CameraDevice { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_Type, WaterMark:ManufacturingResources.Names.MachineCamera_Type_Select, FieldType: FieldTypes.Picker, EnumType:typeof(CameraTypes), IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<CameraTypes> CameraType { get; set; }

        Point2D<double> _absolutePosition;
        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_AbsolutePosition, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> AbsolutePosition
        {
            get => _absolutePosition;
            set => Set(ref _absolutePosition, value);
        }

        double _focusHeight;
        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_FocusHeight, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public double FocusHeight
        {
            get => _focusHeight;
            set => Set(ref _focusHeight, value);    
        }

        private bool _mirrorXAxis;
        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_MirrorX, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool MirrorXAxis
        {
            get => _mirrorXAxis;
            set { Set(ref _mirrorXAxis, value); }
        }

        private bool _mirrorYAxis;
        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_MirrorY, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool MirrorYAxis 
        { 
            get { return _mirrorYAxis; }
            set { Set(ref _mirrorYAxis, value); }
        }

        private Point2D<double> _tool1Offset;
        public Point2D<double> Tool1Offset
        {
            get { return _tool1Offset; }
            set { Set(ref _tool1Offset, value); }
        }

        private Point2D<double> _tool2Offset;
        public Point2D<double> Tool2Offset
        {
            get { return _tool2Offset; }
            set { Set(ref _tool2Offset, value); }
        }

        double _pixelsPerMM = 50;
        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_PixelsPerMM, HelpResource: ManufacturingResources.Names.MachineCamera_PixelsPerMM_Help, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double PixelsPerMM
        {
            get { return _pixelsPerMM; }
            set { Set(ref _pixelsPerMM, Math.Round(value,1)); }
        }

        [FormField(LabelResource: ManufacturingResources.Names.MachineCamera_ImageSize, FieldType: FieldTypes.Point2DSize, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ImageSize { get; set; } = new Point2D<double>(480, 480);

        public ObservableCollection<VisionProfile> VisionProfiles { get; set; } = new ObservableCollection<VisionProfile>();

        public VisionProfile CurrentVisionProfile { get; set; }

        public Point2D<double> Tool3Offset { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(CameraType),
                nameof(Icon),
                nameof(CameraDevice),
                nameof(MirrorXAxis),
                nameof(MirrorYAxis),
                nameof(AbsolutePosition),
                nameof(FocusHeight),
            };
        }
    }
}
