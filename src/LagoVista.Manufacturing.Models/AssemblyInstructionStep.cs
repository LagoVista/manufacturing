using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.AssemblyInstruction_Title, ManufacturingResources.Names.AssemblyInstruction_Description,
               ManufacturingResources.Names.AssemblyInstruction_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
               Icon: "icon-pz-searching-2", Cloneable: true,
               SaveUrl: "/api/mfg/assembly", GetUrl: "/api/mfg/assembly/{id}", GetListUrl: "/api/mfg/assemblies", FactoryUrl: "/api/mfg/assembly/factory",
               DeleteUrl: "/api/mfg/assembly/{id}", ListUIUrl: "/mfg/assemblyinstructions", EditUIUrl: "/mfg/assemblyinstruction/{id}", CreateUIUrl: "/mfg/assemblyinstruction/add")]
    public class AssemblyInstructionStep
    {
        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.AssemblyInstructionStep_Title, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Title { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.AssembyInstructionsStep_Instructions, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(ManufacturingResources))]
        public string Instructions { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.AssemblyInstructionStep_Images, FieldType: FieldTypes.MediaResources, ResourceType: typeof(ManufacturingResources))]
        public List<EntityHeader> Images {get; set;}
    }
}
