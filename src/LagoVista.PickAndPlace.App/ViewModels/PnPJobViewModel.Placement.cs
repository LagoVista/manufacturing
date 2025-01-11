using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        public async void PlaceAllParts()
        {
            _partIndex = 0;
             _machineRepo.CurrentMachine.LocationUpdateEnabled = false;
            await  _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.Tool1);
            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            //while (SelectedPart != null && !_isPaused && _partIndex < SelectedPart.Parts.Count)
            //{
            //    await PlacePartAsync(true);
            //}

            // Disable the stepper motor for the rotation of the nozzle, get's very warm
             _machineRepo.CurrentMachine.SendCommand("M18 E");

             _machineRepo.CurrentMachine.LocationUpdateEnabled = true;

             _machineRepo.CurrentMachine.VacuumPump = false;
             _machineRepo.CurrentMachine.PuffPump = false;

            await SaveJobAsync();
        }

        public async void PlacePart()
        {
             _machineRepo.CurrentMachine.LocationUpdateEnabled = false;
            //if (_partIndex >= SelectedPart.Parts.Count)
            //{
            //    _partIndex = 0;
            //}


            await  _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.Tool1);
            // Make sure any pending location requests have completed.
            await Task.Delay(1000);
            await PlacePartAsync();
             _machineRepo.CurrentMachine.LocationUpdateEnabled = true;

            // Disable the stepper motor for the rotation of the nozzle, get's very warm
             _machineRepo.CurrentMachine.SendCommand("M18 E");
        }

        public async Task PlacePartAsync(bool multiple = false)
        {

                if (! _machineRepo.CurrentMachine.VacuumPump || ! _machineRepo.CurrentMachine.PuffPump)
                {
                     _machineRepo.CurrentMachine.VacuumPump = true;
                     _machineRepo.CurrentMachine.PuffPump = true;
                    await Task.Delay(1000);
                }


                var cmds = new List<string>();

                cmds.Add(SafeHeightGCodeGCode()); // Move to move height
                cmds.Add(GetGoToPartInTrayGCode());
                //cmds.Add(WaitForComplete());
                //cmds.Add(WaitForComplete());

                cmds.Add(ProduceVacuumGCode(true)); // Turn on solenoid 
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(PickHeightGCode()); // Move to pick height                
                cmds.Add(DwellGCode(250)); // Wait 500ms to pickup part.
                cmds.Add(SafeHeightGCodeGCode()); // Go to move height

                var cRotation = 0;
//                cmds.Add(GetGoToInspectionCameraGCode(cRotation));
                await SendInstructionSequenceAsync(cmds);


                // Wait for confirmation on all moves
                while ( _machineRepo.CurrentMachine.Busy)
                {
                    await Task.Delay(1);
                }

                var timeout = DateTime.Now.AddMilliseconds(1000);

                //// wait until we identify a rectangle (probably want a time out here)
                //while (!MVLocatedRectangle.HasValue && DateTime.Now < timeout)
                //{
                //    await Task.Delay(1);
                //}

                //if (MVLocatedRectangle.HasValue)
                //{
                //    Debug.WriteLine($"Error: {MVLocatedRectangle.Value.Center.X}x{MVLocatedRectangle.Value.Center.Y} - {MVLocatedRectangle.Value.Angle}");
                //}
                //else
                //{
                //    Debug.WriteLine("we dont have an error so keep going.");
                //}

                //cmds.Clear();
                //cmds.Add(GetGoToPartOnBoardGCode());

                //cmds.Add(PlaceHeightGCode(SelectedPartPackage));
                ////cmds.Add(WaitForComplete());
                //cmds.Add(ProduceVacuumGCode(false));
                //cmds.Add(ProducePuffGCode(true));
                //cmds.Add(DwellGCode(50)); // Wait 500ms to let placed part settle in
                //cmds.Add(SafeHeightGCodeGCode()); // Return to move height.
                //cmds.Add(ProducePuffGCode(false));
                //cmds.Add(RotationGCode(0)); // Ensure we are at zero position before picking up part.
                //cmds.Add(WaitForComplete());
                //await SendInstructionSequenceAsync(cmds);



                //if (!multiple)
                //{
                //     _machineRepo.CurrentMachine.VacuumPump = false;
                //     _machineRepo.CurrentMachine.PuffPump = false;

                //}

        }


        private string GetGoToPartInTrayGCode()
        {
            //var location = StripFeederVM.GetCurrentPartPosition(SelectedPartStrip, PositionType.CurrentPart);
            //if (location != null)
            //{
            //    return $"G0 X{location.X} Y{location.Y} F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";
            //}
            //else
            //{
            //    var deltaX = Math.Abs(XPartInTray.Value -  _machineRepo.CurrentMachine. _machineRepo.CurrentMachinePosition.X);
            //    var deltaY = Math.Abs(YPartInTray.Value -  _machineRepo.CurrentMachine. _machineRepo.CurrentMachinePosition.Y);
            //    var feedRate =  _machineRepo.CurrentMachine.Settings.FastFeedRate;
            //    return $"G0 X{XPartInTray *  _machineRepo.CurrentMachine.Settings.PartStripScaler.X} Y{YPartInTray *  _machineRepo.CurrentMachine.Settings.PartStripScaler.Y} E0 F{feedRate}";
            //}

            return String.Empty;
        }

        public void GoToPartPositionInTray()
        {
            //if (SelectedPartPackage != null && SelectedPartStrip != null)
            //{
            //     _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
            //     _machineRepo.CurrentMachine.SendCommand(GetGoToPartInTrayGCode());
            //}
        }

        //private String GetGoToPartOnBoardGCodeX()
        //{
        //    var offset = (SelectedPartToBePlaced.X - _job.BoardOffset.X) +  _machineRepo.CurrentMachine.Settings.PCBOffset.X;
        //    return $"G1 X{offset * Job.BoardScaler.X}  F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";
        //}

        //private String GetGoToPartOnBoardGCodeY()
        //{
        //    var offset = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y) +  _machineRepo.CurrentMachine.Settings.PCBOffset.Y;
        //    return $"G1 Y{offset * Job.BoardScaler.Y} F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";
        //}

        //private String GetGoToPartOnBoardGCode()
        //{
        //    var offsetX = (SelectedPartToBePlaced.X - _job.BoardOffset.X) +  _machineRepo.CurrentMachine.Settings.PCBOffset.X;
        //    var offsetY = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y) +  _machineRepo.CurrentMachine.Settings.PCBOffset.Y;
        //    return $"G1 X{offsetX}  Y{offsetY} F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";
        //}

        //private String GetGoToInspectionCameraGCode(double rotation)
        //{
        //    //var offsetX =  _machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.X;
        //    //var offsetY =  _machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.Y;

        //    //return $"G1 X{offsetX}  Y{offsetY} Z{ _machineRepo.CurrentMachine.Settings.PartInspectionCamera.FocusHeight} E{rotation} F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";
        //}

        public async Task GoToPartOnBoard()
        {
            //if (SelectedPartToBePlaced != null)
            //{
            //    await  _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.CameraType);
            //     _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());

            //    var offsetY = (SelectedPartToBePlaced.Y - _job.BoardOffset.Y) +  _machineRepo.CurrentMachine.Settings.PCBOffset.Y;
            //    var offsetX = (SelectedPartToBePlaced.X - _job.BoardOffset.X) +  _machineRepo.CurrentMachine.Settings.PCBOffset.X;
            //    var gcode = $"G1 X{offsetX * Job.BoardScaler.X} Y{offsetY * Job.BoardScaler.Y} F{ _machineRepo.CurrentMachine.Settings.FastFeedRate}";

            //     _machineRepo.CurrentMachine.SendCommand(gcode);
            //}
        }
    }
}
