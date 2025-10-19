// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d2583bd19d14b2466586d607c550bc6ef8e718a8c0db564f81d88f213642c279
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Managers;
using System.ComponentModel;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IProbingManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Begin the process that will be used to set the machine height to the top of the material.
        /// </summary>
        void StartProbe();

        /// <summary>
        /// Abort the process to probe for material height.
        /// </summary>
        void CancelProbe();

        /// <summary>
        /// Probe was completed.
        /// </summary>
        /// <param name="probeResult"></param>
        void ProbeCompleted(Vector3 probeResult);

        ProbeStatus Status { get; }

        /// <summary>
        /// Probing failed.
        /// </summary>
        void ProbeFailed();

        /// <summary>
        /// Send a command to the machine to set the new Z-Axis based on completion of probing
        /// </summary>
        void SetZAxis(double zOffset);

        /// <summary>
        RelayCommand StartProbeCommand { get; }
        RelayCommand CancelProbeCommand { get; }
    }
}
