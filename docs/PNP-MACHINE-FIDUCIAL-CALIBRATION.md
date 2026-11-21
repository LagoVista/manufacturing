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
2. Activate **macfiducial** and follow the established safe-Z camera move.
3. Detect a fresh Hough circle from production image processing.
4. Use stable, physically centered readings to determine bounded XY
   corrections. After **each** correction, wait for machine idle, discard
   all detector history, then collect a new stable observation.
5. Only after the camera/frame/profile/scale remain unchanged and the
   observation is fresh, stable AND within both center tolerances, queue
   the existing G92 machine-origin reset and verify the queued commands
   completed. Only then set the origin-calibrated flag.

This procedure does **not** run exposure tuning, staging-plate calibration,
T003 tool alignment or any other calibration step. The legacy Machine Vision
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
