// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b79644666564e7c2040311fe0bcefd0e32cd7dd5fbb6d65c3326740f27539e84
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.AssemblyInstruction_Title, ManufacturingResources.Names.AssemblyInstruction_Description,
               ManufacturingResources.Names.AssemblyInstruction_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
               Icon: "icon-fo-instructions", Cloneable: true,
               SaveUrl: "/api/mfg/assembly/instruction", GetUrl: "/api/mfg/assembly/instruction/{id}", GetListUrl: "/api/mfg/assembly/instructions", FactoryUrl: "/api/mfg/assembly/instruction/factory",
               DeleteUrl: "/api/mfg/assembly/instruction/{id}", ListUIUrl: "/mfg/instructions", 
               EditUIUrl: "/mfg/instruction/{id}", CreateUIUrl: "/mfg/instruction/add")]
    public class AssemblyInstruction : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory, IIDEntity
    {
        public AssemblyInstructionSummary CreateSummary()
        {
            return new AssemblyInstructionSummary()
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Key = Key,
            };
        }

        [FormField(LabelResource: ManufacturingResources.Names.AssemblyInstruction_Steps, ChildListDisplayMember: "Title", OpenByDefault: true, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/assembly/step/factory",
                ResourceType: typeof(ManufacturingResources))]
        public List<AssemblyInstructionStep> Steps { get; set; } = new List<AssemblyInstructionStep>();

   
        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.AssemblyInstructions_Title, ManufacturingResources.Names.AssemblyInstruction_Description,
               ManufacturingResources.Names.AssemblyInstruction_Description, EntityDescriptionAttribute.EntityTypes.Summary, ResourceType: typeof(ManufacturingResources),
               Icon: "icon-fo-instructions", Cloneable: true,
               SaveUrl: "/api/mfg/assembly/instruction", GetUrl: "/api/mfg/assembly/instruction/{id}", GetListUrl: "/api/mfg/assembly/instructions", FactoryUrl: "/api/mfg/assembly/instruction/factory",
               DeleteUrl: "/api/mfg/assembly/instruction/{id}", ListUIUrl: "/mfg/instructions",
               EditUIUrl: "/mfg/instruction/{id}", CreateUIUrl: "/mfg/instruction/add")]
    public class AssemblyInstructionSummary : SummaryData
    {
    }
}
