// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a0b00caf0beb6a5c23360527f6e4d72ca752aea5968678e64d4c8e791a14abc2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Interfaces
{

    public enum BoardAlignmentManagerStates
    {
        Idle,
        EvaluatingInitialAlignment,
        CenteringFirstFiducial,
        StabilzingAfterFirstFiducialMove,
        MovingToSecondFiducial,
        StabilzingAfterSecondFiducialMove,
        CenteringSecondFiducial,
        BoardAlignmentDetermined,
        TimedOut,
        Failed,
    }

    public interface IBoardAlignmentManager : IDisposable
    {
        void CornerLocated(Point2D<double> offsetFromCenter);

        void CircleLocated(Point2D<double> offsetFromCenter);

        void SetNewMachineLocation(Point2D<double> newLocation);

        void AlignBoard();

        BoardAlignmentManagerStates State { get; set; }

        void CalculateOffsets();
    }
}
