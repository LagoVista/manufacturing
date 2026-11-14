# Nozzle Rotational Runout — Optical Calibration T004

Status: **first WPF and typed-procedure integration; physical acceptance pending**.
Source workstream: `pnp-vision-machine-calibration`, T004
`nozzle-rotation-optical-calibration`. Initial branch base is the accepted
`pnp-guided-calibration-operations` feeder T005 integration commit
`1b14f172e980e6ef0dec7e9bd2dede52305e395c`.

## Goal and terminology

*Nozzle runout* or *nozzle-tip rotational eccentricity* is the angle-dependent
movement of the physical nozzle-tip center in XY when the nozzle rotates.
This is separate from the static camera-to-tool offset (future T003).
The required calibration accuracy is significantly finer than the old
`MVLocatedCircle.OffsetMM` 0.1 mm rounding.

Use the existing `NozzleTipCalibrationViewModel` as the owner of the typed
motion/vision procedure. The commented legacy sweep, hard-coded `/20`
conversion and direct `G92 E0 X... Y...` adjustment must **not** be revived.

## Operator experience

Machine Calibration → Nozzles → **Calibrate Installed Tool Heads** opens a
dedicated dark-modal progress window using the same per-step presentation
type as the feeder calibration. The modal discovers each installed nozzle
from `Machine.Settings.ToolHeads` and calibrates Head 1, then Head 2 if
both are populated. No prior navigation-tool selection is required; the
existing awaited safe tool-head navigation path is reused. Each head has
its own 15-step progress section. The modal starts only after the operator
opens it on a connected, homed machine, displaying
status, actual XY center in mm, observed pixels/mm, elapsed time, retry count,
failure detail and high-Y logical-zero candidate. **Run Again** is available
after a stop; closing the modal never masquerades as emergency stop.

`NozzleRunoutProcedureDefinition` creates 15 ordered stages using the
shared `MachineOperationDefinition` and `MachineOperationSession`:
1. Select the configured, installed nozzle on each tool head and move it over
   the configured bottom camera using the existing safe tool-head transition.
2. Rotate to each of 0, 30, 60, ..., 330 degrees; acquire new stabilized
   bottom-camera nozzle-circle observations with up to three bounded camera
   acquisition attempts per angle.
3. Choose the measured sample with maximum physical Y as the *logical*
   rotational zero, rotate to that commanded angle, and reacquire a sample.
   The first and second high-Y XY positions must agree within 0.1 mm.
4. Calculate a mean-centered twelve-point optical compensation preview and
   the maximum radial deviation and X/Y ranges. No production placement
   compensation is enabled by this procedure.

The code calculates `OffsetPixels.X / EffectivePixelsPerMM` and
`-OffsetPixels.Y / EffectivePixelsPerMM` (the current
`MVLocatedCircle` sign convention), without the legacy 0.1 mm rounding.
The camera's physical orientation / Y sign and the correct A/B rotary
axis must be verified on the actual machine before treating maximum Y
as a trusted zero reference.

A pure `NozzleRunoutMath.Correction` helper linearly interpolates across
30-degree samples including the 330→0 wrap. After a full 12-angle sweep
and verified high-Y revisit, the procedure stores the measured correction
samples and logical zero **in memory on that particular MachineToolHead**,
then marks its runtime `IsCalibrated=true`. These data are *session-only*
(`JsonIgnore`): no machine configuration save, firmware G92, or production
placement correction is performed yet.

`CurrentNozzle` assignment/replacement or explicit
`InvalidateRunoutCalibration()` clears the head's status, offsets and zero,
including a reinstalled nozzle with the same ID. Reloading settings,
connecting/reconnecting the controller, disconnecting, or homing invalidates
every head. We therefore do not reuse a curve after physical setup changes.
A physical nozzle change that does not update the model must explicitly call
`InvalidateRunoutCalibration()` as part of its tool-change workflow. Calibration movement must only start when the machine has been homed and
is safe; merely connecting to a controller never automatically starts motion.

The head's `IsCalibrated` currently means **optical runout measured and
repeatable for this installation**, not that placement compensation is
already wired into the production pick/place motion system. That integration
is a separately verified acceptance step.

## Safety hold: unexpected XY-to-limit move (2026-11-14)

An operator reported that opening the runout calibration modal immediately
jogged the gantry to the machine's maximum X and maximum Y. The procedure
then stopped at the first step reporting that the bottom-camera / Nozzle
profile / calibrated pixels-per-mm was not ready. **Do not retry machine
motion until the physical G-code log and configured coordinates have been
reviewed under supervision.**

Source inspection found an ordering defect: the procedure called
`MoveToToolHeadAsync(tool)` **before** `ValidContext` checked the camera.
This is not a virtual selection: it sends safe-Z, switches to relative
mode (`G91`), enqueues an XY move by `tool.Offset`, switches back to
absolute (`G90`), and resets machine coordinates with `G92 X/Y`.
The later `GoToPartInspectionCameraAsync` also sends a direct absolute XY
move to `MachineCamera.AbsolutePosition` with no built-in boundary
guard. The screenshot establishes an unexpected move, not its precise
commanded destination; compare actual device logs to discriminate the
relative head offset move, camera target, or another queued motion.

**Mitigation on the T004 task branch**:

- Before selecting a head or issuing any motor command, validate an active
  live Part Inspection capture stream with processed frame dimensions, the
  actual camera identity, Nozzle profile with circle detection enabled, and
  positive finite calibrated pixels/mm. Failure shows a specific blocked
  reason with *no movement sent*.
- Read the controller's current XY without issuing a move, then validate
  BOTH the proposed relative head-selection destination and the configured
  bottom-camera destination against the work area.
- Fail closed if either planned target is within 5 mm of an XY travel limit
  or would require over 85 mm automatic travel along either axis from
  the reported current XY. These are temporary commissioning guardrails,
  not the final motion planning architecture.
- Log current XY, physical head offset, bottom-camera target, and work area
  before any accepted physical move. No controller `G92` or absolute XY
  camera approach is allowed merely because the operator opened the modal
  while camera readiness has not been established.
- Focused pure motion-geometry tests cover safe moves, camera on maximum
  XY, unsafe relative tool selection, oversized automatic XY traverses,
  invalid/non-finite input.

The structural safeguards **do not prove the physical machine's coordinate
frame, camera target, rotary reference, Z clearance, or actual controller
behavior**. Windows WPF compilation and supervised bench verification are
required before resuming machine-connected automation. Do not rely on the
modal's Close button as an emergency stop.

## Motion/vision safeguards

- Requires connected, homed, idle, Manual-mode machine, an installed nozzle
  assigned to Head 1 and/or Head 2, a configured in-bounds bottom-camera location,
  running Part Inspection camera, active Nozzle profile with `FindCircles`,
  and positive calibrated pixels/mm.
- Existing `SendSafeMoveHeight`, `GoToPartInspectionCameraAsync`,
  `RotateToolHeadAngleAbsolute` (A for left, B for right), and `M400`
  are reused, with serial-idle checks. Force G90 before rotary commands.
- Existing bottom-camera detection-height command was corrected to use
  ZL or ZR according to the selected tool head, instead of always ZL.
- Camera profile, machine selection, tool head, installed nozzle, camera
  identity and pixels/mm are checked throughout the sweep.
- Each angle's detector history is reset; observations require
  `MVLocatedCircle.Stabilized` and `PositionStable`, even when the
  circle is off-center (the off-center measurement is the runout signal).
- No unexpected motion, profile mismatch, missing fresh circles or
  nonrepeatable high point can be interpreted as a successful calibration.

**Supervised physical acceptance is mandatory.** The common safe-height
command and clearance of *both* independent heads require validation on the
actual machine before the first automatic camera approach. A queued controller
move cannot be stopped by closing a modal; use the physical emergency stop.

## Outstanding acceptance

1. Build `LagoVista.PickAndPlace.App` on Windows; the canonical Forge
   NuGet build does not compile the WPF host.
2. Confirm configured installed nozzles on each head, bottom camera focus/lighting/profile,
   rotary axis A/B, and that both independent nozzle heads clear the
   camera and feeders during safe movements.
3. Execute under supervision, verify all 12 measurement rows and the
   maximum-Y revisit **for each installed head**, including camera Y orientation,
   tool change safety and the WPF per-head status indicators.
4. Repeat the sweep after machine rehome to establish repeatability.
5. Verify a nozzle swap/reinstallation clears only its owning head's
   `IsCalibrated`, verify a machine reconnect/rehome clears both heads,
   and require a new sweep before trusting a changed installation. Add
   production runout compensation separately after physical acceptance.
6. Then proceed with camera-to-tool static offset task T003.

Tests connecting to external hardware must carry `Category="Integration"`;
pure deterministic runout math/session tests run in the canonical suite.
No API, NuGet stable release, Build Server changes, or deployment is implied.
