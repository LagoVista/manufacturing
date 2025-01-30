using LagoVista.Core.Commanding;
using System.Threading.Tasks;

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

        RelayCommand PlaceIndependentPartCommand { get; }
        RelayCommand PlaceGroupPartCommand { get; }
        RelayCommand StartJobCommand { get; }
        RelayCommand AbortJobCommand { get; }
        RelayCommand PauseJobCommand { get; }
        RelayCommand ResumeJobCommand { get; }
        RelayCommand ResetJobCommand { get;  }
    }
}
