// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f7afddd932def9547e46ed7a2eec56635fe0f0bde2fad2e05352be8c4df16e34
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class NozzleTipCalibrationViewModel : ViewModelBase, INozzleTipCalibrationViewModel
    {
        private Dictionary<int, Point2D<double>> _nozzleCalibration;
  //      private List<Point2D<double>> _averagePoints;
        private readonly IMachineRepo _machineRepo;
        private readonly ILocatorViewModel _locatorViewModel;
//        private int _samplesAtPoint = 0;

        public NozzleTipCalibrationViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel)
        {
            _machineRepo = machineRepo; ;
            _locatorViewModel = locatorViewModel;

            CalibrateBottomCameraCommand = new RelayCommand(async () => await StartAsync());
        }


        public Task StartAsync()
        {
            _nozzleCalibration = new Dictionary<int, Point2D<double>>();
    //        _averagePoints = new List<Point2D<double>>();

            //if (_machineRepo.CurrentMachine.Settings.PartInspectionCamera?.AbsolutePosition != null)
            //{
            //    _nozzleCalibration = new Dictionary<int, Point2D<double>>();
            //    _machineRepo.CurrentMachine.SendSafeMoveHeight();

            //    // move to what we think is the top angle.
            //    _machineRepo.CurrentMachine.SendCommand($"G0 E0");
            //    await _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.GCodeOperationTool);


            //    _machineRepo.CurrentMachine.SendCommand($"G0 X{_machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{_machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.Y} Z{_machineRepo.CurrentMachine.Settings.PartInspectionCamera.FocusHeight} F1{_machineRepo.CurrentMachine.Settings.FastFeedRate}");
            //}

            _locatorViewModel.SetLocatorState(MVLocatorState.NozzleCalibration);

            return Task.CompletedTask;
        }

        public void CircleLocation(Point2D<double> center, double diameter, Point2D<double> stdDeviation)
        {
            Debug.WriteLine($"Found Circle: {_machineRepo.CurrentMachine.MachinePosition.X},{_machineRepo.CurrentMachine.MachinePosition.Y} - {stdDeviation.X},{stdDeviation.Y}");

            //if (_targetAngle == Convert.ToInt32(_machineRepo.CurrentMachine.Tool2))
            //{
            //    _samplesAtPoint++;

            //    if (_samplesAtPoint > 1)
            //    {
            //        Status = $"Calibrate Nozzle {_targetAngle}";

            //        var avgX = _averagePoints.Average(pt => pt.X);
            //        var avgY = _averagePoints.Average(pt => pt.Y);
            //        _nozzleCalibration.Add(Convert.ToInt32(_machineRepo.CurrentMachine.Tool2), new Point2D<double>(avgX, avgY));

            //        CalibrationUpdates.Insert(0, $"Angle: {_targetAngle} - {avgX}x{avgY}");

            //        _targetAngle = Convert.ToInt32(_machineRepo.CurrentMachine.Tool2 + 30.0);
            //        _machineRepo.CurrentMachine.SendCommand($"G0 E{_targetAngle} F5000");
            //        _averagePoints.Clear();
            //        _samplesAtPoint = 0;
            //        RaisePropertyChanged(nameof(TargetAngle));

            //    }
            //    else
            //    {
            //        _averagePoints.Add(new Point2D<double>(center.X, center.Y));
            //    }

            //    if (_machineRepo.CurrentMachine.Tool2 >= 360)
            //    {
            //        FinalizeCameraCalibration();
            //    }
            //}
        }

        private void FinalizeCameraCalibration()
        {
            _locatorViewModel.SetLocatorState(MVLocatorState.Idle);
            foreach (var key in _nozzleCalibration.Keys)
            {
                Debug.WriteLine($"{key},{_nozzleCalibration[key].X},{_nozzleCalibration[key].Y}");
            }

            var maxX = _nozzleCalibration.Values.Max(ca => ca.X);
            var maxY = _nozzleCalibration.Values.Max(ca => ca.Y);

            var minX = _nozzleCalibration.Values.Min(ca => ca.X);
            var minY = _nozzleCalibration.Values.Min(ca => ca.Y);

            var preCalX = _machineRepo.CurrentMachine.MachinePosition.X;
            var preCalY = _machineRepo.CurrentMachine.MachinePosition.Y;


            var top = _nozzleCalibration.First(pt => pt.Value.Y == maxY);

            var left = _nozzleCalibration.First(pt => pt.Value.X == minX);
            var right = _nozzleCalibration.First(pt => pt.Value.X == maxX);

            var topAngle = (left.Key + right.Key) / 2 - 180;

            var offsetX = top.Value.X / 20.0;
            var offsetY = top.Value.Y / 20.0;

            //var offsetX = ((maxX - minX) / 60.0);
            //var offsetY = ((maxY - minY) / 60.0);

            _machineRepo.CurrentMachine.SendCommand("G91");
            _machineRepo.CurrentMachine.SendCommand($"G0 X{-offsetX} Y{-offsetY}");
            _machineRepo.CurrentMachine.SendCommand("G90");

            CalibrationUpdates.Insert(0, $"Top found at: {topAngle}");
            CalibrationUpdates.Insert(0, $"Setting Offset: {-offsetX},{-offsetY}");

            Debug.WriteLine($"MIN: {minX},{minY} MAX: {maxX},{maxY}, Adjusting to offset: {offsetX},{offsetY} - Top Angle: {topAngle}");

            _machineRepo.CurrentMachine.SendCommand($"G0 E{topAngle}");
            _machineRepo.CurrentMachine.SendCommand($"G92 E0 X{preCalX} Y{preCalY}");

            Status = "Nozzle Calibration Completed";
        }

        public ObservableCollection<string> CalibrationUpdates { get; } = new ObservableCollection<string>();

        private int _targetAngle;
        public int TargetAngle
        {
            get => _targetAngle;
            set => Set(ref _targetAngle, value);
        }


        int _currentCalibrationAngle;
        public int CurrentCalibrationAngle
        {
            get => _currentCalibrationAngle;
            set => Set(ref _currentCalibrationAngle, value);
        }

        private string _status;
        public string Status
        {
            set => Set(ref _status, value);
            get => _status;
        }

        public RelayCommand CalibrateBottomCameraCommand { get; private set; }

    }
}