// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2b2efb6680d2418d9c6193aa357331069201e0b070364dd8c0a60c3b46d5d087
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PartPack_Title, ManufacturingResources.Names.PartPack_Description,
            ManufacturingResources.Names.PartPack_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-folders", Cloneable: true,
            SaveUrl: "/api/mfg/partpack", GetUrl: "/api/mfg/partpack/{id}", GetListUrl: "/api/mfg/partpacks", FactoryUrl: "/api/mfg/partpack/factory", DeleteUrl: "/api/mfg/partpack/{id}",
            ListUIUrl: "/mfg/partpacks", EditUIUrl: "/mfg/partpack/{id}", CreateUIUrl: "/mfg/partpack/add")]
    public class PartPack : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory, IIDEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-folders";


        public AutoFeederSummary CreateSummary()
        {
            return new AutoFeederSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                Description = Description,
                Key = Key,
                IsPublic = IsPublic
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Description)
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

    }


    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PartPacks_Title, ManufacturingResources.Names.PartPack_Description,
           ManufacturingResources.Names.PartPack_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-folders", Cloneable: true,
           SaveUrl: "/api/mfg/partpack", GetUrl: "/api/mfg/partpack/{id}", GetListUrl: "/api/mfg/partpacks", FactoryUrl: "/api/mfg/partpack/factory", DeleteUrl: "/api/mfg/partpack/{id}",
           ListUIUrl: "/mfg/partpacks", EditUIUrl: "/mfg/partpack/{id}", CreateUIUrl: "/mfg/partpack/add")]
    public class PartPackSummary : SummaryData
    {

    }
}
