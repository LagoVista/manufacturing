// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2749e5bb4f3ef4eb5722c4ca4cf2b5393c33f81de2c2646d9d0ef2e97c09e74c
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Managers;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab
{
    public interface IHeightMapManager : INotifyPropertyChanged
    {
        /// <summary>
        /// The current H Map
        /// </summary>
        HeightMap HeightMap { get; }

        /// <summary>
        /// Returns true if we have a height map associated with the board.
        /// </summary>
        bool HasHeightMap { get; }

        bool HeightMapDirty { get; }

        HeightMapStatus Status { get; }


        void NewHeightMap(HeightMap heightMap);


        void ProbeCompleted(Vector3 probeResult);
        Task OpenHeightMapAsync(string path);
        Task SaveHeightMapAsync(string path);
        Task SaveHeightMapAsync();
        void CloseHeightMap();

        void Reset();

        void CreateTestPattern();

        /// <summary>
        /// Start a H Map Probing Job
        /// </summary>
        void StartProbing();

        /// <summary>
        /// Pause the current H Map Probing Job
        /// </summary>
        void PauseProbing();

        /// <summary>
        /// Cancel the current H Map Probing Job
        /// </summary>
        void CancelProbing();

        /// <summary>
        /// Outline of the PCB Blank
        /// </summary>
        ObservableCollection<Line3D> RawBoardOutline { get; }

        /// <summary>
        /// The XY Coordinates of the points that will be probed.
        /// </summary>
        ObservableCollection<Vector3> ProbePoints { get; }

        IPCBManager PcbManager { get; }

        IGCodeFileManager GCodeFileManager { get; }

        RelayCommand CancelProbeHeightMapCommand { get; }
        RelayCommand StartProbeHeightMapCommand { get; }
        RelayCommand ClearHeightMapCommand { get; }
        RelayCommand SaveHeightMapCommand { get; }

        RelayCommand ApplyHeightMapCommand { get; }

    }
}
