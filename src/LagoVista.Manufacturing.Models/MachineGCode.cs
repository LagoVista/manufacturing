// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 67a336a568b064db55944561c2f592c4f389b3621dd62f8c964169457e50129e
// IndexVersion: 0
// --- END CODE INDEX META ---
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
        ManufacturingResources.Names.GCode_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-fo-gallery-1", Cloneable: true, 
        CreateUIUrl: "/mfg/gcodemapping/add", ListUIUrl: "/mfg/gcodemappings", EditUIUrl: "/mfg/gcodemapping/{id}",
        GetListUrl:"/api/mfg/gcodemappings", GetUrl: "/api/mfg/gcodemapping/{id}", SaveUrl: "/api/mfg/gcodemapping",  DeleteUrl: "/api/mfg/gcodemapping/{id}", FactoryUrl: "/api/mfg/gcodemapping/factory")]
    public class GCodeMapping : MfgModelBase, IFormDescriptor, IFormDescriptorCol2, ISummaryFactory, IIconEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-fo-gallery-1";


        [FormField(LabelResource: ManufacturingResources.Names.GCode_TopLightOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string TopLightOn { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_BottomLightOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string BottmLightOn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_TopLightOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string TopLightOff { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_BottomLightOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string BottmLightOff { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_Exchaust1_On, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Exchause1On { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_Exchaust1_Off, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Exchause1Off { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_Exchaust2_On, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Exchause2On { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_Exchaust2_Off, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string Exchause2Off { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_LeftVacuumOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string LeftVacuumOn { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_RightVacuumOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string RightVacuumOn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_LeftVacuumOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string LeftVacuumOff { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_RightVacuumOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string RightVacuumOff { get; set; }



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
        [FormField(LabelResource: ManufacturingResources.Names.GCode_ParseStatus_RegularExpression, HelpResource:  ManufacturingResources.Names.GCode_ParseStatusRegularExpressionHelp, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ParseStatusRegEx { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.GCode_StatusResponseExample, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string StatusResponseExample { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_Laser_On, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string LaserOn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_Laser_Off, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string LaserOff { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_SpindleOn, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string SpindleOn { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_SpindleOff, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string SpindleOff { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCode_EmergencyStop, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string EmergencyStop { get; set; }


        [FormField(LabelResource: ManufacturingResources.Names.GCode_ToolChange, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(ManufacturingResources))]
        public string ToolChange { get; set; }


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

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Icon),
                nameof(Key),
                nameof(TopLightOn),
                nameof(TopLightOff),
                nameof(BottmLightOn),
                nameof(BottmLightOff),

                nameof(RequestStatusCommand),
                nameof(ParseStatusRegEx),
                nameof(StatusResponseExample),

                nameof(LaserOn),
                nameof(LaserOff),
                nameof(SpindleOn),
                nameof(SpindleOff),
                nameof(EmergencyStop),
                nameof(ToolChange),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(RightVacuumOn),
                nameof(RightVacuumOff),
                nameof(ReadRightVacuumCmd),
                nameof(RightVacuumResponseExample),
                nameof(ParseRightVacuumRegEx),
                

                nameof(LeftVacuumOn),
                nameof(LeftVacuumOff),
                nameof(ReadLeftVacuumCmd),
                nameof(LeftVacuumResponseExample),
                nameof(ParseLeftVacuumRegEx),

                nameof(Exchause1On),
                nameof(Exchause1Off),
                nameof(Exchause2On),
                nameof(Exchause2Off),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeMappings_Title, ManufacturingResources.Names.GCode_Description,
        ManufacturingResources.Names.GCode_Description, EntityDescriptionAttribute.EntityTypes.Manufacturing, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-fo-gallery-1", Cloneable: true,
        CreateUIUrl: "/mfg/gcodemapping/add", ListUIUrl: "/mfg/gcodemappings", EditUIUrl: "/mfg/gcodemapping/{id}",
        GetListUrl: "/api/mfg/gcodemappings", GetUrl: "/api/mfg/gcodemapping/{id}", SaveUrl: "/api/mfg/gcodemapping", DeleteUrl: "/api/mfg/gcodemapping/{id}", FactoryUrl: "/api/mfg/gcodemapping/factory")]
    public class GCodeMappingSummary : SummaryData
    {

    }
}
