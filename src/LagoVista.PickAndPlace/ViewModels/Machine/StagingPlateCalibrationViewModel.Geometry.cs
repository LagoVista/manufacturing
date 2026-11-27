using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public sealed partial class StagingPlateCalibrationViewModel
    {
        private bool _geometryDiagnosticMode;
        private string _geometrySummary = "Not measured";
        public bool GeometryDiagnosticMode
        {
            get => _geometryDiagnosticMode;
            set
            {
                if (!CanClose) return;
                Set(ref _geometryDiagnosticMode, value);
                ResetStages();
                RaisePropertyChanged(nameof(GeometryPlan));
            }
        }
        public string GeometrySummary
        {
            get => _geometrySummary;
            private set => Set(ref _geometrySummary, value);
        }
        public ObservableCollection<string> GeometryResults { get; } = new ObservableCollection<string>();
        public string GeometryPlan => !GeometryDiagnosticMode ? "" :
            TryGeometryGrid(SelectedPlate, out var rows, out var cols, out var error)
                ? "Inspect clear, visible holes before Run: " + string.Join(", ", rows.Select((row, i) => row + cols[i])) + " (return to first)."
                : error;

        private static bool TryGeometryGrid(MachineStagingPlate plate, out string[] rows, out int[] cols, out string error)
        {
            rows = null; cols = null;
            if (!StagingPlateReferenceGate.ValidateConfiguration(plate, out error)) return false;
            var r1 = plate.ReferenceHoleRow1.Id; var r2 = plate.ReferenceHoleRow2.Id;
            var c1 = int.Parse(plate.ReferenceHoleColumn1.Id); var c2 = int.Parse(plate.ReferenceHoleColumn2.Id);
            var cross1 = c2; var cross2 = c1;
            // Same-row parity is established by each configured physical reference.
            if (plate.HolesStaggered && (c1 & 1) != (c2 & 1))
            {
                cross1 -= Math.Sign(c2-c1); cross2 += Math.Sign(c2-c1);
            }
            if (r1 == r2 || c1 == c2 || cross1 == c1 || cross2 == c2)
            {
                error = "Geometry needs two widely separated diagonal reference holes.";
                return false;
            }
            rows = new[] { r1, r2, r1, r2, r1 };
            cols = new[] { c1, c2, cross1, cross2, c1 };
            for (var i = 0; i < rows.Length; i++)
            {
                if (cols[i] > 255 || (plate.FirstUsableColumn > 0 && cols[i] < plate.FirstUsableColumn) ||
                    (plate.LastUsableColumn > 0 && cols[i] > plate.LastUsableColumn) ||
                    (rows[i] == plate.FiducialRow.Id && cols[i].ToString() == plate.FiducialColumn.Id))
                {
                    error = "Diagnostic hole " + rows[i] + cols[i] + " is outside usable columns or is the machine fiducial.";
                    return false;
                }
            }
            error = null;
            return true;
        }

        private async Task RunGeometryDiagnosticAsync(MachineStagingPlate plate)
        {
            if (!TryGeometryGrid(plate, out var rows, out var cols, out var reason))
            {
                Fail(reason); return;
            }
            var nominal = new Point2D<double>[5];
            var measured = new Point2D<double>[5];
            var anchor = MachineConfiguration.MachineFiducial;
            for (var i = 0; i < 5; i++)
            {
                nominal[i] = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(plate, anchor,
                    plate.FiducialColumn.Id, plate.FiducialRow.Id, cols[i].ToString(), rows[i]);
                if (!StagingPlateReferenceGate.WithinWorkArea(nominal[i], Machine.Settings.WorkAreaSize))
                {
                    Fail(StagingPlateReferenceGate.DescribeTravelViolation(nominal[i], Machine.Settings.WorkAreaSize, rows[i]+cols[i]));
                    return;
                }
            }
            GeometryResults.Add(GeometryPlan);
            for (var i = 0; i < 5; i++)
            {
                var label = rows[i]+cols[i]+(i == 4 ? " (return)" : "");
                Step(i+1, "Geometry: move to and center " + label);
                var target = i == 0 ? nominal[i] : new Point2D<double>(
                    measured[0].X+nominal[i].X-nominal[0].X, measured[0].Y+nominal[i].Y-nominal[0].Y);
                GeometryResults.Add($"{label} nominal ({nominal[i].X:F3}, {nominal[i].Y:F3}); approach ({target.X:F3}, {target.Y:F3}) mm");
                if (!await MoveToReferenceAsync(target, label)) return;
                var initial = await Machine.GetCurrentLocationAsync(5000);
                if (!initial.Successful || !StagingPlateReferenceGate.WithinWorkArea(initial.Result, Machine.Settings.WorkAreaSize) ||
                    !MachineReady(out reason) || Machine.Busy)
                {
                    Fail("Could not verify initial position at " + label); return;
                }
                var initialX = initial.Result.X; var initialY = initial.Result.Y;
                var centered = await CenterAndMeasureAsync(label);
                if (centered == null) return;
                measured[i] = new Point2D<double>(centered.X, centered.Y);
                GeometryResults.Add($"{label} initial ({initialX:F3}, {initialY:F3}); centered ({centered.X:F3}, {centered.Y:F3}); " +
                    $"nominal error X={centered.X-nominal[i].X:+0.000;-0.000;0.000}, Y={centered.Y-nominal[i].Y:+0.000;-0.000;0.000}; " +
                    $"approach correction X={centered.X-initialX:+0.000;-0.000;0.000}, Y={centered.Y-initialY:+0.000;-0.000;0.000} mm");
            }
            Step(6, "Fit three points; independently verify fourth and return.");
            if (!StagingPlateGeometry.TryFit(nominal.Take(3).ToArray(), measured.Take(3).ToArray(), out var fit))
            {
                Fail("Degenerate or reversed geometry. No result accepted."); return;
            }
            var residual = StagingPlateGeometry.Distance(fit.Predict(nominal[3]), measured[3]);
            var repeat = StagingPlateGeometry.Distance(measured[0], measured[4]);
            GeometryResults.Add($"Estimated physical mm per controller mm: X={fit.PhysicalXScale:F6}; Y={fit.PhysicalYScale:F6}");
            GeometryResults.Add($"Estimated axis angle={fit.AxisAngleDegrees:F4} deg; plate X direction in controller coordinates={fit.PlateXDirectionDegrees:F4} deg");
            GeometryResults.Add($"Controller cross terms Y/plate X={fit.ControllerYPerPlateX:F6}; X/plate Y={fit.ControllerXPerPlateY:F6}");
            GeometrySummary = $"Four holes + return: independent fourth-hole residual {residual:F3} mm; return difference {repeat:F3} mm (controller space). Assumes accurate plate grid. No alignment saved.";
            FinishCurrentStage("Passed");
            Status = GeometrySummary;
            Log(Status);
        }
    }
}
