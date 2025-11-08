// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e5c5e32b9ee362268f35d136b8958c00bd7cd89685188af2ccd1e08e60d7ed67
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Pcb_Revision_Title, ManufacturingResources.Names.Pcb_Revision_Description,
       ManufacturingResources.Names.Pcb_Revision_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
       FactoryUrl: "/api/mfg/pcb/revision/factory")]
    public class CircuitBoardRevision : IIDEntity, IValidateable, IFormDescriptor
    {
        public CircuitBoardRevision()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revision_RevisionTimeStamp, IsRequired:true, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string RevisionTimeStamp { get; set; }
        
        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revision, FieldType: FieldTypes.Text, IsRequired:true, ResourceType: typeof(ManufacturingResources))]
        public string Revision { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revision_BoardFile, ImageUpload: false, IsFileUploadImage: false, IsRequired:true, FieldType: FieldTypes.FileUpload, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader BoardFile { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revision_SchematicFile, ImageUpload: false, IsFileUploadImage:false, IsRequired:true, FieldType: FieldTypes.FileUpload, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader SchematicFile { get; set; }
        
        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revision_SchematicPdFile, ImageUpload: false, FieldType: FieldTypes.FileUpload, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader SchematicPDFFile { get; set; }
        
        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Revision_BomFile, ImageUpload:false, IsFileUploadImage: false, FieldType: FieldTypes.FileUpload, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader BomFile { get; set; }
        
        [FormField(LabelResource: ManufacturingResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(ManufacturingResources))]
        public string Notes { get; set; }

        public List<PcbComponent> PcbComponents { get; set; } = new List<PcbComponent>();

        public List<PcbPackage> PhysicalPackages { get; set; } = new List<PcbPackage>();

        public List<Via> Vias { get; set; } = new List<Via>();

        public List<Hole> Holes { get; set; } = new List<Hole>();

        public List<PcbLine> TopWires { get; set; }
        public List<PcbLine> BottomWires { get; set; }

        public List<PcbLayer> Layers { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Width, FieldType: FieldTypes.Decimal, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public double? Width { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.Common_Height, FieldType: FieldTypes.Decimal, IsUserEditable:false, ResourceType: typeof(ManufacturingResources))]
        public double? Height { get; set; }

        public List<PcbLine> Outline { get; set; } = new List<PcbLine>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(RevisionTimeStamp),
                nameof(Revision),
                nameof(BoardFile),
                nameof(SchematicFile),
                nameof(SchematicPDFFile),
                nameof(BomFile),
                nameof(Width),
                nameof(Height),
                nameof(Notes),

            };
        }

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader() { Id = Id, Text = Revision };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Pcb_Variant, ManufacturingResources.Names.Pcb_Variant_Description,
       ManufacturingResources.Names.Pcb_Variant_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-core-1", Cloneable: true,
       FactoryUrl: "/api/mfg/pcb/variant/factory")]
    public class CircuitBoardVariant
    {
        public string PartName { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Variant_Sku, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Sku { get; set; }

        public List<PcbComponent> PcbComponents { get; set; } = new List<PcbComponent>();
    }
}
