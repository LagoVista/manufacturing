using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Preflight planning only. All installed component rows are enumerated
    /// before any motion. No row or inventory record is modified by this plan.
    /// </summary>
    public static class FeederCalibrationFlowPlan
    {
        public static List<StripFeederRow> InstalledRows(StripFeeder feeder)
        {
            if (feeder == null)
                throw new ArgumentNullException(nameof(feeder));
            return (feeder.Rows ?? new List<StripFeederRow>())
                .Where(row => row != null && row.Component != null &&
                    !String.IsNullOrWhiteSpace(row.Component.Id.ToString()) &&
                    row.Status?.Value != StripFeederRowStatuses.Empty)
                .OrderBy(row => row.RowIndex)
                .ToList();
        }

        public static List<FeederCalibrationFlowStep> Steps(StripFeeder feeder)
        {
            var items = new List<FeederCalibrationFlowStep>();
            void Add(string title, string key, int? rowIndex = null) =>
                items.Add(new FeederCalibrationFlowStep {
                    Number = items.Count + 1, Title = title,
                    StepKey = key, RowIndex = rowIndex
                });

            Add("Move to calculated feeder origin", "approach-origin");
            Add("Find and save feeder origin", "center-fiducial");
            Add("Read barcode and confirm feeder ID", "barcode");

            foreach (var row in InstalledRows(feeder))
            {
                var prefix = $"Tape {row.RowIndex}";
                Add(prefix + " · Top tape hole", "first-tape-hole", row.RowIndex);
                Add(prefix + " · Bottom tape hole", "last-tape-hole", row.RowIndex);
                Add(prefix + " · Current component present", "verify-current-part", row.RowIndex);
                Add(prefix + " · Previous pocket empty", "verify-previous-empty", row.RowIndex);
            }

            return items;
        }
    }
}
