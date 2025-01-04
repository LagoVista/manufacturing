using LagoVista.PickAndPlace.Models;

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


    public interface ILocatorViewModel
    {
        MVLocatorState LocatorState { get; }
        string Status { get; }
        void SetLocatorState(MVLocatorState state);

        void CirclesLocated(MVLocatedCircles circles);
        void RectLocated(MVLocatedRectangle rect);
        void CornerLocated(MVLocatedCorner corner);
        void CircleLocated(MVLocatedCircle circle);

        void RegisterCircleLocatedHandler(ICircleLocatedHandler handler);
        void UnregisterCircleLocatedHandler(ICircleLocatedHandler handler);

        void RegisterCirclesLocatedHandler(ICirclesLocatedHandler handler);
        void UnregisterCirclesLocatedHandler(ICirclesLocatedHandler handler);

        void RegisterRectangleLocatedHandler(IRectangleLocatedHandler handler);
        void UnregisterRectangleLocatedHandler(IRectangleLocatedHandler handler);
        
        void RegisterCornerLocatedHandler(ICornerLocatedHandler handler);
        void UnRegisterCornerLocatedHandler(ICornerLocatedHandler handler);
    }
}
