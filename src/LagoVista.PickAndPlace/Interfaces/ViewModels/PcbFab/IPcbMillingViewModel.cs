// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c1cb0118fe0d7b59471677d958a36f659943674d20fdaf127299e4660d15f35a
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab
{
    public interface IPcbMillingViewModel : IMachineViewModelBase
    {
        ObservableCollection<PcbMillingProjectSummary> Projects { get; }

        PrintedCircuitBoard PCB { get; }
        PcbMillingProject Project { get; }
        PcbMillingProjectSummary SelectedProject { get; set; }

        RelayCommand CenterBoardCommand { get; }
        RelayCommand CreateProjectCommand { get; }
        RelayCommand GenerateIsolationMillingCommand { get; }

        RelayCommand OpenGCodeFileCommand { get;  }

        RelayCommand OpenProjectCommand { get; }
        RelayCommand EditProjectCommand { get; }
        RelayCommand SaveProjectCommand { get; }

        RelayCommand OpenEagleBoardCommand { get; }
        RelayCommand OpenTopEtchingCommand { get; }
        RelayCommand OpenBottomEtchingCommand { get; }

        RelayCommand ShowBoardMillingGCodeCommand { get; }
        RelayCommand ShowBoardDrillingGCodeCommand { get; }
    
        RelayCommand ShowTopIsolutionGCodeCommand { get; }
        RelayCommand ShowBottomIsolationGCodeCommand { get; }
        RelayCommand ShowHoldDownGCodeCommand { get; }

        RelayCommand ShowBoardBottomMillingGCodeCommand { get; }
        RelayCommand ShowBoardBottomDrillingGCodeCommand { get; }

        public IPCBManager PCBManager { get; }
        public IHeightMapManager HeightMapManager { get; }
        public IGCodeFileManager GCodeFileManager { get; }
    }
}
