using LagoVista.Core.Attributes;
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

        public List<BoardFiducial> BoardFiducials { get; set; } = new List<BoardFiducial>();

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
            get => _stripFeeder;
            set => Set(ref _stripFeeder, value);
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
    }
}
