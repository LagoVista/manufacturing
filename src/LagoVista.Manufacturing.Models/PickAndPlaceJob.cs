﻿using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace LagoVista.Manufacturing.Models
{

    public enum PnPStates
    {
        [EnumLabel("new", ManufacturingResources.Names.PnPState_New, typeof(ManufacturingResources))]
        New,
        [EnumLabel("idle", ManufacturingResources.Names.PnPState_Idle, typeof(ManufacturingResources))]
        Idle,
        [EnumLabel("error", ManufacturingResources.Names.PnpState_Error, typeof(ManufacturingResources))]
        Error,
        [EnumLabel("feederresolved", ManufacturingResources.Names.PnpState_FeederResolved, typeof(ManufacturingResources))]
        FeederResolved,
        [EnumLabel("validated", ManufacturingResources.Names.PnpState_Validated, typeof(ManufacturingResources))]
        Validated,
        [EnumLabel("atfeeder", ManufacturingResources.Names.PnpState_AtFeeder, typeof(ManufacturingResources))]
        AtFeeder,
        [EnumLabel("partpicked", ManufacturingResources.Names.PnpState_PartPicked, typeof(ManufacturingResources))]
        PartPicked,
        [EnumLabel("detectingpart", ManufacturingResources.Names.PnpState_DetectingPart, typeof(ManufacturingResources))]
        DetectingPart,
        [EnumLabel("partdetected", ManufacturingResources.Names.PnpState_PartDetected, typeof(ManufacturingResources))]
        PartDetected,
        [EnumLabel("partnotdetected", ManufacturingResources.Names.PnpState_PartNotDetected, typeof(ManufacturingResources))]
        PartNotDetected,
        [EnumLabel("inspecting", ManufacturingResources.Names.PnPState_Inspecting, typeof(ManufacturingResources))]
        Inspecting,
        [EnumLabel("inspected", ManufacturingResources.Names.PnPState_Inspected, typeof(ManufacturingResources))]
        Inspected,
        [EnumLabel("pickercompensating", ManufacturingResources.Names.PnPState_PickErrorCompensating, typeof(ManufacturingResources))]
        PickErrorCompensating,
        [EnumLabel("pickerrorcompensated", ManufacturingResources.Names.PnPState_PickErrorCompensated, typeof(ManufacturingResources))]
        PickErrorCompensated,
        [EnumLabel("pickerronotcompensated", ManufacturingResources.Names.PnPState_PickErrorNotCompensated, typeof(ManufacturingResources))]
        PickErrorNotCompensated,
        [EnumLabel("atplacelocation", ManufacturingResources.Names.PnPState_AtPlaceLocation, typeof(ManufacturingResources))]
        AtPlaceLocation,
        [EnumLabel("onboard", ManufacturingResources.Names.PnPState_OnBoard, typeof(ManufacturingResources))]
        OnBoard,
        [EnumLabel("placed", ManufacturingResources.Names.PnPState_Placed, typeof(ManufacturingResources))]
        Placed,
        [EnumLabel("advanced", ManufacturingResources.Names.PnPState_Advanced, typeof(ManufacturingResources))]
        Advanced,
        [EnumLabel("placementcompleted", ManufacturingResources.Names.PnPState_PlacementCompleted, typeof(ManufacturingResources))]
        PlacementCompleted,
        [EnumLabel("partcompleted", ManufacturingResources.Names.PnPState_JobCompleted, typeof(ManufacturingResources))]
        PartCompleted,
        [EnumLabel("jobcompleted", ManufacturingResources.Names.JobState_Completed, typeof(ManufacturingResources))]
        JobCompleted,
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJob_Title, ManufacturingResources.Names.PickAndPlaceJob_Description,
    ManufacturingResources.Names.PickAndPlaceJob_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
    SaveUrl: "/api/mfg/pnpjob", GetUrl: "/api/mfg/pnpjob/{id}", GetListUrl: "/api/mfg/pnpjobs", FactoryUrl: "/api/mfg/pnpjob/factory", DeleteUrl: "/api/mfg/pnpjob/{id}",
    ListUIUrl: "/mfg/pnpjobs", EditUIUrl: "/mfg/pnpjob/{id}", CreateUIUrl: "/mfg/pnpjob/add")]
    public class PickAndPlaceJob : MfgModelBase, IValidateable, IFormDescriptor, ISummaryFactory, IIDEntity
    {

        [FormField(LabelResource: ManufacturingResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(ManufacturingResources))]
        public string Icon { get; set; } = "icon-ae-control-panel";

        public PickAndPlaceJobSummary CreateSummary()
        {
            return new PickAndPlaceJobSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                Description = Description,
                Key = Key,
                IsPublic = IsPublic
            };
        }

        private Point2D<double> _defaultWorkOrigin = new Point2D<double>(0, 0);
        public Point2D<double> DefaultBoardOrigin
        {
            get { return _defaultWorkOrigin; }
            set { Set(ref _defaultWorkOrigin, value); }
        }

        private Point2D<double> _actualWorkOrigin = new Point2D<double>(0, 0);
        public Point2D<double> ActualBoardOrigin
        {
            get { return _actualWorkOrigin; }
            set { Set(ref _actualWorkOrigin, value); }
        }


        ObservableCollection<BoardFiducial> _boardFiducials;
        public ObservableCollection<BoardFiducial> BoardFiducials
        {
            get => _boardFiducials;
            set => Set(ref _boardFiducials, value);
        }

        public double BoardAngle { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_CurrentSerialNumber, FieldType: FieldTypes.Integer,  ResourceType: typeof(ManufacturingResources))]
        public int CurrentSerialNumber { get; set; } = 1000;

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Cost, IsUserEditable: true, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public double Cost { get; set; }
        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Extneded, IsUserEditable: true, FieldType: FieldTypes.Money, ResourceType: typeof(ManufacturingResources))]
        public double Extended { get; set; }

        [FormField(LabelResource: ManufacturingResources.Names.PickAndPlaceJob_Board, IsUserEditable:false, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(ManufacturingResources))]
        public EntityHeader Board { get; set; }

        public CircuitBoardRevision BoardRevision { get; set; }

        public ObservableCollection<PickAndPlaceJobPart> Parts { get; set; } = new ObservableCollection<PickAndPlaceJobPart>();

        public ObservableCollection<ErrorMessage> ErrorMessages { get; set; } = new ObservableCollection<ErrorMessage>();

        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        public ObservableCollection<string> Errors 
        {
            get => _errors;
            set => Set(ref _errors, value);
        } 

        private ObservableCollection<string> _warnings = new ObservableCollection<string>();
        public ObservableCollection<string> Warnings
        {
            get => _warnings;
            set => Set(ref _warnings, value);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Board),
                nameof(Cost),
                nameof(Extended),
                nameof(Description)
            };
        }

        Core.Interfaces.ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PickAndPlaceJobs_Title, ManufacturingResources.Names.PickAndPlaceJob_Description,
        ManufacturingResources.Names.PartPack_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), Icon: "icon-ae-control-panel", Cloneable: true,
        SaveUrl: "/api/mfg/pnpjob", GetUrl: "/api/mfg/pnpjob/{id}", GetListUrl: "/api/mfg/pnpjobs", FactoryUrl: "/api/mfg/pnpjob/factory", DeleteUrl: "/api/mfg/pnpjob/{id}",
        ListUIUrl: "/mfg/pnpjobs", EditUIUrl: "/mfg/pnpjob/{id}", CreateUIUrl: "/mfg/pnpjob/add")]
    public class PickAndPlaceJobSummary : SummaryData
    {

    }

    public class BoardFiducial : ModelBase
    {
        private string _name;
        public string Name 
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private Point2D<double> _expected;
        public Point2D<double> Expected 
        {
            get => _expected;
            set => Set(ref _expected, value);
        }
        
        private Point2D<double> _actual;
        public Point2D<double> Actual 
        {
            get => _actual;
            set => Set(ref _actual, value);
        }
    }

    public class PickAndPlaceJobPart : ModelBase
    {
        private string _value;
        public string Value 
        {
            get => _value;
            set => Set(ref _value, value);
        }

        private string _packageName;
        public string PackageName
        {
            get => _packageName;
            set => Set(ref _packageName, value);
        }


        private EntityHeader _autoFeeder;
        public EntityHeader AutoFeeder
        {
            get => _autoFeeder;
            set => Set(ref _autoFeeder, value);
        }

        EntityHeader _stripFeeder;
        public EntityHeader StripFeeder
        {
            get => _stripFeeder;
            set => Set(ref _stripFeeder, value);
        }

        EntityHeader _stripFeederRow;
        public EntityHeader StripFeederRow
        {
            get => _stripFeederRow;
            set => Set(ref _stripFeederRow, value);
        }

        EntityHeader _component;
        public EntityHeader Component
        {
            get => _component;
            set => Set(ref _component, value);
        }

         EntityHeader _componentPackage;
        public EntityHeader ComponentPackage
        {
            get => _componentPackage;
            set => Set(ref _componentPackage, value);
        }

        int _availableCount;
        public int AvailableCount
        {
            get => _availableCount;
            set => Set(ref _availableCount, value);
        }

        ObservableCollection<PickAndPlaceJobPlacement> _placements = new ObservableCollection<PickAndPlaceJobPlacement>();
        public ObservableCollection<PickAndPlaceJobPlacement> Placements 
        {
            get => _placements;
            set
            {
                Set(ref _placements, value);
                RaisePropertyChanged(nameof(Count));
                if(_placements != null)
                {
                    _placements.CollectionChanged += (s, a) => RaisePropertyChanged(nameof(Count));
                }
            }            
        }

        ObservableCollection<string> _errors = new ObservableCollection<string>();
        public ObservableCollection<string> Errors
        {
            get => _errors;
            set => Set(ref _errors, value);
        }

        public int Count => Placements.Count;
        
        public InvokeResult Validate()
        {
            if(AvailableCount < Count)
            {
                return InvokeResult.FromError("not enough parts to place.");
            }

            return InvokeResult.Success;
        }


        public override string ToString()
        {
            return string.Join(',', Placements.Select(pl => pl.Name));
        }
    }

    public class PickAndPlaceJobPlacement : ModelBase
    {
        private string _name;
        public string Name 
        {
            get => _name;
            set => Set(ref _name, value);
        }

        Point2D<double> _pcbLocation;
        public Point2D<double> PCBLocation
        {
            get => _pcbLocation;
            set => Set(ref _pcbLocation, value);
        }

        double _rotation;
        public double Rotation
        {
            get => _rotation;
            set => Set(ref _rotation, value);
        }

        bool _placed;
        public bool Placed
        {
            get => _placed;
            set => Set(ref _placed, value);
        }

        string _placeTimeStamp;
        public string PlaceTimeStamp
        {
            get => _placeTimeStamp;
            set => Set(ref _placeTimeStamp, value);
        }

        ObservableCollection<string> _errors = new ObservableCollection<string>();
        public ObservableCollection<string> Errors
        {
            get => _errors;
            set => Set(ref _errors, value); 
        }

        Point2D<double> _pickErrorOffset;
        public Point2D<double> PickErrorOffset
        {
            get => _pickErrorOffset;
            set => Set(ref _pickErrorOffset, value);
        }

        double _pickErrorAngle;
        public double PickErrorAngle
        {
            get => _pickErrorAngle;
            set => Set(ref _pickErrorAngle, value);
        }

        EntityHeader<PnPStates> _state = EntityHeader<PnPStates>.Create(PnPStates.New);
        public EntityHeader<PnPStates> State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        private string _lastError;
        public string LastError
        {
            get => _lastError;
            set => Set(ref _lastError, value);
        }
    }
}
