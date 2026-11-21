# T013 — Supervised Vacuum Calibration (both heads)

**Source:** `pnp-vision-machine-calibration` workstream; Task T013 `standalone-vacuum-calibration-operator-validation`. Based on the accepted T007 pump/pressure sensing, two-head calibration and machine-save path.

**Status:** Ready for local Windows WPF compile and supervised hardware acceptance; do not finalize or integrate T013 until the operator confirms safe Head 1 and Head 2 behavior.

## What the window does

Machine → **Open Vacuum Calibration** opens a **nonmodal**, fixed-size dark procedure. Opening it neither moves the machine nor energizes any vacuum pump. Nonmodal mode is intentional so the operator can inspect the hardware and use the existing machine control/jog UI.

- The left `CalibrationStepRail` auto-follows the authoritative T007 `MachineOperationSession` states (queued / running / passed / failed), not simulated completion.
- The right evidence column shows Head 1/2 last **accepted**, stable, six-reading sensor windows and their sample timestamps, plus the raw sample window, sensor-derived calibration candidates, and commanded left/right pump output status. Commanded outputs are **never** reported as sensor proof.
- Each head still runs idle vented → pump on/no-part → safe camera/tool travel to configured test XY → configured PickHeight (using the initial fixture/safety acknowledgment and the existing safe-motion preflight) → independently operator-confirmed obstruction and sensor-verified part-present → safe Z retract, pump off and idle recovery.
- The single `Run Both Heads` button uses the operator's initial machine/fixture/PickHeight acknowledgment, then the existing XY, safe-Z, head and bounded PickHeight preflights to move through configured travel without an additional mid-run descent checkbox. It **pauses only before accepting sealed-pressure evidence for EACH head**. Operator-confirmed physical nozzle obstruction is still required and cannot be reused across heads. `Next Guided Step` remains available for manual step-by-step execution.
- The initial machine acknowledgment covers the configured travel/PickHeight for the supervised two-head run, but does not replace either head's independent post-descent contact confirmation. Each head still gets fresh preflight and sensor proof before a part-present candidate is accepted.
- An unsuccessful or cancelled step stops the commanded pumps and attempts the existing verified safe-height cleanup. Use the hardware emergency stop if physical clearance is uncertain or shutdown cannot be verified. Closing is disabled while a pump is commanded on or the procedure has not safely settled.

## Supervised physical test

1. Confirm machine homed, Manual and connected, origin verified this session, safe Z known, configured MachineFiducial within XY work area, both heads/nozzles installed and both PickHeights deliberately verified. Check real fixture clearance and keep hardware emergency stop accessible.
2. Open the vacuum window. Verify no motion/pump activation on open and the rail has only queued steps. Approve the initial machine/fixture acknowledgement.
3. Click **Run Both Heads (pause for contact)**. Observe Head 1 pump-off idle sample, open-nozzle pump response and opposite-head isolation, then safe travel to configured XY.
4. After Head 1's tested safe travel, the runner automatically performs the previously configured PickHeight move; there is no redundant descent approval checkbox. It then pauses at **SEALED-NOZZLE CHECK**. Physically confirm a lightly obstructed nozzle and no collision, check contact confirmation, then Resume. The stable sensor band must still independently verify a sealed/part-present classification.
5. Observe fresh stable sealed/no-part separation, safe Z retract, pumps off and return-to-idle evidence. The procedure automatically transitions to Head 2 only after Head 1 is safely released.
6. Head 2 repeats the safety/motion checks and its configured descent automatically. Repeat the independent **sealed-contact confirmation** for Head 2. Confirm the runner finishes with both pumps off at safe Z and both measured candidates displayed.
7. Review measured head-specific idle/no-part/sealed responses and separation. Click **Apply Verified Calibration to Machine** only after approving both candidates; this writes existing per-head MachineToolHead fields through the existing MachineRepo.SaveCurrentMachineAsync API path. If the save fails, the previous local settings are restored. Without Apply, nothing is saved.

## Hardware limitation

The existing Marlin firmware path has individually commanded pump outputs, but its legacy solenoid actuation is not independently addressable for each head. T013 therefore **does not claim that individual solenoid isolation has been proven**. Genuine proof of independent valves would require a corresponding verified firmware/hardware contract; no new M-codes or guessed thresholds have been added.

The existing vacuum-pressure classification, raw stability/freshness, per-head separation and opposite-channel drift gates remain authoritative. This does not alter T003 camera-to-tool geometry or any production pickup/placement workflow.

## Technical verification

- Pure tests: initial supervised fixture acknowledgment authorizes only configured, verified descent; per-head obstruction confirmation is not transferable; authoritative machine-operation status projection; original T007 vacuum sensor and calibration candidate tests.
- Build server: canonical .NET test suite and deterministic core build.
- Windows WPF compiler and physical machine test must be performed by the operator. A core build is **not** proof that the WPF application compiled on Windows.
- After hardware acceptance: finalize T013 in Forge, review, accept, verify source in `workstream/pnp-vision-machine-calibration`, and only then begin the next task.
