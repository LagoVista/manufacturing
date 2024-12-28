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


        public FeederSummary CreateSummary()
        {
            return new FeederSummary()
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
