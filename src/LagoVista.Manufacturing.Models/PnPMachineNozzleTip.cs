using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.NozzleTip_Title, ManufacturingResources.Names.NozzleTip_Description,
        ManufacturingResources.Names.NozzleTip_Description, EntityDescriptionAttribute.EntityTypes.ChildObject, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-landing-page", Cloneable: true,
        SaveUrl: "/api/mfg/nozzletip", GetUrl: "/api/mfg/nozzletips/{id}", GetListUrl: "/api/mfg/nozzletips", FactoryUrl: "/api/mfg/nozzletip/factory",
        DeleteUrl: "/api/mfg/nozzletip/{id}", ListUIUrl: "/mfg/nozzletips", EditUIUrl: "/mfg/nozzletip/{id}", CreateUIUrl: "/mfg/nozzletip/add")]
    public class PnPMachineNozzleTip : MfgModelBase, IFormDescriptor, IIconEntity, ISummaryFactory
    {
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_InnerDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double InnerDiameter { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_OuterDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double OuterDiameter { get; set; }



        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-stamp-2";

        public PnPMachineNozzleTipSummary CreateSummary()
        {
            return new PnPMachineNozzleTipSummary()
            {
                Icon = Icon,
                Description = Description,
                Id = Id,
                IsPublic = IsPublic,
                Key = Key,
                Name = Name,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(InnerDiameter),
                nameof(OuterDiameter),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public class PnPMachineNozzleTipSummary : SummaryData
    {

    }
 }
