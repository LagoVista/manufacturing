using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeMapping_Title, ManufacturingResources.Names.GCode_Description,
        ManufacturingResources.Names.GCode_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: true, 
        CreateUIUrl: "/mfg/gcodemapping/add", ListUIUrl: "/mfg/gcodemappings", EditUIUrl: "/mfg/gcodemapping/{id}",
        GetListUrl:"/api/mfg/gcodemappings", GetUrl: "/api/mfg/gcodemapping/{id}", SaveUrl: "/api/mfg/gcodemapping",  DeleteUrl: "/api/mfg/gcodemapping/{id}", FactoryUrl: "/api/mfg/gcodemapping/factory")]
    public class GCodeMapping : MfgModelBase, ISummaryFactory, IIconEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-left";


        [FormField(LabelResource: ManufacturingResources.Names.GCode_TopLightOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string TopLightOn { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_BottomLightOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string BottmLightOn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_TopLightOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string TopLightOff { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_BottomLightOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string BottmLightOff { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_LeftVacuumOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string LeftVacuumOn { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_RightVacuumOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string RightVacuumOn { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_ReadLeftVacuumCmd, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ReadLeftVacuumCmd { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_ReadRightVacuumCmd, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ReadRightVacuumCmd { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_LeftVacuumResponseExample, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string LeftVacuumResponseExample { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_ParseLeftVacuumRegEx, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ParseLeftVacuumRegEx { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_RightVacuumResponseExample, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string RightVacuumResponseExample { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_ParseRightVacuumRegEx, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ParseRightVacuumRegEx { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_HomeAllCommand, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string HomeCommandAll { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_HomeXCommand, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string HomeCommandX { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_HomeYCommand, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string HomeCommandY { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_HomeZCommand, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string HomeCommandZ { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_RequestStatusCommand, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string RequestStatusCommand { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_ParseStatus_RegularExpression, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ParseStatusRegEx { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_StatusResponseExample, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string StatusResponseExample { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_Dwell, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string DWellCommand { get; set; }

        public GCodeMappingSummary CreateSummary()
        {
            return new GCodeMappingSummary()
            {
                Id = Id,
                Name = Name,
                Key = Key,
                Description = Description,
                IsPublic = IsPublic,
                Icon = Icon,
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeMappings_Title, ManufacturingResources.Names.GCode_Description,
        ManufacturingResources.Names.GCode_Description, EntityDescriptionAttribute.EntityTypes.CoreIoTModel, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: true,
        CreateUIUrl: "/mfg/gcodemapping/add", ListUIUrl: "/mfg/gcodemappings", EditUIUrl: "/mfg/gcodemapping/{id}",
        GetListUrl: "/api/mfg/gcodemappings", GetUrl: "/api/mfg/gcodemapping/{id}", SaveUrl: "/api/mfg/gcodemapping", DeleteUrl: "/api/mfg/gcodemapping/{id}", FactoryUrl: "/api/mfg/gcodemapping/factory")]
    public class GCodeMappingSummary : SummaryData
    {

    }
}
