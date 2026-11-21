# Nozzle Rotational Runout — Optical Calibration T004

Status: **T004 initial runner accepted; T012 standalone operator-first UI implemented; supervised physical acceptance pending.**
Workstream: `pnp-vision-machine-calibration`, T012 `standalone-nozzle-runout-calibration-validation`.
T004 `nozzle-rotation-optical-calibration` remains closed as historical
algorithm/typed-procedure implementation. T012 modifies its launcher/optical
presentation without reviving any legacy direct G92 zero-reset code.

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

Machine Calibration → **Machine** → **Open Nozzle Calibration** now opens a
fixed-size, borderless procedure without any automatic motion. Select **one
installed nozzle head** and press **Start Runout / Run Again** to execute its
12-point optical sequence. Opening the dialog, changing the head selection,
and pressing **Preview Bottom Camera** do not command any machine, rotary or
toolhead motion. The latter only selects the Nozzle optical profile, enables
the camera/light and initializes fresh video. The procedure can also
initialize video automatically when Start is pressed without a prior preview.

The window reuses the shared `CalibrationStepRail` (auto-follow Running/
Failed steps) and `CalibrationVideoSurface` (actual bottom-camera frames and
typed target/candidate/accepted/rejected markers). The blue acquisition
circle uses the **actual** adaptive dark-opening search radius in pixels,
including its 72-pixel floor at low scale; green means a qualified
scale-locked optical opening. The expanded settings/diagnostics area has
one scrollbar and shows per-angle step duration/retries, measured offsets,
pixel/mm, optical spread and the candidate logical zero.

Each selected head has its own full 18-step procedure, independent of the
other installed head. A stale or missing selection aborts instead of
silently calibrating the next head. Results are runtime-only and the
other head's calibration remains untouched. No previous machine-navigation
tool selection is required; the existing safe head-selection path is
reused. Closing the modal never replaces the hardware emergency stop.

The Nozzle profile editor exposes only settings used by the runout dark
opening/camera pipeline: search radius, centering tolerance (runner hard
caps at 0.10mm), stabilization count, nullable per-profile exposure,
brightness, contrast, focus and pixel scale, RGB/power light overrides,
and camera display flags/zoom. The required shape/circle detectors and
light-on flag are enforced by the procedure and cannot be disabled.
Transient per-nozzle diameter/phase lock is produced by auto-tune and
is not manually editable or saved as global profile tuning. Hough
thresholds, staging plate ROI offsets, and irrelevant vision fields
are intentionally absent.

Search radius previews live on the active camera when valid; all other
changes remain drafts until **Apply Preview**. An unsaved draft or applied
preview blocks the motion sequence. **Save Profile** persists only
approved applied settings, while **Revert** or closing discards an unsaved
preview. Live profile changes reset detector history and never command
motion. The optical acquisition circle may remain visibly unchanged
between two radius values because of the 72px minimum: the diagnostics
report the effective pixel radius to make this explicit.

`NozzleRunoutProcedureDefinition` creates 18 ordered stages using the
shared `MachineOperationDefinition` and `MachineOperationSession`:
1. Verify machine-fiducial calibration and **every configured staging
   plate** is calibrated in this machine session. One physical plate with
   two reference holes is valid for Lumen Jr.; the larger Lumen PNP
   configuration has two plates, each with two reference holes.
2. Automatically select the bottom-camera Nozzle profile, turn on its
   shape/circle detectors, initialize the existing camera capture service
   if the user has not opened the camera tab, and await fresh live frames.
3. Read controller XY and validate the COMPLETE **camera-first** route:
   any previous tool head returning to camera navigation, absolute travel
   to the configured bottom camera in camera coordinates, and ONLY THEN
   applying the new head's relative offset at camera XY.
4. Move into camera navigation if necessary, verify the coordinate
   reset, raise to safe Z, move over the bottom camera and confirm XY.
   Now select the installed nozzle's tool head, verify its G92 reset
   preserves camera XY within 0.5 mm, then lower to detection height.
5. Rotate to 0, 30, 60, ..., 330 degrees, requiring new stabilized
   bottom-camera nozzle-circle observations at every step.
6. Revisit the maximum physical Y point and confirm repeatability to
   0.1 mm before writing runtime-only compensation to the head.
7. Calculate mean-centered 12-point optical compensation. No production
   placement correction is enabled by this procedure.

**The operator does not have to pre-open the bottom camera or enable its
Nozzle detectors.** Camera setup is procedure-owned. An unavailable camera,
missing live frames, invalid camera scale, or unsafe route stops before
any nozzle XY motion, displaying exactly which internal step failed.

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
(`JsonIgnore`): no machine configuration save, rotational-zero firmware
rebase, or production placement correction is performed yet. Existing
camera/tool navigation may still use its coordinate-frame G92 safeguards.

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
  every position in the camera-first route: current→camera navigation,
  camera→configured bottom-camera XY, and camera XY→new tool-head offset.
  Never apply the head offset at the original corner-of-bed position.
- Fail closed if any planned intermediate target (including the move
  returning from an already-selected tool head) is within 5 mm of an
  XY travel limit. Safe, in-bounds camera approaches may traverse the
  bed automatically; no arbitrary 85 mm operator-positioning prerequisite.
  Re-read controller XY after BOTH navigation-frame G92 transitions.
  Abort before the next move if the reported XY was not preserved within
  0.5 mm. Verify arrival at bottom-camera XY before switching heads.
- Log current XY, physical head offset, bottom-camera target, and work area
  before any accepted physical move. No controller `G92` or absolute XY
  camera approach is allowed merely because the operator opened the modal
  while camera readiness has not been established.
- Focused pure tests cover camera at XY maximum, unsafe return-to-camera
  and at-camera tool-selection legs, top-right starting positions that
  formerly failed unnecessarily, full-bed but in-bounds automatic approaches,
  invalid/non-finite input, one-plate and two-plate prerequisites, and
  automatic Nozzle detector setup.

The structural safeguards **do not prove the physical machine's coordinate
frame, camera target, rotary reference, Z clearance, or actual controller
behavior**. Windows WPF compilation and supervised bench verification are
required before resuming machine-connected automation. Do not rely on the
modal's Close button as an emergency stop.

## Motion/vision safeguards

- Requires connected, homed, idle, Manual-mode machine, calibrated machine
  fiducial and every configured staging plate, and an installed nozzle
  assigned to Head 1 and/or Head 2. Procedure-owned preparation automatically
  configures Nozzle circle detection, starts camera capture and waits for
  live frames. The saved camera device/position/scale must be valid; failures
  are visible internal stage results rather than user setup instructions.
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
3. Preview the bottom camera without motion, select one installed head, then
   press Start only under supervision. Verify all 12 measurement rows and the
   maximum-Y revisit for **that selected head**, including camera Y orientation,
   tool-change safety and the WPF current-stage indicators. Repeat as a separate
   supervised procedure for the other installed head.
4. Repeat the sweep after machine rehome to establish repeatability.
5. Verify a nozzle swap/reinstallation clears only its owning head's
   `IsCalibrated`, verify a machine reconnect/rehome clears both heads,
   and require a new sweep before trusting a changed installation. Add
   production runout compensation separately after physical acceptance.
6. Then proceed with camera-to-tool static offset task T003.

Tests connecting to external hardware must carry `Category="Integration"`;
pure deterministic runout math/session tests run in the canonical suite.
No API, NuGet stable release, Build Server changes, or deployment is implied.
