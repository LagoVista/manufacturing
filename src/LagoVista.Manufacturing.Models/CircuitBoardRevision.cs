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
    public class CircuitBoardRevision : IIDEntity, IValidateable, IFormDescriptor, IFormDescriptorCol2
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

        [FormField(LabelResource: ManufacturingResources.Names.Pcb_Variants, FieldType: FieldTypes.ChildListInline, OpenByDefault:true, ChildListDisplayMembers:"partName,sku", FactoryUrl: "/api/mfg/pcb/variant/factory", ResourceType: typeof(ManufacturingResources))]
        public List<CircuitBoardVariant> Variants { get; set; } = new List<CircuitBoardVariant>();

        public List<PcbComponent> PcbComponents { get; set; } = new List<PcbComponent>();

        public List<PcbPackage> PhysicalPackages { get; set; } = new List<PcbPackage>();


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

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Variants),
            };
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
