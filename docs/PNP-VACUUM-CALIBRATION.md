# T013 — Supervised Vacuum Calibration (both heads)

**Source:** `pnp-vision-machine-calibration` workstream; Task T013 `standalone-vacuum-calibration-operator-validation`. Based on the accepted T007 pump/pressure sensing, two-head calibration and machine-save path.

**Status:** Ready for local Windows WPF compile and supervised hardware acceptance; do not finalize or integrate T013 until the operator confirms safe Head 1 and Head 2 behavior.

## What the window does

Machine → **Open Vacuum Calibration** opens a **nonmodal**, fixed-size dark procedure. Opening it neither moves the machine nor energizes any vacuum pump. Nonmodal mode is intentional so the operator can inspect the hardware and use the existing machine control/jog UI.

- The left `CalibrationStepRail` auto-follows the authoritative T007 `MachineOperationSession` states (queued / running / passed / failed), not simulated completion.
- The right evidence column shows Head 1/2 last fresh 15-reading trimmed sensor centers and timestamps, all signed raw counts, original extrema, robust noise envelopes, measured no-part/obstructed band gap, candidates and commanded pump states. Commanded outputs are **never** sensor proof. A valid sampling window alone is not proof of part presence; acceptance requires independent state separation.
- Each head runs idle vented → pump on/no-part **before positioning** → safe camera/tool travel to configured test XY → configured PickHeight (after explicit Start and verified motion preflight) → automatic 15-sample robust sealed vacuum reading and sensor-validated part-present candidate → safe Z retract, pump off and idle recovery. In **Guided Step** mode only, the operator explicitly confirms physical contact before the sealed sample.
- **Run Both Heads restores the original T007 automatic flow**: after each head reaches verified configured PickHeight, it immediately acquires a fresh sealed-pressure sensor window. It does **not** pause for a manual contact checkbox or claim the operator observed contact. The noise-aware candidate classifier must independently prove distinguishable open/sealed states with measured noise margin, or the run safely faults/retracts. `Next Guided Step` remains available for step-by-step operation with explicit per-head contact confirmation.
- Starting the procedure is a deliberate operator action; inspect the fixture and configured PickHeights before doing so. Machine preflight verifies the commanded movement, while **sensor classification** verifies automatic sealed-nozzle behavior; only Guided mode additionally requires an operator-observed contact confirmation. Neither mode can save thresholds without independently verified pressure evidence.
- An unsuccessful or cancelled step stops the commanded pumps and attempts the existing verified safe-height cleanup. Use the hardware emergency stop if physical clearance is uncertain or shutdown cannot be verified. Closing is disabled while a pump is commanded on or the procedure has not safely settled.

## Supervised physical test

1. Confirm machine homed, Manual and connected, origin verified this session, safe Z known, configured MachineFiducial within XY work area, both heads/nozzles installed and both PickHeights deliberately verified. Check real fixture clearance and keep hardware emergency stop accessible.
2. Open the vacuum window. Verify no motion/pump activation on open and the rail has only queued steps. Confirm the fixture is clear and both configured PickHeights are safe before pressing the explicit start button; there is no redundant safety checkbox.
3. Click **Run Both Heads**. Observe Head 1 pump-off idle sample, open-nozzle pump response and opposite-head isolation, then safe travel to configured XY.
4. After Head 1's verified safe travel and configured PickHeight move, **Run Both proceeds immediately to the sealed-vacuum sensor window** without a checkbox or Resume click. Watch the actual pressure evidence; an open/no-seal response must fail candidate classification and trigger existing safe cleanup. For individual guided steps, confirm physical contact before sampling.
5. Observe fresh stable sealed/no-part separation, safe Z retract, pumps off and return-to-idle evidence. The procedure automatically transitions to Head 2 only after Head 1 is safely released.
6. Head 2 repeats the motion checks and automatic sealed-pressure evidence. No contact checkbox is needed during Run Both. Confirm the procedure finishes with both pumps off at safe Z and both measured candidates displayed. Guided mode still needs a separate confirmation for Head 2.
7. Review measured head-specific idle/no-part/sealed responses and separation. Click **Apply Verified Calibration to Machine** only after approving both candidates; this writes existing per-head MachineToolHead fields through the existing MachineRepo.SaveCurrentMachineAsync API path. If the save fails, the previous local settings are restored. Without Apply, nothing is saved.

## Detector-oriented sampling and measured noise

The physical sensors are used as head-specific **part-present detectors**, not calibrated pressure gauges. Observed operator ranges are approximately: Head 1 idle 3,500..4,200, open/no-part -152k..-142k, obstructed -1M..-900k; Head 2 idle 900..1,500, open/no-part -55k..-40k, obstructed -240k..-220k raw counts. These are indicative observations, not verified saved settings.

The earlier T013 strict window rule rejected any six-reading pump-on window whose full raw range exceeded 8% of its idle-to-open response; it repeatedly failed the right head even when no-part and obstructed readings were much farther apart. The new T013 path captures **15 fresh signed readings** per state with the existing 150ms inter-sample delay. Its persisted candidate center is a trimmed mean (discarding the highest and lowest three readings); the original raw minimum/maximum are retained for diagnostics, while sorted positions 3..13 form a robust noise envelope. A large outlier is never silently used as a state center.

Before any XY/Z positioning, the open-nozzle center must differ from idle by at least the existing required response AND three times the combined idle/open noise radii; opposite-head isolation still applies. After the configured supervised motion, obstructed must lie beyond open in the same raw polarity and must have non-overlapping robust envelopes with center separation of at least four times the combined open/sealed noise radii. Otherwise the candidate is rejected and no calibration is saved. A midpoint detection threshold with existing 3% hysteresis is retained.

An optional five-reading, four-vote helper is provided for future live detector consumers; it fails to Unknown on conflicting states and is **not** wired into existing single-sample production checks by this task. Calibration still requires verified return-to-idle, safe retraction, pump shutdown and explicit Apply; all hardware preflight, motion and sensor communication checks remain intact.

This change does not modify the ESP32 firmware, Marlin transport, machine motion or saved-entity fields. A Windows WPF build and supervised physical verification of both heads are still required.

## Hardware limitation

The existing Marlin firmware path has individually commanded pump outputs, but its legacy solenoid actuation is not independently addressable for each head. T013 therefore **does not claim that individual solenoid isolation has been proven**. Genuine proof of independent valves would require a corresponding verified firmware/hardware contract; no new M-codes or guessed thresholds have been added.

The existing vacuum-pressure classification, raw stability/freshness, per-head separation and opposite-channel drift gates remain authoritative. This does not alter T003 camera-to-tool geometry or any production pickup/placement workflow.

## Technical verification

- Pure tests: Run Both can acquire sealed-pressure samples without a contact checkbox; Guided Step requires head-specific proof; invalid head cannot sample; machine-operation status projection and the original T007 pressure-stability/candidate tests remain authoritative. The runner still enforces connected/homed/manual/origin/XY/safe-Z/pickheight motion guards.
- Build server: canonical .NET test suite and deterministic core build.
- Windows WPF compiler and physical machine test must be performed by the operator. A core build is **not** proof that the WPF application compiled on Windows.
- After hardware acceptance: finalize T013 in Forge, review, accept, verify source in `workstream/pnp-vision-machine-calibration`, and only then begin the next task.
