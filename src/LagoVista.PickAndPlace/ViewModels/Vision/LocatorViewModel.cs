﻿using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LagoVista.PickAndPlace.ViewModels.Vision
{
    public class LocatorViewModel : ViewModelBase, ILocatorViewModel
    {
        private List<ICircleLocatedHandler> _circleLocatedHandlers = new List<ICircleLocatedHandler>();
        private List<ICirclesLocatedHandler> _circlesLocatedHandlers = new List<ICirclesLocatedHandler>();
        private List<IRectangleLocatedHandler> _rectLocatedHandlers = new List<IRectangleLocatedHandler>();
        private List<ICornerLocatedHandler> _cornerLocatedHandlers = new List<ICornerLocatedHandler>();

        Timer _circlesLocatedTimeoutTimer;
        Timer _circleLocatedTimeoutTimer;
        Timer _rectLocatedTimeoutTimer;
        Timer _cornerLocatedTimoutTimer;

        IMachineRepo _machineRepo;

        public LocatorViewModel(IMachineRepo machineRepo)
        {
            _machineRepo = machineRepo;
         
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

            SetLocatorStatus();
        }

        private void SetLocatorStatus()
        {
            if (_circlesLocatedHandlers.Any())
            {
                _machineRepo.CurrentMachine.IsLocating = true;
            }
            else if (_circleLocatedHandlers.Any())
            {
                _machineRepo.CurrentMachine.IsLocating = true;
            }
            else if (_rectLocatedHandlers.Any())
            {
                _machineRepo.CurrentMachine.IsLocating = true;
            }
            else if (_cornerLocatedHandlers.Any())
            {
                _machineRepo.CurrentMachine.IsLocating = true;
            }
            else
                _machineRepo.CurrentMachine.IsLocating = false;

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
            if (_circlesLocatedHandlers.Any())
            {
                Status = $"{DateTime.Now:t} - Circles Located - Notify {_rectLocatedHandlers.Count} handlers.";

                foreach (var handler in _circlesLocatedHandlers.ToList())
                {
                    handler.CirclesLocated(circles);
                }
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (_circleLocatedHandlers.Any())
            {
                Status = $"{DateTime.Now:t} - Circle Located - Notify {_rectLocatedHandlers.Count} handlers.";

                foreach (var handler in _circleLocatedHandlers.ToList())
                {
                    handler.CircleLocated(circle);
                }
            }
        }

        public void RectLocated(MVLocatedRectangle rect)
        {
            if (_rectLocatedHandlers.Any())
            {
                Status = $"{DateTime.Now:t} - Rectangle Located - Notify {_rectLocatedHandlers.Count} handlers.";
                foreach (var handler in _rectLocatedHandlers.ToList())
                {
                    handler.RectangleLocated(rect);
                }
            }
        }

        public void CornerLocated(MVLocatedCorner corner)
        {
            if (_cornerLocatedHandlers.Any())
            {
                Status = $"{DateTime.Now:t} - Corner Located - Notify {_rectLocatedHandlers.Count} handlers.";

                foreach (var handler in _cornerLocatedHandlers.ToList())
                {
                    handler.CornerLocated(corner);
                }
            }
        }

        private void RectangleLocatorTimeoutHandler(object state)
        {
            lock (_rectLocatedHandlers)
            {
                if (_rectLocatedTimeoutTimer != null)
                {
                    _rectLocatedTimeoutTimer.Dispose();
                }

                Status = $"{DateTime.Now:t} - Rectangle Locator Timeout - Notify {_rectLocatedHandlers.Count} handlers.";

                foreach (var handler in _rectLocatedHandlers.ToList())
                {
                    handler.RectangleLocatorTimeout();
                }

            }
        }

        public void RegisterRectangleLocatedHandler(IRectangleLocatedHandler handler, int timeoutMS = 5000)
        {
            lock (_rectLocatedHandlers)
            {
                if (_rectLocatedTimeoutTimer != null)
                {
                    _rectLocatedTimeoutTimer.Dispose();
                }

                _rectLocatedHandlers.Add(handler);
                _rectLocatedTimeoutTimer = new Timer(RectangleLocatorTimeoutHandler, handler, TimeSpan.FromMilliseconds(timeoutMS), TimeSpan.Zero);
                _machineRepo.CurrentMachine.IsLocating = true;
            }
        }



        public void UnregisterRectangleLocatedHandler(IRectangleLocatedHandler handler)
        {
            lock (_rectLocatedHandlers)
            {
                if (_rectLocatedTimeoutTimer != null)
                {
                    _rectLocatedTimeoutTimer.Dispose();
                }

                _rectLocatedHandlers.Remove(handler);
            }
            SetLocatorStatus();
        }


        private void CornerLocatorTimedOut(object state)
        {
            lock (_cornerLocatedHandlers)
            {
                _cornerLocatedTimoutTimer?.Dispose();

                Status = $"{DateTime.Now:t} - Corner Locator Timeout - Notify {_cornerLocatedHandlers.Count} handlers.";

                foreach (var handler in _cornerLocatedHandlers)
                {
                    handler.CornerLocatorTimeout();
                }
            }
        }

        public void RegisterCornerLocatedHandler(ICornerLocatedHandler handler, int timeoutMS = 5000)
        {
            lock (_cornerLocatedHandlers)
            {
                if (_cornerLocatedTimoutTimer != null)
                {
                    _cornerLocatedTimoutTimer.Dispose();
                }                

                _cornerLocatedHandlers.Add(handler);
                _cornerLocatedTimoutTimer = new Timer(CornerLocatorTimedOut, handler, timeoutMS, -1);
                _machineRepo.CurrentMachine.IsLocating = true;
            }
        }

        public void UnRegisterCornerLocatedHandler(ICornerLocatedHandler handler)
        {
            lock (_cornerLocatedHandlers)
            {
                _cornerLocatedHandlers.Remove(handler);
            }
            SetLocatorStatus();
        }

        private void CircleLocatorTimedOut(object state)
        {
            lock (_circleLocatedHandlers)
            {
                if (_circleLocatedTimeoutTimer != null)
                {
                    _circleLocatedTimeoutTimer.Dispose();
                }

                Status = $"{DateTime.Now:t} - Circle Locator Timeout - Notify {_circleLocatedHandlers.Count} handlers.";

                foreach (var handler in _circleLocatedHandlers.ToList())
                {
                    handler.CircleLocatorTimeout();
                }
            }
        }

        public void RegisterCircleLocatedHandler(ICircleLocatedHandler handler, int timeoutMS = 5000)
        {
            lock (_circleLocatedHandlers)
            {
                if (_circleLocatedTimeoutTimer != null)
                {
                    _circleLocatedTimeoutTimer.Dispose();
                }

                _circleLocatedHandlers.Add(handler);
                _circleLocatedTimeoutTimer = new Timer(CircleLocatorTimedOut, handler, timeoutMS, -1);
                _machineRepo.CurrentMachine.IsLocating = true;
            }
        }

        public void UnregisterCircleLocatedHandler(ICircleLocatedHandler handler)
        {
            lock (_circleLocatedHandlers)
            {
                _circleLocatedHandlers.Remove(handler);
            }
            _machineRepo.CurrentMachine.IsLocating = true;
        }

        private void CirclesLocatorTimedOut(object state)
        {
            lock (_circlesLocatedHandlers)
            {
                if (_circlesLocatedTimeoutTimer != null)
                {
                    _circlesLocatedTimeoutTimer.Dispose();
                }

                Status = $"{DateTime.Now:t} - Circles Locator Timeout - Notify {_circlesLocatedHandlers.Count} handlers.";

                foreach (var handler in _circlesLocatedHandlers)
                {
                    handler.CirclesLocatorTimeout();
                }
            }
        }

        public void RegisterCirclesLocatedHandler(ICirclesLocatedHandler handler, int timeoutMS = 5000)
        {
            lock (_circlesLocatedHandlers)
            {
                if (_circlesLocatedTimeoutTimer != null)
                {
                    _circlesLocatedTimeoutTimer.Dispose();
                }

                _circlesLocatedHandlers.Add(handler);
                _circlesLocatedTimeoutTimer = new Timer(CirclesLocatorTimedOut, handler, timeoutMS, -1);
                _machineRepo.CurrentMachine.IsLocating = true;
            }
        }



        public void UnregisterCirclesLocatedHandler(ICirclesLocatedHandler handler)
        {
            lock (_circlesLocatedHandlers)
            {
                _circlesLocatedHandlers.Remove(handler);
            }
            SetLocatorStatus();
        }

        public RelayCommand AbortMVLocatorCommand { get; private set; }

    }
}
