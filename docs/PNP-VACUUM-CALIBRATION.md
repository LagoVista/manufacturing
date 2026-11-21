# T013 — Supervised Vacuum Calibration (both heads)

**Source:** `pnp-vision-machine-calibration` workstream; Task T013 `standalone-vacuum-calibration-operator-validation`. Based on the accepted T007 pump/pressure sensing, two-head calibration and machine-save path.

**Status:** Ready for local Windows WPF compile and supervised hardware acceptance; do not finalize or integrate T013 until the operator confirms safe Head 1 and Head 2 behavior.

## What the window does

Machine → **Open Vacuum Calibration** opens a **nonmodal**, fixed-size dark procedure. Opening it neither moves the machine nor energizes any vacuum pump. Nonmodal mode is intentional so the operator can inspect the hardware and use the existing machine control/jog UI.

- The left `CalibrationStepRail` auto-follows the authoritative T007 `MachineOperationSession` states (queued / running / passed / failed), not simulated completion.
- The right evidence column shows Head 1/2 last **accepted**, stable, six-reading sensor windows and their sample timestamps, plus the raw sample window, sensor-derived calibration candidates, and commanded left/right pump output status. Commanded outputs are **never** reported as sensor proof.
- Each head still runs idle vented → pump on/no-part → safe camera/tool travel to configured test XY → operator-approved configured PickHeight → independently operator-confirmed obstruction and sensor-verified part-present → safe Z retract, pump off and idle recovery.
- The single `Run Both Heads` button resumes the automatic portions of the T007 runner, but **pauses before the configured PickHeight descent and before sealed-pressure capture for EACH head**. The operator must explicitly confirm each checkpoint for the current physical head. This is not an unattended vacuum contact operation. `Next Guided Step` is available for an entirely stepwise run.
- The safety clearance for Head 1 **never** authorizes Head 2. An overall machine acknowledgement also cannot authorize nozzle descent.
- An unsuccessful or cancelled step stops the commanded pumps and attempts the existing verified safe-height cleanup. Use the hardware emergency stop if physical clearance is uncertain or shutdown cannot be verified. Closing is disabled while a pump is commanded on or the procedure has not safely settled.

## Supervised physical test

1. Confirm machine homed, Manual and connected, origin verified this session, safe Z known, configured MachineFiducial within XY work area, both heads/nozzles installed and both PickHeights deliberately verified. Check real fixture clearance and keep hardware emergency stop accessible.
2. Open the vacuum window. Verify no motion/pump activation on open and the rail has only queued steps. Approve the initial machine/fixture acknowledgement.
3. Click **Run Both Heads (pause for contact)**. Observe Head 1 pump-off idle sample, open-nozzle pump response and opposite-head isolation, then safe travel to configured XY.
4. At Head 1's **DESCENT APPROVAL**, physically inspect alignment and clearance; check approval and click **Resume Both-Head Procedure**. After the machine reaches configured PickHeight, it **pauses again**. Confirm a lightly obstructed nozzle and no collision, check **SEALED-NOZZLE CHECK**, and Resume.
5. Observe fresh stable sealed/no-part separation, safe Z retract, pumps off and return-to-idle evidence. The procedure automatically transitions to Head 2 only after Head 1 is safely released.
6. Repeat independent descent and contact confirmations for Head 2. Confirm the runner finishes with both pumps off at safe Z and both Head 1/2 measured candidates displayed.
7. Review measured head-specific idle/no-part/sealed responses and separation. Click **Apply Verified Calibration to Machine** only after approving both candidates; this writes existing per-head MachineToolHead fields through the existing MachineRepo.SaveCurrentMachineAsync API path. If the save fails, the previous local settings are restored. Without Apply, nothing is saved.

## Hardware limitation

The existing Marlin firmware path has individually commanded pump outputs, but its legacy solenoid actuation is not independently addressable for each head. T013 therefore **does not claim that individual solenoid isolation has been proven**. Genuine proof of independent valves would require a corresponding verified firmware/hardware contract; no new M-codes or guessed thresholds have been added.

The existing vacuum-pressure classification, raw stability/freshness, per-head separation and opposite-channel drift gates remain authoritative. This does not alter T003 camera-to-tool geometry or any production pickup/placement workflow.

## Technical verification

- Pure tests: per-head descent approval and obstruction confirmation; authoritative machine-operation status projection; original T007 vacuum sensor and calibration candidate tests.
- Build server: canonical .NET test suite and deterministic core build.
- Windows WPF compiler and physical machine test must be performed by the operator. A core build is **not** proof that the WPF application compiled on Windows.
- After hardware acceptance: finalize T013 in Forge, review, accept, verify source in `workstream/pnp-vision-machine-calibration`, and only then begin the next task.
