using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using System.Collections.ObjectModel;

namespace LagoVista.Manufacturing.Models
{
    public class MfgModelBase : EntityBase, IValidateable, IDescriptionEntity
    {
        public MfgModelBase()
        {
            ValidationErrors = new ObservableCollection<ErrorMessage>();
            IsValid = true;
        }


        [CloneOptions(false)]
        [FormField(LabelResource: ManufacturingResources.Names.Common_IsValid, FieldType: FieldTypes.Bool, IsUserEditable: false, ResourceType: typeof(ManufacturingResources))]
        public bool IsValid { get; set; }

        [CloneOptions(false)]
        [FormField(LabelResource: ManufacturingResources.Names.Common_ValidationErrors, FieldType: FieldTypes.ChildList, IsUserEditable: false, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<ErrorMessage> ValidationErrors { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, IsUserEditable: false, ResourceType: typeof(ManufacturingResources))]
        public string Description { get; set; }

    }
}
