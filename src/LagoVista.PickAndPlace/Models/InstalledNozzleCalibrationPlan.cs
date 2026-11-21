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
        /// A single explicit operator Start performs all installed heads in
        /// deterministic Head 1 -> Head 2 order. Reject duplicate hardware
        /// assignments rather than silently using an arbitrary second head.
        /// </summary>
        public static bool TryCreateSequentialPlan(
            LagoVista.Manufacturing.Models.Machine machine,
            out IReadOnlyList<MachineToolHead> sequence,
            out string reason)
        {
            var installed = InstalledHeads(machine);
            sequence = new List<MachineToolHead>();
            if (installed.Count == 0)
            {
                reason = "No installed nozzle assigned to Head 1 or Head 2.";
                return false;
            }
            if (installed.GroupBy(head => head.HeadIndex).Any(group => group.Count() != 1))
            {
                reason = "Two configured tool heads have the same HeadIndex; fix machine configuration before runout.";
                return false;
            }
            sequence = installed;
            reason = null;
            return true;
        }

        /// <summary>
        /// Revalidate the original nozzle/physical head before starting each
        /// next head; a nozzle swap during Head 1 must not authorize movement
        /// or measurement of a changed Head 2 installation.
        /// </summary>
        public static bool MatchesPlannedInstallation(
            LagoVista.Manufacturing.Models.Machine machine,
            MachineToolHead plannedHead, string nozzleId)
        {
            return plannedHead != null && machine?.ToolHeads != null &&
                machine.ToolHeads.Any(head => ReferenceEquals(head, plannedHead)) &&
                !String.IsNullOrWhiteSpace(nozzleId) &&
                String.Equals(Convert.ToString(plannedHead.CurrentNozzle?.Id),
                    nozzleId, StringComparison.Ordinal);
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
