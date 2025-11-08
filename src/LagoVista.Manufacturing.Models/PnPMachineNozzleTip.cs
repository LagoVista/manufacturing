// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 62f5df4ad972a5a5f080c93e0a0ea65b44c57cea17dc283c3bf137cf9aa499d2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.NozzleTip_Title, ManufacturingResources.Names.NozzleTip_Description,
        ManufacturingResources.Names.NozzleTip_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-landing-page", Cloneable: true,
        SaveUrl: "/api/mfg/pnp/nozzletip", GetUrl: "/api/mfg/pnp/nozzletip/{id}", GetListUrl: "/api/mfg/pnp/nozzletips", FactoryUrl: "/api/mfg/pnp/nozzletip/factory",
        DeleteUrl: "/api/mfg/pnp/nozzletip/{id}", ListUIUrl: "/mfg/nozzletips", EditUIUrl: "/mfg/nozzletip/{id}", CreateUIUrl: "/mfg/nozzletip/add")]
    public class PnPMachineNozzleTip : MfgModelBase, IFormDescriptor, IIconEntity, ISummaryFactory
    {
        public PnPMachineNozzleTip()
        {
            Id = Guid.NewGuid().ToId();
        }
        
        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-control-panel";

        private double? _height;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_Height, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public double? Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_InnerDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double InnerDiameter { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_OuterDiameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double OuterDiameter { get; set; }


        private decimal? _idleVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_IdleVacuum, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public decimal? IdleVacuum
        {
            get => _idleVacuum;
            set => Set(ref _idleVacuum, value);
        }

        private decimal? _partPickedVacuum;
        [FormField(LabelResource: ManufacturingResources.Names.NozzleTip_PartPickedVacuum, FieldType: FieldTypes.Decimal, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public decimal? PartPickedVacuum
        {
            get => _partPickedVacuum;
            set => Set(ref _partPickedVacuum, value);
        }

        public PnPMachineNozzleTipSummary CreateSummary()
        {
            return new PnPMachineNozzleTipSummary()
            {
                Id = Id,
                Key = Key,
                IsPublic = IsPublic,
                Name = Name,
                Icon = Icon,
                Description = Description,
                InnerDiameter = InnerDiameter,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Height),
                nameof(InnerDiameter),
                nameof(OuterDiameter),
                nameof(IdleVacuum),
                nameof(PartPickedVacuum),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.NozzleTips_Title, ManufacturingResources.Names.NozzleTip_Description,
        ManufacturingResources.Names.NozzleTip_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-fo-landing-page", Cloneable: true,
        SaveUrl: "/api/mfg/pnp/nozzletip", GetUrl: "/api/mfg/pnp/nozzletip/{id}", GetListUrl: "/api/mfg/pnp/nozzletips", FactoryUrl: "/api/mfg/pnp/nozzletip/factory",
        DeleteUrl: "/api/mfg/pnp/nozzletip/{id}", ListUIUrl: "/mfg/nozzletips", EditUIUrl: "/mfg/nozzletip/{id}", CreateUIUrl: "/mfg/nozzletip/add")]
    public class PnPMachineNozzleTipSummary : SummaryData
    {
        public double InnerDiameter { get; set; }
    }
 }
