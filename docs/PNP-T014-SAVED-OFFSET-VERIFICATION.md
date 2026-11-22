# T014 — Saved camera-to-tool offset / independent third-pickup acceptance

**Status:** Implemented in T014 task workspace; Windows WPF and live hardware acceptance are pending.

## Why there are three pickups

1. **First pickup, old offset:** The top camera locates the fixed white disc. The selected nozzle picks with its existing stored camera-to-head offset. The bottom camera observes *disc center minus independently measured nozzle center*; this proposes a static XY correction.
2. **Second pickup, temporary correction:** The machine returns the disc, locates it again, and applies the correction as a **guarded relative jog** while the old stored offset remains unchanged. Fresh pickup and bottom-camera measurements verify the proposed correction with the existing 0.120 mm acceptance gate. The 16-step run ends with `commit-offset` intentionally **Unsaved / Blocked** and uploads a historical run receipt.
3. **Third pickup, actual saved offset:** Only after successful two-pick history upload, an operator clicks **Approve, Save & Verify Third Pickup** and separately confirms the real-machine movement/configuration write in a warning dialog. The candidate expires after 15 minutes and is rejected if the machine/head/nozzle/camera/old offset or required physical readiness has changed. The app persists the new `MachineToolHead.Offset` through the existing Manufacturing machine PUT and **reads it back from Manufacturing before moving**. It re-measures the empty nozzle, re-locates the returned disc, then uses normal `MoveHeadToCylinderAsync(target, null)` / `PickCylinderAsync(target, null)`: **no temporary correction jog**. Fresh vacuum and bottom-camera optical proof must place the disc center within **0.200 mm radial distance from the nozzle axis**. After proof it returns the disc safely and restores camera mode. This is a separate 11-stage history run, normally `Completed`.

## Optical acceptance and the green ring

- The green ring is not a generic “disc found” marker. The ordinary `calibrationdisc` detector displays yellow (regardless of whether the disc happens to be near the *camera* image center).
- The accepted third-pickup WPF overlay positions the measured disc ring and a separate nozzle-axis reference using their actual bottom-camera pixel offsets. Only an independently **verified saved-offset** third pickup can turn the disc ring **green**. A measured out-of-tolerance result is red.
- The frame remains focused and stationary for 1.5 seconds for the acceptance ring, then returns to safe common Z before disc return. The persistent result text/badge also displays acceptance after the image changes.
- Two-pick verification retains its stricter **0.120 mm** mathematical gate; final saved-offset acceptance uses the operator's **0.200 mm** maximum radial error, and the detector's optical radius/circularity/ROI gates are unchanged.

## Safety and failure semantics

- No automatic write during the first two picks or simply from clicking **Run All Calibrations (Except Vacuum)**.
- The batch launcher leaves each T014 window open for explicit operator approval. It advances to the next head only after the **third saved-offset pickup passed all 11 stages AND the new run history was acknowledged**.
- Failure before the saved write does not authorize motion. Failure after a saved write attempts guarded disc recovery, camera safe positioning and restoring the old offset in Manufacturing. If return or rollback cannot be verified, the operator sees a strong warning; no green result is claimed.
- History receipts capture old and proposed XY, saved write acknowledgement/readback, third actual residual, detector metrics, restoration outcome, linked two-pick Run ID, and error path. The existing local JSON backup remains available if the API is unavailable.
- Hardware emergency stop remains independent. The app's canonical Linux .NET build/tests do not compile the native Windows WPF surface or prove physical motion. A supervised Windows build and real-machine acceptance are still required.

## Observed baseline (before these changes)

Lumen Jr. observed in Manufacturing history November 22, 2026:
- Head 1: previous (22.700, 65.000) -> candidate (23.510, 65.344) mm; second pickup residual 0.115 mm, candidate **unsaved**.
- Head 2: previous (-22.900, 65.400) -> candidate (-22.987, 65.436) mm; second pickup residual 0.020 mm, candidate **unsaved**.

These historical two-pick candidates are *not* proof that a saved-offset third pickup has already passed.
