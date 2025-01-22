using LagoVista.Core;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Diagnostics;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public partial class MachineControlViewModel
    {
        public void CycleStart()
        {
            _machineRepo.CurrentMachine.CycleStart();
        }

        public void SoftReset()
        {
            _machineRepo.CurrentMachine.SoftReset();
        }

        public void FeedHold()
        {
            _machineRepo.CurrentMachine.FeedHold();
        }

        public void ClearAlarm()
        {
            _machineRepo.CurrentMachine.ClearAlarm();
        }

        private void RelativeJog(JogDirections direction)
        {
            _machineRepo.CurrentMachine.SendCommand("G91");

            switch (direction)
            {
                case JogDirections.XPlus:
                    _machineRepo.CurrentMachine.SendCommand($"{_machineRepo.CurrentMachine.Settings.JogGCodeCommand} X{XYStepSize.ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    _machineRepo.CurrentMachine.SendCommand($"{_machineRepo.CurrentMachine.Settings.JogGCodeCommand} Y{(XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} X{(-XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Y{(-XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Minus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Minus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T1Plus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(-ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
            }

            _machineRepo.CurrentMachine.SendCommand("G90");
        }

        private const double ShaftOffsetCorrection = 0.5;

        private void AbsoluteJog(JogDirections direction)
        {
            var current = _machineRepo.CurrentMachine.WorkspacePosition;

            switch (direction)
            {
                case JogDirections.XPlus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} X{(current.X + XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YPlus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Y{(current.Y + XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZPlus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(current.Z + ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.XMinus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} X{(current.X - XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.YMinus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Y{(current.Y - XYStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.ZMinus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(current.Z - ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Minus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(_machineRepo.CurrentMachine.ToolCommonZ - ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                case JogDirections.T0Plus:
                    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(_machineRepo.CurrentMachine.ToolCommonZ + ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                    break;
                //case JogDirections.T1Minus:
                //    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(_machineRepo.CurrentMachine.Tool1 - ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                //    break;
                //case JogDirections.T1Plus:
                //    _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} Z{(_machineRepo.CurrentMachine.Tool1 + ZStepSize).ToDim()} F{ _machineRepo.CurrentMachine.Settings.JogFeedRate}");
                //    break;
                case JogDirections.LeftToolHeadRotateMinus:
                    {
                        var newAngle = Math.Round(_machineRepo.CurrentMachine.LeftToolHeadRotate - 90);
                        var normalizedAngle = newAngle % 360;
                        if (normalizedAngle < 0)
                        {
                            normalizedAngle += 360;
                        }

                        var xOffset = -(Math.Sin(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);
                        var yOffset = -(Math.Cos(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);

                        //_machineRepo.CurrentMachine.SendCommand("G90");
                        _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} A{(newAngle).ToDim()} F5000");
                        //_machineRepo.CurrentMachine.SendCommand("G90");

                        //_machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} A{(newAngle).ToDim()} F5000");
                        ///*                        _machineRepo.CurrentMachine.SendCommand("G91");
                        //                        _machineRepo.CurrentMachine.SendCommand($"G0 X{xOffset.ToDim()}  Y{yOffset.ToDim()}");
                        //                        _machineRepo.CurrentMachine.SendCommand("G90");*/

                        //Debug.WriteLine($"New Angle {newAngle}°, Normalized {normalizedAngle}° Correction: ({xOffset.ToDim()} - {yOffset.ToDim()})");
                    }
                    break;

                case JogDirections.LeftToolHeadRotatePlus:
                    {
                        var newAngle = Math.Round(_machineRepo.CurrentMachine.LeftToolHeadRotate + 90);
                        var normalizedAngle = newAngle % 360;
                        if (normalizedAngle < 0)
                        {
                            normalizedAngle += 360;
                        }

                        var xOffset = -(Math.Sin(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);
                        var yOffset = -(Math.Cos(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);

                        _machineRepo.CurrentMachine.SendCommand($"{ _machineRepo.CurrentMachine.Settings.JogGCodeCommand} A{(newAngle).ToDim()} F5000");
                        /*Machine.SendCommand("G91");
                        _machineRepo.CurrentMachine.SendCommand($"G0 X{xOffset.ToDim()}  Y{yOffset.ToDim()}");
                        _machineRepo.CurrentMachine.SendCommand("G90");*/

                        Debug.WriteLine($"New Angle {newAngle}°, Normalized {normalizedAngle}° Correction: ({xOffset.ToDim()} - {yOffset.ToDim()})");

                    }
                    break;
                case JogDirections.RightToolHeadRotateMinus:
                    {
                        var newAngle = Math.Round(_machineRepo.CurrentMachine.RightToolHeadRotate - 90);
                        var normalizedAngle = newAngle % 360;
                        if (normalizedAngle < 0)
                        {
                            normalizedAngle += 360;
                        }

                        var xOffset = -(Math.Sin(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);
                        var yOffset = -(Math.Cos(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);

                        _machineRepo.CurrentMachine.SendCommand($"{_machineRepo.CurrentMachine.Settings.JogGCodeCommand} B{(newAngle).ToDim()} F5000");
                        ///*                        _machineRepo.CurrentMachine.SendCommand("G91");
                        //                        _machineRepo.CurrentMachine.SendCommand($"G0 X{xOffset.ToDim()}  Y{yOffset.ToDim()}");
                        //                        _machineRepo.CurrentMachine.SendCommand("G90");*/

                        //Debug.WriteLine($"New Angle {newAngle}°, Normalized {normalizedAngle}° Correction: ({xOffset.ToDim()} - {yOffset.ToDim()})");
                    }
                    break;

                case JogDirections.RightToolHeadRotatePlus:
                    {
                        var newAngle = Math.Round(_machineRepo.CurrentMachine.RightToolHeadRotate + 90);
                        var normalizedAngle = newAngle % 360;
                        if (normalizedAngle < 0)
                        {
                            normalizedAngle += 360;
                        }

                        var xOffset = -(Math.Sin(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);
                        var yOffset = -(Math.Cos(normalizedAngle.ToRadians()) * ShaftOffsetCorrection);

                        _machineRepo.CurrentMachine.SendCommand($"{_machineRepo.CurrentMachine.Settings.JogGCodeCommand} B{(newAngle).ToDim()} F5000");
                        ///*Machine.SendCommand("G91");
                        //_machineRepo.CurrentMachine.SendCommand($"G0 X{xOffset.ToDim()}  Y{yOffset.ToDim()}");
                        //_machineRepo.CurrentMachine.SendCommand("G90");*/

                        //Debug.WriteLine($"New Angle {newAngle}°, Normalized {normalizedAngle}° Correction: ({xOffset.ToDim()} - {yOffset.ToDim()})");

                    }
                    break;
            }
        }

        public void Jog(JogDirections direction)
        {
            if ((_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.Repeteir_PnP ||
                _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.LumenPnP_V4_Marlin ||
                _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.Marlin_Laser ||
                _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.Marlin ||
                _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.GRBL1_1) &&
                (direction != JogDirections.LeftToolHeadRotatePlus && direction != JogDirections.LeftToolHeadRotateMinus && 
                direction != JogDirections.RightToolHeadRotatePlus && direction != JogDirections.RightToolHeadRotateMinus)
                )
            {
                RelativeJog(direction);
            }
            else
            {
                AbsoluteJog(direction);
            }
        }

        public void Home(HomeAxis axis)
        {
            switch (axis)
            {
                case HomeAxis.All:
                    _machineRepo.CurrentMachine.SendCommand("G28 X Y");
                    _machineRepo.CurrentMachine.SendCommand("T0");
                    _machineRepo.CurrentMachine.SendCommand("G28 Z");
                    _machineRepo.CurrentMachine.SendCommand("T1");
                    _machineRepo.CurrentMachine.SendCommand("G28 Z");
                    _machineRepo.CurrentMachine.SendCommand("T1");
                    _machineRepo.CurrentMachine.SendCommand("G28 Z");
                    break;
                case HomeAxis.X:
                    _machineRepo.CurrentMachine.SendCommand("G28 X");
                    break;
                case HomeAxis.Y:
                    _machineRepo.CurrentMachine.SendCommand("G28 Y");
                    break;
                case HomeAxis.Z:
                    _machineRepo.CurrentMachine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T0:
                    _machineRepo.CurrentMachine.SendCommand("G28 Z");
                    break;
                case HomeAxis.T1:
                    _machineRepo.CurrentMachine.SendCommand("G28 P");
                    break;
                case HomeAxis.C:
                    _machineRepo.CurrentMachine.SendCommand("G28 C");
                    break;
            }
        }

        public void ResetAxisToZero(ResetAxis axis)
        {
            switch (axis)
            {
                case ResetAxis.All:
                    if (_machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.GRBL1_1)
                    {
                        _machineRepo.CurrentMachine.SendCommand("G10 P0 L20 X0 Y0 Z0");
                    }
                    else
                    {
                        _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 X0 Y0 Z0 C0");
                    }
                    break;
                case ResetAxis.X:
                    _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 X0");
                    break;
                case ResetAxis.Y:
                    _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 Y0");
                    break;
                case ResetAxis.Z:
                    _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 Z0");
                    break;
                case ResetAxis.T0:
                    _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 Z0");
                    break;
                case ResetAxis.T1:
                    _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 Z0");
                    break;
                case ResetAxis.C:
                    _machineRepo.CurrentMachine.SendCommand("G10 L20 P0 C0");
                    break;


            }
        }

        public void HandleKeyDown(WindowsKey key, bool isShift, bool isControl)
        {
            switch (key)
            {
                case WindowsKey.Home:
                    if (isControl)
                    {
                        _machineRepo.CurrentMachine.SetWorkspaceHome();
                    }
                    else if (isShift)
                    {
                        _machineRepo.CurrentMachine.HomingCycle();
                    }
                    else
                    {
                        _machineRepo.CurrentMachine.GotoWorkspaceHome();
                    }

                    break;

                case WindowsKey.Left:
                    if (isControl)
                    {
                        Jog(JogDirections.LeftToolHeadRotateMinus);
                    }
                    else
                    {
                        Jog(JogDirections.XMinus);
                    }
                    break;
                case WindowsKey.Right:
                    if (isControl)
                    {
                        Jog(JogDirections.LeftToolHeadRotatePlus);
                    }
                    else
                    {
                        Jog(JogDirections.XPlus);
                    }
                    break;
                case WindowsKey.Up:
                    if (isControl)
                    {
                        Jog(JogDirections.T0Minus);
                    }
                    else
                    {
                        Jog(JogDirections.YPlus);
                    }
                    break;
                case WindowsKey.Down:
                    if (isControl)
                    {
                        Jog(JogDirections.T0Plus);
                    }
                    else
                    {
                        Jog(JogDirections.YMinus);
                    }
                    break;

                case WindowsKey.OemPlus:

                    switch (XYStepMode)
                    {
                        case StepModes.XLarge:
                            XYStepSizeSlider += 10;
                            XYStepSizeSlider = Math.Min(100, XYStepSizeSlider);
                            break;
                        case StepModes.Large:
                            XYStepSizeSlider = XYStepSizeSlider += 5;
                            XYStepSizeSlider = Math.Min(20, XYStepSizeSlider);
                            break;
                        case StepModes.Medium:
                            XYStepSizeSlider = XYStepSizeSlider += 1;
                            XYStepSizeSlider = Math.Min(10, XYStepSizeSlider);
                            break;
                        case StepModes.Small:
                            XYStepSizeSlider = XYStepSizeSlider += 0.1;
                            XYStepSizeSlider = Math.Min(1, XYStepSizeSlider);
                            break;
                        case StepModes.Micro:
                            XYStepSizeSlider = XYStepSizeSlider += 0.01;
                            XYStepSizeSlider = Math.Min(0.1, XYStepSizeSlider);
                            break;
                    }

                    break;
                case WindowsKey.OemMinus:


                    switch (XYStepMode)
                    {
                        case StepModes.XLarge:
                            XYStepSizeSlider = XYStepSizeSlider -= 10;
                            XYStepSizeSlider = Math.Max(20, XYStepSizeSlider);
                            break;
                        case StepModes.Large:
                            XYStepSizeSlider = XYStepSizeSlider -= 2.5;
                            XYStepSizeSlider = Math.Max(10, XYStepSizeSlider);
                            break;
                        case StepModes.Medium:
                            XYStepSizeSlider = XYStepSizeSlider -= 1;
                            XYStepSizeSlider = Math.Max(1, XYStepSizeSlider);
                            break;
                        case StepModes.Small:
                            XYStepSizeSlider = XYStepSizeSlider -= 0.1;
                            XYStepSizeSlider = Math.Max(0.1, XYStepSizeSlider);
                            break;
                        case StepModes.Micro:
                            XYStepSizeSlider = XYStepSizeSlider -= 0.01;
                            XYStepSizeSlider = Math.Max(0.01, XYStepSizeSlider);

                            break;
                    }


                    break;

                case WindowsKey.PageUp:
                    if (isControl)
                    {
                        switch (ZStepMode)
                        {
                            case StepModes.XLarge:
                                break;
                            case StepModes.Large:
                                break;
                            case StepModes.Medium:
                                ZStepMode = StepModes.Large;
                                break;
                            case StepModes.Small:
                                ZStepMode = StepModes.Medium;
                                break;
                            case StepModes.Micro:
                                ZStepMode = StepModes.Small;
                                break;
                        }
                    }
                    else
                    {
                        switch (XYStepMode)
                        {
                            case StepModes.XLarge:
                                break;
                            case StepModes.Large:
                                XYStepMode = StepModes.XLarge;
                                break;
                            case StepModes.Medium:
                                XYStepMode = StepModes.Large;
                                break;
                            case StepModes.Small:
                                XYStepMode = StepModes.Medium;
                                break;
                            case StepModes.Micro:
                                XYStepMode = StepModes.Small;
                                break;
                        }
                    }
                    break;
                case WindowsKey.PageDown:
                    if (isControl)
                    {
                        switch (ZStepMode)
                        {
                            case StepModes.XLarge:
                                ZStepMode = StepModes.Large;
                                break;
                            case StepModes.Large:
                                ZStepMode = StepModes.Medium;
                                break;
                            case StepModes.Medium:
                                ZStepMode = StepModes.Small;
                                break;
                            case StepModes.Small:
                                ZStepMode = StepModes.Micro;

                                break;
                            case StepModes.Micro:
                                break;
                        };
                    }
                    else
                    {
                        switch (XYStepMode)
                        {
                            case StepModes.XLarge:
                                XYStepMode = StepModes.Large;
                                break;
                            case StepModes.Large:
                                XYStepMode = StepModes.Medium;
                                break;
                            case StepModes.Medium:
                                XYStepMode = StepModes.Small;
                                break;
                            case StepModes.Small:

                                XYStepMode = StepModes.Micro;

                                break;
                            case StepModes.Micro:
                                break;
                        };
                    }
                    break;
            }
        }
    }
}