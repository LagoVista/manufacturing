using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System.Collections.Generic;

namespace LagoVista.PickAndPlace.ViewModels.Vision
{
    public class LocatorViewModel : ViewModelBase, ILocatorViewModel
    {
        private List<ICircleLocatedHandler> _circleLocatedHandlers = new List<ICircleLocatedHandler>();
        private List<ICirclesLocatedHandler> _circlesLocatedHandlers = new List<ICirclesLocatedHandler>();
        private List<IRectangleLocatedHandler> _rectLocatedHandlers = new List<IRectangleLocatedHandler>();
        private List<ICornerLocatedHandler> _cornerLocatedHandlers = new List<ICornerLocatedHandler>();

        public LocatorViewModel()
        {
            AbortMVLocatorCommand = new RelayCommand(() =>
            {
                SetLocatorState(MVLocatorState.Idle);
                AbortVMLocator();
            });
        }

        private void AbortVMLocator()
        {
            foreach (var handler in _circleLocatedHandlers)
            {
                handler.CircleLocatorAborted();
            }

            foreach (var handler in _rectLocatedHandlers)
            {
                handler.RectangleLocatorAborted();
            }

            foreach (var handler in _cornerLocatedHandlers)
            {
                handler.CornerLocatorAborted();
            }

            foreach (var handler in _circlesLocatedHandlers)
            {
                handler.CirclesLocatorAborted();
            }
        }

        private MVLocatorState _mvLocatorState = MVLocatorState.Default;
        public MVLocatorState LocatorState
        {
            get => _mvLocatorState;
            private set => _mvLocatorState = value;
        }

        private string _status;
        public string Status
        {
            private set => Set(ref _status, value);
            get => _status;
        }

        public void SetLocatorState(MVLocatorState state)
        {
            LocatorState = state;
        }

        public void CirclesLocated(MVLocatedCircles circles)
        {
            foreach(var handler in _circlesLocatedHandlers)
            {
                handler.CirclesLocated(circles);
            }   
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            foreach (var handler in _circleLocatedHandlers)
            {
                handler.CircleLocated(circle);
            }
        }

        public void RectLocated(MVLocatedRectangle rect)
        {
            foreach(var handler in _rectLocatedHandlers)
            {
                handler.RectangleLocated(rect);
            }
        }

        public void CornerLocated(MVLocatedCorner corner)
        {
            foreach(var handler in _cornerLocatedHandlers)
            {
                handler.CornerLocated(corner);
            }
        }

        public void RegisterRectangleLocatedHandler(IRectangleLocatedHandler handler)
        {
            _rectLocatedHandlers.Add(handler);
        }

        public void UnregisterRectangleLocatedHandler(IRectangleLocatedHandler handler)
        {
            _rectLocatedHandlers.Remove(handler);
        }

        public void RegisterCornerLocatedHandler(ICornerLocatedHandler handler)
        {
            _cornerLocatedHandlers.Add(handler);
        }

        public void UnRegisterCornerLocatedHandler(ICornerLocatedHandler handler)
        {
            _cornerLocatedHandlers.Remove(handler);
        }

        public void RegisterCircleLocatedHandler(ICircleLocatedHandler handler)
        {
            _circleLocatedHandlers.Add(handler);
        }

        public void UnregisterCircleLocatedHandler(ICircleLocatedHandler handler)
        {
            _circleLocatedHandlers.Remove(handler);
        }

        public void RegisterCirclesLocatedHandler(ICirclesLocatedHandler handler)
        {
            _circlesLocatedHandlers.Remove(handler);
        }

        public void UnregisterCirclesLocatedHandler(ICirclesLocatedHandler handler)
        {
            _circlesLocatedHandlers.Remove(handler);
        }

        public RelayCommand AbortMVLocatorCommand { get; private set; }

    }
}
