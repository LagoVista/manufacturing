# T010 — Standalone Staging Plate Calibration

## Entry point

**Machine Calibration → Machine → Open Staging Plate Calibration**

Select one staging plate from the list in the dedicated modal. Opening the
window or selecting a plate does not move the machine. The old **Staging Plates**
tab remains the authority for configuration and reference-hole assignment;
the Machine tab is the launchpad.

This procedure is intentionally separate from T008 exposure, T009 machine
fiducial origin, T004 nozzle runout, and deferred T003 camera/tool offset.
The legacy all-plate continuation in `MachineCoreActionsViewModel` is
unchanged. T010 never calls it, never sends G92, and never automatically
calibrates the next plate.

## Independent seven-step procedure

1. Validate a connected, homed, Manual-mode machine with a verified
   machine-origin calibration **in the current session**. The selected
   plate must define its fiducial row and column and two distinct reference
   holes, all using valid grid positions.
2. Check acknowledged firmware idle, select the existing
   `stagingplatehole` profile and live position camera. Calculate a nominal
   first-hole position using `StagingPlateUtils` and the machine fiducial
   position, not the stale previously saved reference coordinates. Raise
   to safe Z before navigating XY.
3. Acquire fresh, stabilized production Hough-circle observations newer
   than the last completed movement. Center on hole 1 using at most six
   small bounded XY corrections, confirming serial idle and clearing
   detector history after each move. Read the true controller-reported
   machine coordinate for hole 1.
4. Derive the nominal second-hole approach from the **newly measured**
   first hole and authoritative plate spacing/grid configuration. Approach
   the second hole with safe-Z, work-area, mode and idle gates.
5. Detect, center and measure hole 2 using an independent fresh detector
   history and real controller-reported machine coordinates.
6. Verify separation and orientation against the plate's configured
   geometry, rejecting collapsed, reversed or implausible two-hole
   measurements.
7. Only after both observations pass and machine/camera/profile remain
   unchanged, write both reference-hole coordinates and save the machine
   configuration. Mark `IsCalibrated` only after persistence succeeds.
   A failed run preserves prior reference positions.

The live video uses the exact T008/T009 `CalibrationVideoSurface`,
`CalibrationStepRail`, and typed circle overlay semantics. Expand
diagnostics for frame times, circle size, center offsets, stability,
active Hough profile and timestamped step/motion/save history.

## Diagnosing the second-reference-hole travel gate

The second reference is computed as **freshly measured first-hole XY**
plus the configured reference-hole row/column displacement at the plate's
current hole spacing. Detecting and centering the first reference does not
prove the second calculated target is physically reachable.

If a calculated hole target is outside configured `Machine.Settings.WorkAreaSize`
(X=0 through `WorkAreaSize.X`, Y=0 through `WorkAreaSize.Y` in the
machine's current G-code coordinates), the procedure **must stop before any
XY move**. Open the diagnostics panel to see the exact target, controller's
current XY, requested delta, allowed maximum and magnitude by which the
target exceeds the bounds. The stage's failure text and UTC event timeline
contain the same values.

Before revising any travel configuration, verify all of the following against
the actual installed fixture and homed controller:

- The selected plate's reference-hole row/column and `HoleSpacing` are
  correct, and the hole-1 controller coordinate makes sense after G92.
- The second reference is physically within confirmed collision-free
  machine travel, including camera/tool clearance and frame endstops.
- The controller's actual coordinate origin and firmware travel envelope
  agree with configured `WorkAreaSize`. A negative target **cannot** be
  fixed merely by increasing the positive-axis maximum.
- Only then update the machine's authoritative Work Area Size setting if
  its current value is smaller than safely verified physical travel.

Do **not** compensate by silently clamping the target, bypassing bounds,
increasing the requested movement without verification, or using the frame
size as if it were a safe work-area limit. The target remains blocked until
the configuration/geometry discrepancy is resolved.

## Safety and test boundaries

No machine action starts automatically. Each move requires a verified
current-session machine origin, position camera and actual
`stagingplatehole` Hough detector, homed/connected/Manual-mode machine,
configured positive XY travel bounds, safe-Z, finite small centering
corrections, a controller idle acknowledgment and settled Busy state.
Changing the machine/camera/profile/scale blocks success. An observation
must be captured *after the last completed move*; prior-circle
observations cannot authorize the next correction or save.

The geometry test allows at most 2% reference span error (minimum 1 mm)
and 15 degrees of installation rotation. Those initial engineering
bounds must be validated against the actual fixture and adjusted using
observed physical evidence if necessary. They are not a claim of final
production tolerance.

Canonical Build Server core .NET tests cover staging geometry and
reused Hough freshness guards. They do **not** compile Windows WPF or
validate actual motion. Operator should locally build the app, open
the window and inspect the live alignment and overlay without starting
the run, then perform a supervised first calibration with emergency
stop accessible. T010 remains open until that physical acceptance.
