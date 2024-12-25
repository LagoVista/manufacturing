using LagoVista.Core;c
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.NozzleTip_Title, ManufacturingResources.Names.NozzleTip_Description,
        ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), 
        Icon: "icon-pz-searching-2", Cloneable: true,
        FactoryUrl: "/api/mfg/feeder/factory" )]
    public class ToolNozzleTip : ModelBase, IFormDescriptor
    {
        public ToolNozzleTip()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(ManufacturingResources))]

        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_SafeMoveHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double SafeMoveHeight { get; set; }
        
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_PickHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PickHeight { get; set; }
   
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_BoardHeight, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double BoardHeight { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_InnerDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int InnnerDiameter { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_OuterDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public int OuterDiameter { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_ToolRackLocation, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public Point3D<double> ToolRackLocation { get; set; }

        private decimal _idleVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_IdleVacuum, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal IdleVacuum 
        { 
            get => _idleVacuum;
            set => Set(ref _idleVacuum, value); 
        }
        
        private decimal _partPickedVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_PartPickedVacuum, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public decimal PartPickedVacuum 
        {
            get => _partPickedVacuum;
            set => Set(ref _partPickedVacuum, value);
        }

        public List<string> GetFormFields()
        {
            throw new NotImplementedException();
        }
    }
}
