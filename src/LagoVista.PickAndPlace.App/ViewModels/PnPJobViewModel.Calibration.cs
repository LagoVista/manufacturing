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

        public async void Perform_machineCurrentMachineAlignment()
        {
             _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
            LocatorVM.SetLocatorState(MVLocatorState.Idle);

             _machineRepo.CurrentMachine.HomeViaOrigin();

            await  _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.Camera);
             _machineRepo.CurrentMachine.GotoWorkspaceHome();

             _machineRepo.CurrentMachine.GotoPoint( _machineRepo.CurrentMachine.Settings.DefaultWorkspaceHome.X,  _machineRepo.CurrentMachine.Settings.DefaultWorkspaceHome.Y, true);

            VisionManagerVM.SelectMVProfile("mchfiducual");

            LocatorVM.SetLocatorState(MVLocatorState.MachineFidicual);
        }

        public void GotoMachineFiducial()
        {
//            GoToFiducial(0);
        }

        public void SeMachineFiducial()
        {
            if (MessageBox.Show("Are you sure you want to reset the  _machineRepo.CurrentMachine fiducial?", "Reset?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                 _machineRepo.CurrentMachine.Settings.MachineFiducial.X =  _machineRepo.CurrentMachine.NormalizedPosition.X;
                 _machineRepo.CurrentMachine.Settings.MachineFiducial.Y =  _machineRepo.CurrentMachine.NormalizedPosition.Y;
            }

            //await SaveMachineAsync();
        }

        public async void GoToRefPoint()
        {
            VisionManagerVM.SelectMVProfile("tapehold");
            await  _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.Camera);

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
            //    case MVLocatorState. _machineRepo.CurrentMachineFidicual:
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
