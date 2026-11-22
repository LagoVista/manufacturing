# T014 — Saved camera-to-tool offset / independent third-pickup acceptance

**Status:** Implemented in T014 task workspace; Windows WPF and live hardware acceptance are pending.

## Why there are three pickups

1. **First pickup, old offset:** The top camera locates the fixed white disc. The selected nozzle picks with its existing stored camera-to-head offset. The bottom camera observes *disc center minus independently measured nozzle center*; this proposes a static XY correction.
2. **Second pickup, temporary correction:** The machine returns the disc, locates it again, and applies the correction as a **guarded relative jog** while the old stored offset remains unchanged. Fresh pickup and bottom-camera measurements verify the proposed correction with the operator-approved 0.200 mm radial acceptance gate. The 16-step run ends with `commit-offset` intentionally **Unsaved / Blocked** and uploads a historical run receipt.
3. **Third pickup, actual saved offset:** Only after successful two-pick history upload, an operator clicks **Approve, Save & Verify Third Pickup** and separately confirms the real-machine movement/configuration write in a warning dialog. The candidate expires after 15 minutes and is rejected if the machine/head/nozzle/camera/old offset or required physical readiness has changed. The app persists the new `MachineToolHead.Offset` through the existing Manufacturing machine PUT and **reads it back from Manufacturing before moving**. It re-measures the empty nozzle, re-locates the returned disc, then uses normal `MoveHeadToCylinderAsync(target, null)` / `PickCylinderAsync(target, null)`: **no temporary correction jog**. Fresh vacuum and bottom-camera optical proof must place the disc **within 0.200 mm of the bottom camera's optical center**, with the nozzle within 0.120 mm of that center and the disc-to-nozzle offset within 0.200 mm. After proof it returns the disc safely and restores camera mode. This is a separate 11-stage history run, normally `Completed`.

## Optical acceptance and the green ring

- The green ring is not a generic “disc found” marker. The ordinary `calibrationdisc` detector displays yellow (regardless of whether the disc happens to be near the *camera* image center).
- The accepted third-pickup WPF overlay positions the measured disc ring and a separate nozzle-axis reference in actual bottom-camera pixels. Only an independently **verified saved-offset** pickup with disc **centered in the IMAGE** within 0.200 mm, nozzle centered within 0.120 mm, and disc centered relative to the nozzle within 0.200 mm may turn **green**. A measured out-of-tolerance result is red.
- The frame remains focused and stationary for 1.5 seconds for the acceptance ring, then returns to safe common Z before disc return. The persistent result text/badge also displays acceptance after the image changes.
- Both the second-pick temporary-correction verification and the third-pick saved-offset acceptance use the operator's **0.200 mm maximum radial disc-to-nozzle error**. Third-pass acceptance **also** requires disc-to-camera center <= 0.200 mm and nozzle-to-camera center <= 0.120 mm. The detector optical radius/circularity/ROI gates and the pickup's saved-offset motion are unchanged.

## Bottom-camera optical framing (inspection motion only)

The fixed machine bottom-camera target may NOT place the selected nozzle at the image center after camera-to-tool calibration. For example, the verified Head 1 saved-offset pickup on November 22 showed **disc-to-nozzle 0.017 mm**, yet both objects were **~1.78 mm off the image center**. That is an excellent pickup but not the simple visual alignment the operator requested.

Before each run's first pickup, T014 now measures the empty nozzle in the bottom camera, performs small supervised **0.8 mm X and Y probe moves at safe common Z** (unless already centered), and uses the fresh nozzle observations to solve the real 2D camera-to-machine XY response. The response is measured rather than assuming the camera's rotation, sign or orientation. A **maximum 4.0 mm** temporary correction is allowed, only if the measured response is well-conditioned and all physical XY targets remain within 5 mm of machine travel limits. It moves to the computed inspection-only XY coordinate, reacquires a fresh nozzle center and requires nozzle-camera centering within **0.120 mm**. Failure stops calibration and retracts to safe Z before any pickup. The same session-only inspection correction is applied whenever T014 later carries a disc to the bottom camera. The third saved-offset pickup recalculates its own inspection framing after saving a new head offset.

**The inspection framing translation is never written to MachineToolHead.Offset or MachineCamera.AbsolutePosition.** It does not alter the pickup approach or apply T004's rotational eccentricity curve; it exclusively aligns the nozzle's actual optical center with the bottom-camera image center. T004's logical angle zero is a separate concern and is not claimed to be a newly implemented physical rotation correction.

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

Historical two-pick candidates are *not* proof that a saved-offset third pickup passed. Separately, Head 1's later saved-offset third-pick run `71609B2718D547018D6B5122ADD8F29C` passed with 0.017 mm disc-to-nozzle error, and the saved Head 1 XY is (23.4471, 65.3247) mm. That older run did **not** include the newly added camera-image-center verification. Head 2's stored offset remains unchanged until operator approval and a fresh supervised pass.
