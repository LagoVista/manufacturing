# T013 — Supervised Vacuum Calibration (both heads)

**Source:** `pnp-vision-machine-calibration` workstream; Task T013 `standalone-vacuum-calibration-operator-validation`. Based on the accepted T007 pump/pressure sensing, two-head calibration and machine-save path.

**Status:** Ready for local Windows WPF compile and supervised hardware acceptance; do not finalize or integrate T013 until the operator confirms safe Head 1 and Head 2 behavior.

## What the window does

Machine → **Open Vacuum Calibration** opens a **nonmodal**, fixed-size dark procedure. Opening it neither moves the machine nor energizes any vacuum pump. Nonmodal mode is intentional so the operator can inspect the hardware and use the existing machine control/jog UI.

- The left `CalibrationStepRail` auto-follows the authoritative T007 `MachineOperationSession` states (queued / running / passed / failed), not simulated completion.
- The right evidence column shows Head 1/2 last **accepted** six-reading sensor windows and sample timestamps, raw sensor windows, sensor-derived calibration candidates, and commanded left/right pump states. Commanded outputs are **never** reported as sensor proof. Pump-off idle is a fresh empirical noise reference; pump-on and sealed stages must separately pass the existing stability and separation limits.
- Each head runs idle vented → pump on/no-part **before positioning** → safe camera/tool travel to configured test XY → configured PickHeight (after explicit Start and verified motion preflight) → automatic stable six-sample sealed vacuum reading and sensor-validated part-present candidate → safe Z retract, pump off and idle recovery. In **Guided Step** mode only, the operator explicitly confirms physical contact before the sealed sample.
- **Run Both Heads restores the original T007 automatic flow**: after each head reaches verified configured PickHeight, it immediately acquires a fresh sealed-pressure sensor window. It does **not** pause for a manual contact checkbox or claim the operator observed contact. The unchanged vacuum candidate classifier must independently prove part-present through stable samples and adequate open/sealed separation, or the run safely faults/retracts. `Next Guided Step` remains available for step-by-step operation with explicit per-head contact confirmation.
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

## Right head pump-on settling and diagnostics

On a supervised physical run, Head 1 completed its optical travel and vacuum checks, then the RIGHT pump-off idle baseline passed, but Head 2 **pump-on/open-nozzle** sampling measured -83,335 to -72,772 raw counts (10,563 spread) against an unchanged permitted spread of about 6,415. Head 2 did not move in XY or Z because the runner deliberately proves its open-nozzle pump response **before** starting any right-head positioning. This was neither a coordinate-frame failure nor an unstable idle baseline.

The controller's M400 acknowledgment confirms the pump command, not pressure stabilization. The T013 follow-up lets pump-on pressure physically settle for 1.2 seconds (instead of 450 ms), with at most **five** fresh independent six-sample windows rather than three and a bounded 750 ms pause between rejected pump-on windows. Every window still uses the **original** `AllowedRawSpread(idle, mean)` acceptance formula (8% of observed pump response, with measured idle noise as a floor), the same `RequiredResponse` check and opposite-head isolation. No unstable sample is accepted, no XY/Z motion starts if pump-on evidence fails, and failure/abort retains pump-off cleanup. All other sampling stages preserve their original three-window budgets.

The UI now reports **Head N pump-on/open nozzle** as the actual failing stage, with per-window raw readings, span, current sampling attempt and exact substage. Do not increase tolerances or save a head profile on a noisy pump response; inspect actual nozzle openness, pump plumbing, electrical noise, and sensor-channel readings if the right channel never stabilizes.

## Hardware limitation

The existing Marlin firmware path has individually commanded pump outputs, but its legacy solenoid actuation is not independently addressable for each head. T013 therefore **does not claim that individual solenoid isolation has been proven**. Genuine proof of independent valves would require a corresponding verified firmware/hardware contract; no new M-codes or guessed thresholds have been added.

The existing vacuum-pressure classification, raw stability/freshness, per-head separation and opposite-channel drift gates remain authoritative. This does not alter T003 camera-to-tool geometry or any production pickup/placement workflow.

## Technical verification

- Pure tests: Run Both can acquire sealed-pressure samples without a contact checkbox; Guided Step requires head-specific proof; invalid head cannot sample; machine-operation status projection and the original T007 pressure-stability/candidate tests remain authoritative. The runner still enforces connected/homed/manual/origin/XY/safe-Z/pickheight motion guards.
- Build server: canonical .NET test suite and deterministic core build.
- Windows WPF compiler and physical machine test must be performed by the operator. A core build is **not** proof that the WPF application compiled on Windows.
- After hardware acceptance: finalize T013 in Forge, review, accept, verify source in `workstream/pnp-vision-machine-calibration`, and only then begin the next task.
