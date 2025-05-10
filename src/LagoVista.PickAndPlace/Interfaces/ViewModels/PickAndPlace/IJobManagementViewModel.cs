using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IJobManagementViewModel : IViewModel
    {
        ObservableCollection<PickAndPlaceJobSummary> Jobs { get; }
        PickAndPlaceJob Job { get; }
        PickAndPlaceJobRun JobRun { get; }
        PickAndPlaceJobSummary SelectedJob { get; set; }
        CircuitBoardRevision CircuitBoard { get; }

        Component CurrentComponent { get; }
        ComponentPackage CurrentComponentPackage { get; }

        AvailablePart SelectedAvailablePart { get; set; }

        PartsGroup PartGroup { get; set; }

        PickAndPlaceJobPlacement Placement { get; set; }

        Task<InvokeResult> SetPartGroupToPlaceAsync(PartsGroup part);
        Task<InvokeResult> SetIndividualPartToPlaceAsync(PickAndPlaceJobPlacement placement);

        IPartsViewModel PartsViewModel { get; }
        bool IsSubstituting { get; }


        Task<InvokeResult> CompletePlacementAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="placement"></param>
        /// <param name="rotated90">If the feeder is rotated/vertical, we need to correct for that.</param>
        /// <returns></returns>
        Task<InvokeResult> RotateCurrentPartAsync(PartsGroup part, PickAndPlaceJobPlacement placement, bool rotated90, bool reverse);

        double GetRotationAngle(PartsGroup part, PickAndPlaceJobPlacement placement, bool rotated90, bool reverse);

        RelayCommand SaveCommand { get; }
        RelayCommand ReloadJobCommand { get; }
        RelayCommand RefreshConfigurationPartsCommand { get; }
        RelayCommand SetBoardOriginCommand { get; }
        RelayCommand CheckBoardFiducialsCommand { get; }

        RelayCommand ShowComponentDetailCommand { get; }
        RelayCommand ShowSchematicCommand { get; }

        RelayCommand ResolvePartsCommand { get; }
        RelayCommand ResolveJobCommand { get; }

        RelayCommand ShowDataSheetCommand { get; }

        RelayCommand SubstitutePartCommand { get; }
        RelayCommand SaveSubstitutePartCommand { get; }
        RelayCommand SaveComponentPackageCommand { get; }
        RelayCommand CancelSubstitutePartCommand { get; }

        RelayCommand CreateJobRunCommand { get; }
        RelayCommand SaveJobRunCommand {get;}

        long Vacuum { get; set; }
    }
}
