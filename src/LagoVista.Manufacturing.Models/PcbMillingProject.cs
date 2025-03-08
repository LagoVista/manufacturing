using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PCB.Eagle.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Models
{
    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PcbMillProject_Title, ManufacturingResources.Names.PcbMillingProject_Description,
        ManufacturingResources.Names.PcbMillingProject_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources), 
        Icon: "icon-ae-control-panel", Cloneable: true,
        SaveUrl: "/api/mfg/pcb/milling", GetUrl: "/api/mfg/pcb/milling/{id}", GetListUrl: "/api/mfg/pcb/millings", FactoryUrl: "/api/mfg/pcb/milling/factory", 
        DeleteUrl: "/api/mfg/pcb/milling/{id}",
        ListUIUrl: "/mfg/pcbmillings", EditUIUrl: "/mfg/pcbmilling/{id}", CreateUIUrl: "/mfg/pcbmilling/add")]
    public class PcbMillingProject : MfgModelBase, IFormDescriptor, IValidateable, ISummaryFactory
    {
        private bool _isEditing;

        public PcbMillingProject()
        {
            Fiducials = new ObservableCollection<Hole>();
            ConsolidatedDrillRack = new ObservableCollection<ConsolidatedDrillBit>();
            _isEditing = false;
        }

        private string _currentFileName;

        private double _scrapSides;
        public double ScrapSides
        {
            get { return _scrapSides; }
            set { Set(ref _scrapSides, value); }
        }

        private double _scrapTopBottom;
        public double ScrapTopBottom 
        {
            get { return _scrapTopBottom; }
            set { Set(ref _scrapTopBottom, value); }
        }

        private EntityHeader _eagleBRDFilePath;
        public EntityHeader EagleBRDFilePath
        {
            get { return _eagleBRDFilePath; }
            set { Set(ref _eagleBRDFilePath, value); }
        }

        private EntityHeader _topEtchingFilePath;
        public EntityHeader TopEtchingFilePath
        {
            get { return _topEtchingFilePath; }
            set { Set(ref _topEtchingFilePath, value); }
        }

        private EntityHeader _bottomEtchingFilePath;
        public EntityHeader BottomEtchingFilePath
        {
            get { return _bottomEtchingFilePath; }
            set { Set(ref _bottomEtchingFilePath, value); }
        }

        public double HoldDownDiameter { get; set; }
        public double HoldDownDrillDiameter { get; set; }
        public double HoldDownDrillDepth { get; set; }
        public double HoldDownBoardOffset { get; set; }

        public double StockWidth { get; set; }
        public double StockHeight { get; set; }
        public double StockThickness { get; set; }

        public List<Drill> GetHoldDownDrills(PrintedCircuitBoard board)
        {

            var radius = HoldDownDiameter / 2;
            var drills = new List<Drill>
            {
                new Drill() { X = ScrapSides - (HoldDownBoardOffset + radius), Y = ScrapTopBottom - (HoldDownBoardOffset + radius), D = HoldDownDiameter },
                new Drill() { X = board.Width + ScrapSides + HoldDownBoardOffset + radius, Y = ScrapTopBottom - (HoldDownBoardOffset + radius), D = HoldDownDiameter },
                new Drill() { X = ScrapSides - (HoldDownBoardOffset + radius), Y = board.Height + ScrapTopBottom + HoldDownBoardOffset + radius, D = HoldDownDiameter },
                new Drill() { X = board.Width + ScrapSides + HoldDownBoardOffset + radius, Y = board.Height + ScrapTopBottom + HoldDownBoardOffset + radius, D = HoldDownDiameter }
            };
            return drills;
        }

        [JsonIgnore]
        public bool IsEditing { get { return _isEditing; } }

        public bool PauseForToolChange { get; set; }
        public double DrillSpindleDwell { get; set; }
        public int DrillSpindleRPM { get; set; }
        public int SafePlungeRecoverRate { get; set; }
        public int DrillPlungeRate { get; set; }
        public double DrillSafeHeight { get; set; }

        public double HeightMapGridSize { get; set; }
        public double MillSpindleRPM { get; set; }
        public double MillSpindleDwell { get; set; }
        public double MillToolSize { get; set; }
        public double MillCutDepth { get; set; }
        public int MillFeedRate { get; set; }
        public int MillPlungeRate { get; set; }
        public double MillSafeHeight { get; set; }

        public bool FlipBoard { get; set; }

        List<Hole> _fiducualOptions;
        public List<Hole> FiducialOptions
        {
            get { return _fiducualOptions; }
            set { Set(ref _fiducualOptions, value); }
        }

        public ObservableCollection<Hole> Fiducials { get; set; }

        public PcbMillingProject Clone()
        {
            return this.MemberwiseClone() as PcbMillingProject;
        }

        public ObservableCollection<ConsolidatedDrillBit> ConsolidatedDrillRack { get; set; }

        public async Task SaveAsync(String fileName = null)
        {

            if(String.IsNullOrEmpty(fileName))
            {
                if(_currentFileName == null)
                {
                    throw new Exception("Must provide a file name if we aren't editing an existing file.");
                }

                fileName = _currentFileName;
            }

            await Core.PlatformSupport.Services.Storage.StoreAsync(this, fileName);

            _currentFileName = fileName;

            _isEditing = true;
        }

        public static async Task<PcbMillingProject> OpenAsync(String fileName)
        {
            var project = await Core.PlatformSupport.Services.Storage.GetAsync<PcbMillingProject>(fileName);
            project._currentFileName = fileName;
            project._isEditing = true;
            return project;
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Description),
            };
        }

        public PcbMillingProjectSummary CreateSummary()
        {
            return new PcbMillingProjectSummary()
            {
                Description = Description,
                Id = Id,
                Key = Key,
                Name = Name,
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }

        public static PcbMillingProject Default
        {
            get
            {
                return new PcbMillingProject()
                {
                    PauseForToolChange = false,
                    StockWidth = 100,
                    StockHeight = 80,
                    HoldDownBoardOffset = 3,
                    HoldDownDiameter = 3,
                    HoldDownDrillDiameter = 2,
                    HoldDownDrillDepth = 5,
                    DrillSpindleDwell = 3,
                    DrillSpindleRPM = 20000,
                    DrillPlungeRate = 200,
                    DrillSafeHeight = 5,
                    StockThickness = 1.57,
                    MillCutDepth = 0.5,
                    MillFeedRate = 500,
                    MillPlungeRate = 200,
                    MillSafeHeight = 5,
                    MillSpindleDwell = 3,
                    MillSpindleRPM = 15000,
                    MillToolSize = 3.15,
                    ScrapTopBottom = 5,
                    ScrapSides = 5,
                    SafePlungeRecoverRate = 500,
                    FlipBoard = true,
                    HeightMapGridSize = 10,
               
                };
            }
        }
    }

    [EntityDescription(ManufacutringDomain.Manufacturing, ManufacturingResources.Names.PcbMillProject_Title, ManufacturingResources.Names.PcbMillingProject_Description,
        ManufacturingResources.Names.PcbMillingProject_Description, EntityDescriptionAttribute.EntityTypes.CircuitBoards, ResourceType: typeof(ManufacturingResources),
        Icon: "icon-ae-control-panel", Cloneable: true,
        SaveUrl: "/api/mfg/pcb/milling", GetUrl: "/api/mfg/pcb/milling/{id}", GetListUrl: "/api/mfg/pcb/millings", FactoryUrl: "/api/mfg/pcb/milling/factory",
        DeleteUrl: "/api/mfg/pcb/milling/{id}",
        ListUIUrl: "/mfg/pcbmillings", EditUIUrl: "/mfg/pcbmilling/{id}", CreateUIUrl: "/mfg/pcbmilling/add")]
    public class PcbMillingProjectSummary : SummaryData
    {

    }
}