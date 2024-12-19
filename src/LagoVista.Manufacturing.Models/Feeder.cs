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
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Feeder_Title, ManufacturingResources.Names.Feeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-searching-2", Cloneable: true,
            SaveUrl: "/api/mfg/feeder", GetUrl: "/api/mfg/Feeder/{id}", GetListUrl: "/api/mfg/feeders", FactoryUrl: "/api/mfg/feeder/factory",
            DeleteUrl: "/api/mfg/feeder/{id}", ListUIUrl: "/mfg/fFeeders", EditUIUrl: "/mfg/feeder/{id}", CreateUIUrl: "/mfg/feeder/add")]
    public class Feeder : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory, IIDEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-pz-searching-2";


        public FeederSummary CreateSummary()
        {
            return new FeederSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                Description = Description,
                Key = Key,
                IsPublic = IsPublic,
                Component = Component?.Text,
                ComponentId = Component?.Id,
                ComponentKey = Component?.Key
            };
        }

        private decimal? _pickX;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickX, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? PickX
        {
            get => _pickX;
            set => Set(ref _pickX, value);
        }

        private decimal? _pickY;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_PickY, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? PickY
        {
            get => _pickY;
            set => Set(ref _pickY, value);
        }

        private decimal? _tapeAngle;
        [FormField(LabelResource: ManufacturingResources.Names.Feeder_TapeAngle, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public decimal? TapeAngle
        {
            get => _tapeAngle;
            set => Set(ref _tapeAngle, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(PickX),
                nameof(TapeAngle),
                nameof(PickY),
                nameof(Component),
                nameof(Description)
            };
        }

        private EntityHeader<Component> _component;
        [FormField(LabelResource: ManufacturingResources.Names.Component_Title, FieldType: FieldTypes.Custom, CustomFieldType: "componentpicker", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<Component> Component
        {
            get => _component;
            set => Set(ref _component, value);
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.Feeders_Title, ManufacturingResources.Names.Feeder_Description,
            ManufacturingResources.Names.Feeder_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources), Icon: "icon-pz-stamp-2", Cloneable: true,
            SaveUrl: "/api/mfg/feeder", GetUrl: "/api/mfg/feeder/{id}", GetListUrl: "/api/mfg/feeders", FactoryUrl: "/api/mfg/feeder/factory",
            DeleteUrl: "/api/mfg/feeder/{id}", ListUIUrl: "/mfg/fFeeders", EditUIUrl: "/mfg/feeder/{id}", CreateUIUrl: "/mfg/feeder/add")]
    public class FeederSummary : SummaryData
    {
        public string ComponentId { get; set; }
        public string ComponentKey { get; set; }
        public string Component { get; set; }
    }
}
