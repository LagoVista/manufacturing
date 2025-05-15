using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System.Threading.Tasks;
using static LagoVista.PickAndPlace.ViewModels.PickAndPlace.JobExecutionViewModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IJobExecutionViewModel : IMachineViewModelBase
    {

        Task StartJobAsync();
        Task PauseJobAsync();
        Task ResumeJobAsync();
        Task ResetJobAsync();
        Task StopJobAsync();
        Task AbortJobAsync();

        EntityHeader<JobState> State { get; }
        IFeederViewModel ActiveFeederViewModel { get; }

        IVacuumViewModel VacuumViewModel { get; }
        IJobManagementViewModel JobVM { get; }
        ICircuitBoardViewModel PcbVM { get; }
        IPartInspectionViewModel PartInspectionVM { get; }

        RelayCommand GoToPartOnBoardCommand { get; }
        RelayCommand PlaceIndependentPartCommand { get; }
        RelayCommand PlaceGroupPartCommand { get; }
        RelayCommand StartJobCommand { get; }
        RelayCommand AbortJobCommand { get; }
        RelayCommand PauseJobCommand { get; }
        RelayCommand ResumeJobCommand { get; }
        RelayCommand ResetJobCommand { get;  }
    }
}
