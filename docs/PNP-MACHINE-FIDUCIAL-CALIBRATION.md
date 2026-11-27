# T009: Machine Fiducial Calibration

## Operator entry point

Open Machine Calibration -> Cameras -> **Open Machine Fiducial Calibration**.
The new fixed-size, borderless WPF dialog reuses T008's step rail, live camera
surface, marker colors, header drag surface and collapsed troubleshooting panel.
Opening it is safe and does not initiate movement. Click **Run / Run Again**
to execute the standalone machine-fiducial procedure.

## Independent stages

1. Validate connected, homed, idle/manual machine; live position camera;
   configured fiducial, no overlapping locator.
2. Confirm the previous machine command queue has actually finished
   (firmware M400 acknowledgement and two consecutive settled idle-status
   reads). Repeat this guard after activating **macfiducial**, immediately
   before the first new safe-Z or fiducial XY movement. A short, bounded
   status transition after exposure tuning is tolerated without ignoring
   the machine Busy flag; an unacknowledged M400, persistent Busy state,
   another locator, disconnected machine or changed Manual mode blocks
   the move with a clear diagnostic.
3. Follow the established safe-Z move and detect a fresh Hough circle
   from production image processing.
4. Use stable, physically centered readings to determine bounded XY
   corrections. After **each** correction, wait for machine idle, discard
   all detector history, then collect a new stable observation.
5. Only after the camera/frame/profile/scale remain unchanged and the
   observation is fresh, stable AND within both center tolerances, queue
   the existing G92 machine-origin reset and verify the queued commands
   completed. Only then set the origin-calibrated flag. On success,
   existing staging plates are marked uncalibrated and that dependent
   state is saved. Their physical reference locations are preserved.

This procedure does **not** run exposure tuning, staging-plate calibration,
T003 tool alignment or any other calibration step. It only invalidates
staging-plate calibrated flags after successfully resetting the origin
and saves that status; the operator runs staging calibration separately. The legacy Machine Vision
Origin action remains unchanged for backward compatibility; it still has
its original staging continuation. Do not confuse that legacy button with
the dedicated standalone T009 entry point.

## Live evidence and diagnostics

The processed position-camera feed is unmodified. A shared typed overlay
renderer draws the actual centered target/search region (blue), Hough
candidate (yellow/red) and stable verified center (green). Circle coordinates
and radii are reported in original processed-frame pixels and transformed
using the same Uniform image mapping as the displayed video. Diagnostics
show actual frame UTC, track detection count, radius and center error
in px/mm, physical positional spread, tracker stabilization, selected profile
and its real Hough parameters, correction count, and machine position.
No fake confidence estimates. Existing production detector is unchanged.

## Optional manual controls

Expand **Advanced tools** (collapsed by default). Within it, each of the
following panels is independently collapsed until needed.

- **Tune Hough circle detector** edits a draft of the *actual* macfiducial
  production parameters: Hough DP, physical separation, edge/accumulator
  thresholds, minimum/maximum circle radius, search radius, centering
  tolerance, and stabilization count. No unrelated contour or nozzle
  controls are shown.
- **Apply Preview** validates the numeric values and applies them only in
  memory; no save occurs. Prior detections are invalidated. **Observe
  8 Seconds** runs the real detector against fresh live top-camera frames
  without moving the machine, and displays circle overlays and measured
  statistics. The observation is diagnostic only and cannot set origin.
- **Save Profile** explicitly persists the applied macfiducial changes.
  **Revert** restores the saved profile and discards the draft. Closing the
  dialog also reverts any unsaved preview. Automatic calibration cannot
  start with unsaved preview settings, so its subsequent origin save cannot
  accidentally persist unapproved changes. Invalid typed numeric input
  blocks Apply Preview.
- **Fine position jog** is manual single-axis X/Y movement in fixed 0.1 mm
  or 0.5 mm increments. It requires connected/homed/Manual/idle machine,
  active camera navigation (no selected tool head), no competing locator,
  valid camera, finite work-area coordinates, safe-Z completion and serial
  acknowledgement. It cannot jog outside XY travel, never resets origin,
  and invalidates all previously captured circle evidence. A fresh
  observation is mandatory for later automatic calibration.

No manual adjustment bypasses the existing fresh, stable, centered
origin-reset gate or starts the staging-plate sequence. Treat every jog as
a physical machine movement with the emergency stop within reach.

## Failure semantics / physical acceptance

No origin reset happens before completed safe movement, fresh and stable
circle measurements, and centered verification. Changing the camera,
profile, frame freshness, calibration scale or operating mode blocks
success. Failure before G92 preserves the previous machine-origin flag.
If G92 has been queued but the serial acknowledgement is missing, origin
status is marked **unverified** instead of falsely claiming prior calibration
remains valid. Bounded detector timeout and finite correction attempts
prevent indefinite machine-controlled correction.

Canonical .NET tests cover pure evidence qualification and projection
logic; they do not compile the Windows WPF app or prove real hardware
movement. Operator must compile locally, inspect UI rendering/overlays,
then supervise first physical motion with emergency stop accessible.
