using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Production-path pickup using an operator-approved, persisted camera-to-tool
    /// translation. Distinct from the two-pick temporary-jog proposal.
    /// </summary>
    public static class CameraToolSavedOffsetVerificationProcedure
    {
        public static MachineOperationDefinition Create()
        {
            var result = new MachineOperationDefinition
            {
                Key = "camera-tool-saved-offset-pickup-verification",
                Version = 1,
                Title = "Camera-to-Tool Saved Offset: Independent Pickup Proof"
            };
            Add(result, "readiness", MachineOperationVerificationKind.MachineObserved);
            Add(result, "save-offset", MachineOperationVerificationKind.MachineObserved, true);
            Add(result, "read-back", MachineOperationVerificationKind.MachineObserved);
            Add(result, "nozzle-reference", MachineOperationVerificationKind.VisionVerified);
            Add(result, "target", MachineOperationVerificationKind.VisionVerified);
            Add(result, "approach", MachineOperationVerificationKind.MachineObserved);
            Add(result, "pick", MachineOperationVerificationKind.MachineObserved);
            Add(result, "inspect", MachineOperationVerificationKind.VisionVerified);
            Add(result, "verify", MachineOperationVerificationKind.VisionVerified);
            Add(result, "return", MachineOperationVerificationKind.MachineObserved);
            Add(result, "safe-camera-mode", MachineOperationVerificationKind.MachineObserved);
            return result;
        }

        private static void Add(MachineOperationDefinition def, string key,
            MachineOperationVerificationKind verification, bool saved = false)
        {
            def.Steps.Add(new MachineOperationStepDefinition
            {
                Key = key,
                Title = key.Replace('-', ' '),
                RequiredVerification = verification,
                RequiresSavedEvidence = saved
            });
        }
    }

    /// <summary>Captured at the final held-disc inspection; overlay is display-only.</summary>
    public sealed class CameraToolSavedPickupProof
    {
        public double DiscOffsetXmm { get; set; }
        public double DiscOffsetYmm { get; set; }
        public double NozzleOffsetXmm { get; set; }
        public double NozzleOffsetYmm { get; set; }
        public double DiscDiameterMm { get; set; }
        public double ResidualMm { get; set; }
        public double NozzleCameraResidualMm { get; set; }
        public double DiscCameraResidualMm { get; set; }
        public bool Verified { get; set; }
    }
}
