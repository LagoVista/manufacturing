using LagoVista.Core;
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

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.NozzleTip_Title, ManufacturingResources.Names.NozzleTip_Description,
        ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: true,
        FactoryUrl: "/api/mfg/machine/stagingplate/factory")]
    public class MachineStagingPlate : ModelBase, IFormDescriptor
    {

        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Key, FieldType: FieldTypes.Key, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Key { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-control-panel";

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_HoleSpacing, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double HoleSpacing { get; set; } = 15;

        [FormField(LabelResource: ManufacturingResources.Names.Common_Size, FieldType: FieldTypes.Point2DSize, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Size { get; set; }


        private Point2D<double> _firstHole = new Point2D<double>(15,15);
        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_FirstHole, HelpResource:ManufacturingResources.Names.MachineStagingPlate_FirstHole_Help, 
            FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FirstHole
        {
            get => _firstHole;
            set => Set(ref _firstHole, value);
        }

        private Point2D<double> _origin = new Point2D<double>(0,0);
        [FormField(LabelResource: ManufacturingResources.Names.Common_Origin, HelpResource: ManufacturingResources.Names.MachineStagingPlate_FirstHole_Help,
            FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin
        {
            get => _origin;
            set => Set(ref _origin, value);
        }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Size),
                nameof(HoleSpacing),
                nameof(Origin),
                nameof(FirstHole),
            };
        }
    }
}
