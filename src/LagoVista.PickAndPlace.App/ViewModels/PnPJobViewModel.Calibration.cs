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
            LocatorVM.LocatorState = MVLocatorState.Idle;
        }

        public async void PerformMachineAlignment()
        {
            Machine.SendCommand(SafeHeightGCodeGCode());
            LocatorVM.LocatorState = MVLocatorState.Idle;

            Machine.HomeViaOrigin();

            await Machine.SetViewTypeAsync(ViewTypes.Camera);
            Machine.GotoWorkspaceHome();

            Machine.GotoPoint(Machine.Settings.DefaultWorkspaceHome.X, Machine.Settings.DefaultWorkspaceHome.Y, true);

            SelectMVProfile("mchfiducual");

            LocatorVM.LocatorState = MVLocatorState.MachineFidicual;
        }

        public void GotoMachineFiducial()
        {
            GoToFiducial(0);
        }

        public async void SetMachineFiducial()
        {
            if (MessageBox.Show("Are you sure you want to reset the machine fiducial?", "Reset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Machine.Settings.MachineFiducial.X = Machine.NormalizedPosition.X;
                Machine.Settings.MachineFiducial.Y = Machine.NormalizedPosition.Y;
            }

            await SaveMachineAsync();
        }

        public async void GoToRefPoint()
        {
            SelectMVProfile("tapehold");
            await Machine.SetViewTypeAsync(ViewTypes.Camera);

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


        public override void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation)
        {
            switch (LocatorVM.LocatorState)
            {
                case MVLocatorState.MachineFidicual:
                    JogToLocation(point);
                    break;
                case MVLocatorState.WorkHome:
                    JogToLocation(point);
                    break;
                case MVLocatorState.BoardFidicual1:
                    JogToLocation(point);
                    break;

                case MVLocatorState.BoardFidicual2:
                    JogToLocation(point);
                    break;
                case MVLocatorState.NozzleCalibration:
                    PerformBottomCameraCalibration(point, diameter, stdDeviation);
                    break;
                default:
                    break;
            }
        }

        public void SetBottomCamera()
        {
            Machine.SendCommand($"G92 X{Machine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{Machine.Settings.PartInspectionCamera.AbsolutePosition.Y}");
        }
    }
}
