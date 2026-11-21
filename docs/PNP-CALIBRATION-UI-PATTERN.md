# PnP calibration UI pattern (T008 first implementation)

## Scope
Each calibration procedure is independently runnable. Camera Exposure (T008) establishes the reusable WPF shell; Machine Fiducial (T009) and camera/tool alignment (T003) reuse it later. This is NOT an orchestrator.

## Simple by default
- Left: compact, scrollable internal steps with Pending, Running, Passed, Failed or Blocked and elapsed time.
- Center: prominent live camera image and overlays tied to actual processed frames.
- Header: current procedure, real camera, detector/algorithm, camera baseline/profile and run/verified/saved state.
- Bottom: short measured result and prominently visible safety/failure explanation when needed.
- Detailed measurements, diagnostic images, tuning parameters and event timeline in panels collapsed by default. Never hide required operator actions.

## Video annotation contract
Use one shared overlay renderer/coordinate system for camera frames. Visually distinguish search region, detected candidate, accepted/stable object, positional error and verified result. Include units, timestamps/frame age, legend and data source. Transforms must respect actual image crop, mirror and zoom. No invented confidence scores. Invalidate stale observations when camera/profile/scale change.

## Exposure algorithm
Use the existing bounded dark/white reference auto-tune (`ImageCaptureService.RunAutoTuneExposureAsync`). Authoritative baseline is `MachineCamera.Exposure`; `macfiducial` profile inherits it after success. Failed sampling or persistence preserves last-good baseline and unrelated profile overrides. Show actual exposure, sampled dark/white image levels, temporal spread, contrast, iteration count, sampled frames, expected target bands, elapsed time, failure reason, verified vs saved state. Hard-coded algorithm constants remain read-only diagnostics unless explicitly wired to validated runtime settings.

## Reuse
T009 reuses step panel, camera surface and overlay semantics with real fiducial-circle observations. Staging-plate and T003 will adopt independently. Keep safety gates and no automatic chaining.
