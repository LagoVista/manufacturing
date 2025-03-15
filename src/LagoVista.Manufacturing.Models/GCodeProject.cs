using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeProject_Title, ManufacturingResources.Names.GCodeProject_Description,
     ManufacturingResources.Names.GCodeProject_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true,
     SaveUrl: "/api/mfg/gcode/project", GetUrl: "/api/mfg/gcode/project/{id}", GetListUrl: "/api/mfg/gcode/projects", FactoryUrl: "/api/mfg/gcode/project/factory",
     DeleteUrl: "/api/mfg/gcode/project/{id}", ListUIUrl: "/mfg/gcodeprojects", EditUIUrl: "/mfg/gcodeproject/{id}", CreateUIUrl: "/mfg/gcodeproject/add")]
    public class GCodeProject : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory
    {
        [FormField(LabelResource: ManufacturingResources.Names.GCodeProject_StockWidth, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double StockWidth { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProject_StockHeight, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double StockHeight { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProject_StockDepth, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double StockDepth { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProject_Origin, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProject_Tools, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/gcode/tool/factory",
            ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodeTool> Tools { get; set; } = new ObservableCollection<GCodeTool>();

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProject_Layers, OpenByDefault:true, FieldType: FieldTypes.ChildListInline, ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodeLayer> Layers { get; set; } = new ObservableCollection<GCodeLayer>();

        public GCodeProjectSummary CreateSummary()
        {
            return new GCodeProjectSummary()
            {
                Id = Id,
                IsDeleted = IsDeleted,
                Key = Key,
                Name = Name,
                Description = Description,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(StockWidth),
                nameof(StockHeight),
                nameof(StockDepth),
                nameof(Origin),
                nameof(Tools),
                nameof(Layers)
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeProject_Title, ManufacturingResources.Names.GCodeProject_Description,
     ManufacturingResources.Names.GCodeProject_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true,
     SaveUrl: "/api/mfg/gcode/project", GetUrl: "/api/mfg/gcode/project/{id}", GetListUrl: "/api/mfg/gcode/projects", FactoryUrl: "/api/mfg/gcode/project/factory",
     DeleteUrl: "/api/mfg/gcode/project/{id}", ListUIUrl: "/mfg/gcodeprojects", EditUIUrl: "/mfg/gcodeproject/{id}", CreateUIUrl: "/mfg/gcodeproject/add")]
    public class GCodeProjectSummary : SummaryData
    {

    }


    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeProjectTool_Title, ManufacturingResources.Names.GCodeProjectTool_Description,
     ManufacturingResources.Names.GCodeProjectTool_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/layer/factory")]
    public class GCodeLayer : IFormDescriptor, IValidateable, IFormAdditionalActions
    {
        [FormField(LabelResource: ManufacturingResources.Names.Common_Location, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProjectTool_SafeMoveHeight, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public double SafeMoveHeight { get; set; } = 5;

        [FormField(LabelResource: ManufacturingResources.Names.GCodeLayer_Holes, OpenByDefault: true, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/mfg/gcode/hole/factory", ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodeHole> Holes { get; set; } = new ObservableCollection<GCodeHole>();

        [FormField(LabelResource: ManufacturingResources.Names.GCodeLayer_Drills, FieldType: FieldTypes.ChildListInline, OpenByDefault: true, FactoryUrl: "/api/mfg/gcode/drill/factory", ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodeDrill> Drill { get; set; } = new ObservableCollection<GCodeDrill>();

        [FormField(LabelResource: ManufacturingResources.Names.GCodeLayer_Rectangles, FieldType: FieldTypes.ChildListInline, OpenByDefault: true, FactoryUrl: "/api/mfg/gcode/rect/factory", ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodeRectangle> Rectangles { get; set; } = new ObservableCollection<GCodeRectangle>();

        [FormField(LabelResource: ManufacturingResources.Names.GCodeLayer_Polygons, FieldType: FieldTypes.ChildListInline, OpenByDefault: true, FactoryUrl: "/api/mfg/gcode/polygon/factory", ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodePolygon> Polygons { get; set; } = new ObservableCollection<GCodePolygon>();

        [FormField(LabelResource: ManufacturingResources.Names.GCodeLayer_Planes, FieldType: FieldTypes.ChildListInline, OpenByDefault: true, FactoryUrl: "/api/mfg/gcode/plane/factory", ResourceType: typeof(ManufacturingResources))]
        public ObservableCollection<GCodePlane> Plane { get; set; } = new ObservableCollection<GCodePlane>();

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()
            {
                 new FormAdditionalAction()
                 {
                     Key = "drillarray",
                     Icon = "fa fa-slider",
                     Title = "Create Drill Array"
                 }
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(SafeMoveHeight),
                nameof(Holes),
                nameof(Drill),
                nameof(Rectangles),
                nameof(Polygons),
                nameof(Plane)
            };
        }
    }

    public enum GCodeToolTypes
    {
        [EnumLabel(GCodeTool.GCodeTool_MillingBit, ManufacturingResources.Names.GCodeToolTypes_MillingBit, typeof(ManufacturingResources))]
        MillingBit,
        [EnumLabel(GCodeTool.GCodeTool_DrillBit, ManufacturingResources.Names.GCodeToolTypes_DrillBit, typeof(ManufacturingResources))]
        DrillBit,
        [EnumLabel(GCodeTool.GCodeTool_Laser, ManufacturingResources.Names.GCodeToolTypes_Laser, typeof(ManufacturingResources))]
        Laser,
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCodeProjectTool_Title, ManufacturingResources.Names.GCodeProjectTool_Description,
     ManufacturingResources.Names.GCodeProjectTool_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/tool/factory")]
    public class GCodeTool : IFormDescriptor, IValidateable
    {
        public const string GCodeTool_MillingBit = "miling";
        public const string GCodeTool_DrillBit = "drill";
        public const string GCodeTool_Laser = "laser";

        public GCodeTool()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(ManufacturingResources))]
        public string Name { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProjectTool_Diameter, FieldType: FieldTypes.Decimal, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double Diameter { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProjectTool_PlungeDepth, FieldType: FieldTypes.Decimal, HelpResource: ManufacturingResources.Names.GCodeProjectTool_PlungeDepth_Help, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double PlungeDepth { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProjectTool_FeedRate, FieldType: FieldTypes.Decimal, HelpResource: ManufacturingResources.Names.GCodeProjectTool_FeedRate_Help, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double FeedRate { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeProjectTool_SpindleRPM, FieldType: FieldTypes.Integer, HelpResource: ManufacturingResources.Names.GCodeProjectTool_SpindleRPM_Help, IsRequired: true, ResourceType: typeof(ManufacturingResources))]
        public double SpindleRpm { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeTool_ToolType, IsRequired: true, FieldType: FieldTypes.Picker, EnumType: typeof(GCodeToolTypes), WaterMark: ManufacturingResources.Names.GCodePojectTool_ToolType_Select, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader<GCodeToolTypes> ToolType { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(ToolType),
                nameof(Diameter),
                nameof(PlungeDepth),
                nameof(FeedRate),
                nameof(SpindleRpm),
            };
        }
    }

    public class GCodeOperation : IFormConditionalFields
    {
        public GCodeOperation()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeOperation_Tool, IsRequired:true, FieldType: FieldTypes.EntityHeaderPicker, PickerType:"gcodeprojectool", ResourceType: typeof(ManufacturingResources))]
        public EntityHeader GcodeOperationTool { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeOperation_Depth, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double Depth { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeOperation_EntireDepth, HelpResource: ManufacturingResources.Names.GCodeOperation_EntireDepth_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool EntireDepth { get; set; } = true;

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(Depth) },
                Conditionals = new List<FormConditional>()
                {
                       new FormConditional()
                       {    
                           Field = nameof(EntireDepth), Value = "false",
                           VisibleFields = new List<string>() { nameof(Depth) },
                           RequiredFields = new List<string>() { nameof(Depth) }
,                       }
                }
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCode_Hole_Title, ManufacturingResources.Names.GCode_Hole_Description,
     ManufacturingResources.Names.GCode_Hole_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/hole/factory")]
    public class GCodeHole : GCodeOperation, IFormDescriptor
    {
        [FormField(LabelResource: ManufacturingResources.Names.Common_Location, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Location { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeHole_Diameter, IsRequired: true, FieldType: FieldTypes.Decimal, ResourceType: typeof(ManufacturingResources))]
        public double Diameter { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(GcodeOperationTool),
                nameof(Location),
                nameof(Diameter),
                nameof(EntireDepth),
                nameof(Depth),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCode_Rectangle_Title, ManufacturingResources.Names.GCode_Rectangle_Description,
     ManufacturingResources.Names.GCode_Rectangle_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/rect/factory")]
    public class GCodeRectangle : GCodeOperation, IFormDescriptor
    {
        [FormField(LabelResource: ManufacturingResources.Names.Common_Origin, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Origin { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Size, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Size { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.GCodeRectangle_CornerRadius, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Radius { get; set; } = new Point2D<double>(0, 0);

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(GcodeOperationTool),
                nameof(Origin),
                nameof(Size),
                nameof(Radius),
                nameof(EntireDepth),
                nameof(Depth),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCode_Plane_Title, ManufacturingResources.Names.GCode_Plane_Description,
     ManufacturingResources.Names.GCode_Plane_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/plane/factory")]
    public class GCodePlane : GCodeOperation, IFormDescriptor
    {
        [FormField(LabelResource: ManufacturingResources.Names.Common_Origin, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]

        public Point2D<double> Origin { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.Common_Size, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Size { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(GcodeOperationTool),
                nameof(Origin),
                nameof(Size),
                nameof(EntireDepth),
                nameof(Depth),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCode_Drill_Title, ManufacturingResources.Names.GCode_Drill_Description,
     ManufacturingResources.Names.GCode_Drill_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/drill/factory")]
    public class GCodeDrill : GCodeOperation, IFormDescriptor
    {
        [FormField(LabelResource: ManufacturingResources.Names.Common_Location, IsRequired: true, FieldType: FieldTypes.Point2D, ResourceType: typeof(ManufacturingResources))]
        public Point2D<double> Location { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(GcodeOperationTool),
                nameof(Location),
                nameof(EntireDepth),
                nameof(Depth),
            };
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.GCode_Polygon_TItle, ManufacturingResources.Names.GCode_Polygon_Description,
     ManufacturingResources.Names.GCode_Polygon_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
     Icon: "icon-fo-laptop-fullscreen", Cloneable: true, FactoryUrl: "/api/mfg/gcode/polygon/factory")]
    public class GCodePolygon : GCodeOperation, IFormDescriptor
    {
        [FormField(LabelResource: ManufacturingResources.Names.GCodePoloygon_Closed, HelpResource: ManufacturingResources.Names.GCodePoloygon_Closed_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(ManufacturingResources))]
        public bool Closed { get; set; } = true;

        [FormField(LabelResource: ManufacturingResources.Names.GCodePoloygon_Points, FieldType: FieldTypes.Point2DArray, ResourceType: typeof(ManufacturingResources))]
        public List<Point2D<double>> Points { get; set; } = new List<Point2D<double>>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(GcodeOperationTool),
                nameof(Closed),
                nameof(EntireDepth),
                nameof(Depth),
                nameof(Points),
            };
        }
    }
}
