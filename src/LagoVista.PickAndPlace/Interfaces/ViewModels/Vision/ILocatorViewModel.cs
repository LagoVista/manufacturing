// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 05a716a1edc0258a58ec0a31316b16aac11b457ba8a8480a24184ffa9e6ca047
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Models;
using System;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public enum MVLocatorState
    {
        Idle,
        MachineFidicual,
        BoardFidicual1,
        BoardFidicual2,
        Default,
        NozzleCalibration,
        WorkHome,
        PartInTape,
    }

    public interface ILocatorViewModel : IViewModel
    {
        MVLocatorState LocatorState { get; }
        string Status { get; }
        bool IsLocating { get; }
        TimeSpan? Duration { get; }
        void SetLocatorState(MVLocatorState state);

        void CirclesLocated(MVLocatedCircles circles);
        void RectLocated(MVLocatedRectangle rect);
        void CornerLocated(MVLocatedCorner corner);
        void CircleLocated(MVLocatedCircle circle);

        void RegisterCircleLocatedHandler(ICircleLocatedHandler handler, int timeoutMS = 5000);
        void UnregisterCircleLocatedHandler(ICircleLocatedHandler handler);

        void RegisterCirclesLocatedHandler(ICirclesLocatedHandler handler, int timeoutMS = 5000);
        void UnregisterCirclesLocatedHandler(ICirclesLocatedHandler handler);

        void RegisterRectangleLocatedHandler(IRectangleLocatedHandler handler, int timeoutMS = 5000);
        void UnregisterRectangleLocatedHandler(IRectangleLocatedHandler handler);
        
        void RegisterCornerLocatedHandler(ICornerLocatedHandler handler, int timeoutMS = 5000);
        void UnRegisterCornerLocatedHandler(ICornerLocatedHandler handler);
    }
}
