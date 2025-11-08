// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ffef90903b1d4aec49f674b1bc08597dd85dc025e7c5587ac586bf4bd2e2fcc2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.AssemblyInstructionStep_Title, ManufacturingResources.Names.AssemblyInstruction_Description,
               ManufacturingResources.Names.AssemblyInstruction_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
               Icon: "icon-fo-instructions", Cloneable: true,
               FactoryUrl: "/api/mfg/assembly/factory")]
    public class AssemblyInstructionStep : IFormDescriptor
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: ManufacturingResources.Names.AssemblyInstructionStep_Title, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Title { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.AssembyInstructionsStep_Instructions, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(ManufacturingResources))]
        public string Instructions { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.AssemblyInstructionStep_Images, FieldType: FieldTypes.MediaResources, ResourceType: typeof(ManufacturingResources))]
        public List<EntityHeader> Images { get; set; } = new List<EntityHeader>();

        [FormField(LabelResource: ManufacturingResources.Names.AssemblyInstructionStep_Parts, FieldType: FieldTypes.ChildListInlinePicker, ResourceType: typeof(ManufacturingResources))]
        public List<EntityHeader> Parts { get; set; } = new List<EntityHeader>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Title),
                nameof(Instructions),
                nameof(Images),
                nameof(Parts),
            };
        }
    }
}
