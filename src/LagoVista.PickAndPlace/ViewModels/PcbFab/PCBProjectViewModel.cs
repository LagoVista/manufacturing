// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 63ff93b7211b19c72f95977b1a2807ac16d6edfb6c1853b5cc5eee09fa7ae83b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Managers;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PickAndPlace.ViewModels.PcbFab.PcbFab
{
    public class PCBProjectViewModel : ViewModelBase
    {
//        public event EventHandler GenerateIsolationEvent;

        PcbMillingProject _project;
        public PcbMillingProject Project
        {
            get { return _project; }
            set { Set(ref _project, value); }
        }

        private PrintedCircuitBoard _pcb;
        public PrintedCircuitBoard PCB
        {
            get { return _pcb; }
            set { Set(ref _pcb, value); }
        }

        public PCBProjectViewModel(PcbMillingProject project)
        {
            Project = project;
            SaveDefaultProfileCommand = new RelayCommand(SaveDefaultProfile);
            OpenEagleBoardCommand = new RelayCommand(OpenEagleBoard);
            OpenTopEtchingCommand = new RelayCommand(OpenTopEtching);
            OpenBottomEtchingCommand = new RelayCommand(OpenBottomEtching);
            CenterBoardCommand = new RelayCommand(CenterBoard);
            //GenerateIsolationMillingCommand = new RelayCommand(GenerateIsolation);

            if (!string.IsNullOrEmpty(Project.EagleBRDFileLocalPath))
            {
                try
                {
                    var doc = XDocument.Load(Project.EagleBRDFileLocalPath);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.FiducialOptions = PCB.Holes.Where(drl => drl.D > 2).ToList();
                }
                catch (Exception) { }
            }
        }

        public void GenerateIsolation()
        {
        //    PCB2Gode.CreateGCode(ViewModel.Project.EagleBRDFileLocalPath, ViewModel.Project);
        }

        public bool CanCenterboard()
        {
            return PCB != null;
        }

        public bool CanGenerateIsolation()
        {
            return PCB != null;
        }

        public Task LoadDefaultSettings()
        {
            return Task.CompletedTask;
            //Project = await Storage.GetAsync<PcbMillingProject>("Default.pcbproj");
            //if (Project == null)
            //{
            //    Project = PcbMillingProject.Default;
            //}
        }

        public async void OpenEagleBoard()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterPCB);
            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    Project.EagleBRDFileLocalPath = result;
                    Project.EagleBRDFile = null;
                    Project.TopEtchingFile = null;
                    Project.BottomEtchingFile = null;

                    var doc = XDocument.Load(Project.EagleBRDFileLocalPath);
                    PCB = EagleParser.ReadPCB(doc);
                    Project.FiducialOptions = PCB.Holes.Where(drl => drl.D > 2).ToList();
                }
                catch
                {
                    await Popups.ShowAsync("Could not open Eage File");
                }
            }
        }

        public async void OpenTopEtching()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
               Project.TopEtchingFileLocalPath = result;
            }
        }

        public async void OpenBottomEtching()
        {
            var result = await Popups.ShowOpenFileAsync(Constants.FileFilterGCode);
            if (!string.IsNullOrEmpty(result))
            {
               Project.BottomEtchingFileLocalPath = result;
            }
        }

        public void CenterBoard()
        {
            Project.ScrapSides = Math.Round((Project.StockWidth - PCB.Width) / 2, 2);
            Project.ScrapTopBottom = Math.Round((Project.StockHeight - PCB.Height) / 2, 2);
        }

        

        public async Task<bool> LoadExistingFile(string file)
        {
            Project = await Storage.GetAsync<PcbMillingProject>(file);
            return Project != null;
        }

        public async void SaveDefaultProfile()
        {
            var brdFileName = Project.EagleBRDFile;
            Project.EagleBRDFile = null;

            await Storage.StoreAsync(Project, "Default.pcbproj");
            Project.EagleBRDFile = brdFileName;
        }

        public RelayCommand CenterBoardCommand { get; private set; }

        public RelayCommand SaveDefaultProfileCommand { get; private set; }
        public RelayCommand OpenEagleBoardCommand { get; private set; }
        public RelayCommand OpenTopEtchingCommand { get; private set; }
        public RelayCommand OpenBottomEtchingCommand { get; private set; }
        public RelayCommand GenerateIsolationMillingCommand { get; private set; }

        public string AddDrillBit(DrillBit bit)
        {
            if (ConsolidatedDrillBit == null)
            {
                return null;
            }

            /* If it already exists here, don't add it again */
            if (ConsolidatedDrillBit.Bits.Where(bt => bt.ToolName == bit.ToolName).Any())
            {
                return null;
            }

            foreach (var consolidatedDrill in Project.ConsolidatedDrillRack)
            {
                if (consolidatedDrill != ConsolidatedDrillBit && consolidatedDrill.Bits.Where(bt => bt.ToolName == bit.ToolName).Any())
                {
                    return consolidatedDrill.NewToolName;
                }
            }

            ConsolidatedDrillBit.AddBit(bit);

            return null;
        }

        public void RemoveBit(DrillBit bit)
        {
            var localBit = ConsolidatedDrillBit.Bits.Where(bt => bt.ToolName == bit.ToolName).FirstOrDefault();
            if (localBit != null)
            {
                ConsolidatedDrillBit.Bits.Remove(localBit);
            }
        }

        ConsolidatedDrillBit _consolidatedDrillBit;
        public ConsolidatedDrillBit ConsolidatedDrillBit
        {
            get { return _consolidatedDrillBit; }
            set { Set(ref _consolidatedDrillBit, value); }
        }
    }
}
