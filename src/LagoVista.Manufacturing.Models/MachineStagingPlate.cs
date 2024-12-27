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

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.MachineStagingPlate_Title, ManufacturingResources.Names.MachineStagingPlate_Description,
        ManufacturingResources.Names.MachineStagingPlate_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: true,
        FactoryUrl: "/api/mfg/machine/stagingplate/factory")]
    public class MachineStagingPlate : ModelBase, IFormDescriptor, IFormDescriptorCol2
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

        [FormField(LabelResource: ManufacturingResources.Names.Common_Color, FieldType: FieldTypes.Color, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Color { get; set; } = "#000000";

        [FormField(LabelResource: ManufacturingResources.Names.Common_Size, FieldType: FieldTypes.Point2DSize, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Size { get; set; } = new Point2D<double>(600, 120);

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_HolesStaggered, HelpResource: ManufacturingResources.Names.MachineStagingPlate_HolesStaggered_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool HolesStaggered { get; set; } = true;

        private Point2D<double> _firstHole = new Point2D<double>(15,30);
        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_FirstHole, HelpResource:ManufacturingResources.Names.MachineStagingPlate_FirstHole_Help, 
            FieldType: FieldTypes.Point2D, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> FirstHole
        {
            get => _firstHole;
            set => Set(ref _firstHole, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_Origin_Origin_Approx, HelpResource: ManufacturingResources.Names.MachineStagingPlate_Origin_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin { get; set; }


        Point2D<double> _referenceHoleLocation1 = new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleLocation1, HelpResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleLocation_Help, FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ReferenceHoleLocation1
        {
            get => _referenceHoleLocation1;
            set => Set(ref _referenceHoleLocation1, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleCol1, HelpResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleCol_Help, AddEnumSelect: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleColumn1 { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleRow1, HelpResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleRow_Help, AddEnumSelect: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleRow1{ get; set; }


        Point2D<double> _referenceHoleLocation2 =new Point2D<double>();
        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleLocation2, HelpResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleLocation_Help,  FieldType: FieldTypes.Point2D, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> ReferenceHoleLocation2
        {
            get => _referenceHoleLocation2;
            set => Set(ref _referenceHoleLocation2, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleCol2, HelpResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleCol_Help, AddEnumSelect: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleColumn2 { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleRow2, HelpResource: ManufacturingResources.Names.MachineStagingPlate_ReferenceHoleRow_Help, AddEnumSelect: true, FieldType: FieldTypes.Picker, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader ReferenceHoleRow2 { get; set; }




        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Key, Name);
        }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Size),
                nameof(FirstHole),
                nameof(HolesStaggered),
                nameof(HoleSpacing),
                nameof(Color),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Origin),
                nameof(ReferenceHoleColumn1),
                nameof(ReferenceHoleRow1),
                nameof(ReferenceHoleLocation1),
                nameof(ReferenceHoleColumn2),
                nameof(ReferenceHoleRow2),
                nameof(ReferenceHoleLocation2),
            };
        }
    }
}
