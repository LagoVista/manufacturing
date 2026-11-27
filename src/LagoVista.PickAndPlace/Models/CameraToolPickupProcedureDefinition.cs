using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Guided, per-head two-pick calibration. As with nozzle runout, this is a
    /// progress/evidence contract, NOT an executable G-code script.
    /// Concrete WPF operations must supply each camera/machine proof.
    /// </summary>
    public static class CameraToolPickupProcedureDefinition
    {
        public static MachineOperationDefinition Create()
        {
            var result = new MachineOperationDefinition
            {
                Key = "camera-tool-two-pass-pickup",
                Version = 1,
                Title = "Camera-to-Tool XY Calibration"
            };
            Add(result, "readiness", "Verify machine, cameras, tool and runout",
                "Require connected, homed Manual machine, verified fiducial/staging plates, installed nozzle and T004 runout calibration.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "known-location", "Approach configured Known Location",
                "Use Machine.KnownCalibrationPoint, safe camera navigation and existing Known Location position-camera profile. Never overwrite the operator's saved XY reference.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "first-target", "Verify and center the white calibration cylinder",
                "Find a fresh stable 7 mm cylinder circle against the black holder. Reuse existing Known Location circle vision; bounded top-camera XY jog if required, then re-observe and record actual machine XY. Reject missing or ambiguous targets.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "nozzle-reference", "Measure the empty nozzle reference",
                "At the bottom camera, acquire a fresh stable empty-nozzle opening at a fixed head angle before picking the cylinder. Preserve its physical XY reference for both passes.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "first-approach", "Verify approximate pickup motion",
                "Check head selection, physical XY travel, safe Z and configured pickup height before moving.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "first-pick", "Pick plate with approximate head offset",
                "Use production vacuum on, configured pick Z and vacuum-sensor part-present confirmation.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "first-inspect", "Measure plate relative to calibrated nozzle",
                "At known inspection height, use fresh stabilized bottom-camera vision and nozzle-runout axis.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "candidate", "Calculate unsaved XY correction",
                "Bound plate-minus-nozzle error in machine coordinates; do not change the persisted offset.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "replace-part", "Return the cylinder to its holder",
                "Use bounded motion, configured placement Z, verified release and safe retract.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "second-target", "Re-observe the returned cylinder",
                "Return to saved Known Location, reacquire the 7 mm cylinder with the same Known Location profile, center with a bounded verified jog if necessary; stop if fixture moved or target changed.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "second-approach", "Apply candidate only as a bounded XY jog",
                "Keep the previous stored tool offset while independently checking physical travel limits.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "second-pick", "Perform independent verification cylinder pickup",
                "Repeat pickup with candidate jog and require fresh vacuum-sensor confirmation.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "second-inspect", "Re-measure held plate with bottom camera",
                "Require fresh, stable, nozzle-referenced optical evidence on the same head/nozzle.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "verify", "Require residual XY within tolerance",
                "Accept only if second-pick radial error is within the verified calibration tolerance.",
                MachineOperationVerificationKind.VisionVerified);
            Add(result, "safe-camera-mode", "Return to safe height and camera navigation",
                "Raise to configured safe Z, return to camera mode using the ORIGINAL head offset, and verify controller state.",
                MachineOperationVerificationKind.MachineObserved);
            Add(result, "commit-offset", "Save verified tool offset only when auto-save is enabled",
                "Only now persist MachineToolHead.Offset and verify an authoritative saved receipt.",
                MachineOperationVerificationKind.MachineObserved, requiresSavedEvidence:true);
            return result;
        }

        private static void Add(MachineOperationDefinition definition,
            string key, string title, string instructions,
            MachineOperationVerificationKind verification, bool requiresSavedEvidence = false)
        {
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key = key,
                Title = title,
                Instructions = instructions,
                RequiredVerification = verification,
                RequiresSavedEvidence = requiresSavedEvidence
            });
        }
    }
}
