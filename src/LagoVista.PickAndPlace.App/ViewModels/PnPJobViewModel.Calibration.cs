using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private void AbortMVLocator()
        {
            LocatorVM.SetLocatorState(MVLocatorState.Idle);
        }

        public async void Perform_machineAlignment()
        {
            _machine.SendCommand(SafeHeightGCodeGCode());
            LocatorVM.SetLocatorState(MVLocatorState.Idle);

            _machine.HomeViaOrigin();

            await _machine.SetViewTypeAsync(ViewTypes.Camera);
            _machine.GotoWorkspaceHome();

            _machine.GotoPoint(_machine.Settings.DefaultWorkspaceHome.X, _machine.Settings.DefaultWorkspaceHome.Y, true);

            VisionManagerVM.SelectMVProfile("mchfiducual");

            LocatorVM.SetLocatorState(MVLocatorState.MachineFidicual);
        }

        public void Goto_machineFiducial()
        {
//            GoToFiducial(0);
        }

        public void Set_machineFiducial()
        {
            if (MessageBox.Show("Are you sure you want to reset the _machine fiducial?", "Reset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _machine.Settings.MachineFiducial.X = _machine.NormalizedPosition.X;
                _machine.Settings.MachineFiducial.Y = _machine.NormalizedPosition.Y;
            }

            //await SaveMachineAsync();
        }

        public async void GoToRefPoint()
        {
            VisionManagerVM.SelectMVProfile("tapehold");
            await _machine.SetViewTypeAsync(ViewTypes.Camera);

            throw new NotImplementedException();
        }

        public Task SetRefPoint()
        {
            throw new NotImplementedException();
        }

        public Task GoToCurrentPartInPartStrip()
        {
            throw new NotImplementedException();
        }



        private void PerformBottomCameraCalibration(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
        }

        public void SetBoardOffset()
        {
            throw new NotImplementedException();
        }

        public async void ClearBoardOffset()
        {
            throw new NotImplementedException();
        }


        public void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
            //switch (LocatorVM.LocatorState)
            //{
            //    case MVLocatorState._machineFidicual:
            //        JogToLocation(point);
            //        break;
            //    case MVLocatorState.WorkHome:
            //        JogToLocation(point);
            //        break;
            //    case MVLocatorState.BoardFidicual1:
            //        JogToLocation(point);
            //        break;

            //    case MVLocatorState.BoardFidicual2:
            //        JogToLocation(point);
            //        break;
            //    case MVLocatorState.NozzleCalibration:
            //        PerformBottomCameraCalibration(point, diameter, stdDeviation);
            //        break;
            //    default:
            //        break;
            //}
        }

    }
}
