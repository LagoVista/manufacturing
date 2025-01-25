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
               Icon: "icon-pz-searching-2", Cloneable: true,
               SaveUrl: "/api/mfg/assembly", GetUrl: "/api/mfg/assembly/{id}", GetListUrl: "/api/mfg/assemblies", FactoryUrl: "/api/mfg/assembly/factory",
               DeleteUrl: "/api/mfg/assembly/{id}", ListUIUrl: "/mfg/assemblyinstructions", EditUIUrl: "/mfg/assemblyinstruction/{id}", CreateUIUrl: "/mfg/assemblyinstruction/add")]
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
               Icon: "icon-pz-searching-2", Cloneable: true,
               SaveUrl: "/api/mfg/assembly", GetUrl: "/api/mfg/assembly/{id}", GetListUrl: "/api/mfg/assemblies", FactoryUrl: "/api/mfg/assembly/factory",
               DeleteUrl: "/api/mfg/assembly/{id}", ListUIUrl: "/mfg/assemblyinstructions", EditUIUrl: "/mfg/assemblyinstruction/{id}", CreateUIUrl: "/mfg/assemblyinstruction/add")]
    public class AssemblyInstructionSummary : SummaryData
    {
    }
}
