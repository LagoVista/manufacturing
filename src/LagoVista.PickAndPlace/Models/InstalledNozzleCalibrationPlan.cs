using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Discover nozzles assigned to actual machine tool heads, not nozzle
    /// rack entries or the currently selected machine navigation tool.
    /// These assignments describe the physical installation in machine
    /// settings. Navigation selection is independent.
    /// </summary>
    public static class InstalledNozzleCalibrationPlan
    {
        public static IReadOnlyList<MachineToolHead> InstalledHeads(LagoVista.Manufacturing.Models.Machine machine)
        {
            return machine?.ToolHeads == null
                ? new List<MachineToolHead>()
                : machine.ToolHeads
                    .Where(head => head != null &&
                        (head.HeadIndex == 1 || head.HeadIndex == 2) &&
                        head.CurrentNozzle != null &&
                        !String.IsNullOrWhiteSpace(Convert.ToString(head.CurrentNozzle.Id)))
                    .OrderBy(head => head.HeadIndex)
                    .ToList();
        }

        /// <summary>
        /// Turn an operator-selected installed head into a one-head plan.
        /// A stale/disconnected selection must never cause a fallback sweep
        /// of all heads or of an arbitrary other head.
        /// </summary>
        public static IReadOnlyList<MachineToolHead> SelectOne(
            IReadOnlyList<MachineToolHead> installed, MachineToolHead selected)
        {
            return installed != null && selected != null &&
                installed.Any(head => ReferenceEquals(head, selected))
                ? new List<MachineToolHead> { selected }
                : new List<MachineToolHead>();
        }

        public static MachineToolHead ChooseFirst(
            IReadOnlyList<MachineToolHead> installed, MachineToolHead active)
        {
            if (installed == null || installed.Count == 0) return null;
            // Respect an already selected and installed head; otherwise
            // choose the first configured installed nozzle deterministically.
            return installed.FirstOrDefault(head =>
                       ReferenceEquals(head, active) ||
                       (active != null && head.Id.Equals(active.Id)))
                   ?? installed[0];
        }
    }
}
