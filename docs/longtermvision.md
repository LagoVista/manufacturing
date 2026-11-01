# Long-Term Vision

## Purpose

The long-term goal is to turn the LagoVista pick-and-place stack into a calibration-driven machine platform where motion, vision, feeder geometry, and tooling all share a trustworthy physical model.

The near-term work remains deliberately practical: make the existing machine reliable on real hardware, keep the production WPF application usable, and prove each calibration and vision capability in the Vision Workbench before expanding the architecture.

The long-term architecture should emerge from proven machine behavior rather than drive it prematurely.

## Guiding Principles

- Make the happy path work, then tighten the screws.
- Repository and production machine behavior are authority.
- Preserve production usability while improving boundaries behind it.
- Prefer deterministic geometry and calibration for physical measurements.
- Use machine configuration and package data as authoritative inputs; do not duplicate physical constants in workbench logic.
- Treat calibration as persisted machine state, not as temporary UI state.
- Adapt imaging conditions to known physical geometry; never adapt the expected physical geometry to a bad image.
- Avoid large framework or UI rewrites until the underlying machine physics are proven.

## Machine and Vision Direction

The machine should become increasingly self-calibrating and confidence-driven.

### Feeder and Tape Geometry

The production feeder/package model should drive tape pitch, package offsets, feeder orientation, hole-side conventions, and pickup calculations.

Classical machine vision is the preferred tool for constrained physical geometry such as:

- sprocket-hole detection,
- tape-axis estimation,
- hole pitch,
- pixels-per-millimeter,
- nozzle center,
- fiducials,
- package alignment,
- camera/tool offsets.

The workbench should remain the proving ground for these algorithms, but the calculations themselves should live in reusable production services/models rather than workbench-only code.

### Camera-to-Tool Calibration

Camera calibration should establish repeatable physical relationships between each camera and the machine/tool coordinate system.

Static camera-to-tool offset should be modeled separately from rotation-dependent nozzle eccentricity.

The top and bottom cameras should ultimately support repeatable calibration flows that can be rerun when confidence is lost or hardware is changed.

### Nozzle Rotation Calibration

The bottom camera is well suited to optical nozzle calibration.

A future calibration sequence should:

1. establish a repeatable optical rotation-home reference,
2. rotate the nozzle through a complete sweep,
3. measure nozzle-center displacement through the sweep,
4. fit or store a compensation curve,
5. interpolate compensation during placement.

A practical implementation may use measurements every 15 or 30 degrees rather than requiring a complex continuous model.

Rotation home should be treated as machine state that may need to be re-established after power loss, mechanical disturbance, or loss of confidence.

## Adaptive Imaging

Exposure, illumination, and threshold should become bounded adaptive calibration parameters.

The expected operating pattern is:

1. start with the last-known-good settings for the camera and operation,
2. run the normal detection,
3. accept the result when confidence is high,
4. only enter tuning when confidence degrades,
5. perform a bounded deterministic search,
6. cache the successful settings for subsequent parts.

The likely tuning order is:

1. controlled LED illumination,
2. camera exposure,
3. threshold or related detector parameters.

Candidate settings should be evaluated against physical expectations such as:

- valid hole count,
- expected pitch,
- expected radius,
- line-fit residual,
- false-positive count,
- positional consistency.

This tuning should not run exhaustively for every frame. The first part or changed context may pay the tuning cost; subsequent parts should reuse the calibrated state.

A controlled enclosure or shroud with a matte interior and controlled LED illumination may provide more value than increasingly complex image-processing algorithms by reducing ambient-light variability.

## Classical Vision and Neural Vision

Neural vision is complementary to, not a replacement for, deterministic machine vision.

Classical vision should continue to own measurements where precise geometry matters.

Neural models may later help with semantic questions such as:

- identifying printed component markings,
- confirming resistor values or color bands,
- detecting LED orientation or polarity,
- recognizing IC pin-1 markings,
- recognizing diode or capacitor polarity,
- detecting the wrong component or package.

A useful hybrid architecture is:

- neural model: locate, segment, classify, or interpret,
- classical vision: compute precise geometry,
- deterministic application logic: validate against BOM/package/machine expectations.

## Data and Learning

As the machine matures, useful image events should retain enough metadata to reconstruct what happened.

Potential retained context includes:

- camera identity,
- machine XYZ position,
- nozzle/tool identity,
- commanded rotation angle,
- exposure and illumination settings,
- active vision profile,
- detector results,
- confidence,
- accepted/rejected outcome,
- human correction when one occurs.

This data could later support improved calibration, diagnostics, and training data without making machine operation dependent on an AI service.

## Production WPF Direction

The existing WPF pick-and-place application remains valuable and should not be discarded merely to modernize the UI.

Near-term work should preserve the current operator experience while extracting or reinforcing reusable service boundaries around:

- machine connection and motion,
- machine state,
- camera capture,
- vision processing,
- feeder geometry,
- calibration,
- tooling.

Existing production controls and view models can continue to operate while those lower-level boundaries improve.

A broad WPF control-library rewrite is not a prerequisite for the machine work.

## Future Edge Architecture

Once machine physics, calibration, and vision are proven, the preferred future architecture is a headless edge runtime.

A likely shape is:

### Edge Runtime

A .NET service running on the machine owns:

- deterministic machine motion,
- safety constraints,
- machine state,
- camera acquisition,
- calibration state,
- real-time vision,
- feeder/tool operations.

The edge runtime should own anything where latency, safety, or physical determinism matters.

### Local and Remote UI

The operator interface can eventually move toward a browser-based client, potentially running locally in kiosk mode while also being accessible from tablets or other devices.

An Angular or similar UI could communicate with the edge runtime through HTTP and SignalR/WebSockets.

This allows the UI to evolve independently without moving machine-critical behavior into the browser.

### Platform Responsibilities

The cloud/platform should remain responsible for higher-level concerns such as:

- configuration authority,
- orchestration,
- history,
- diagnostics,
- software/package lifecycle,
- fleet-level visibility,
- long-running analysis.

The platform should not become a hard real-time dependency for machine motion.

## Hardware Direction

A Raspberry Pi 5-class device is likely sufficient for deterministic classical machine vision and machine-control workloads.

A Jetson-class device becomes attractive if heavier local neural inference becomes an important part of the operating loop.

The hardware decision should follow proven workload requirements rather than lead the architecture.

## Evolution Path

The intended progression is:

1. prove authenticated Workbench operation on the real machine,
2. prove motion, cameras, and authoritative machine configuration,
3. make feeder pickup geometry authoritative,
4. establish camera/tool calibration,
5. establish nozzle rotation and eccentricity calibration,
6. make vision settings adaptive and confidence-driven,
7. preserve production WPF while tightening service boundaries,
8. only then introduce a headless edge runtime and browser/mobile control surface,
9. layer semantic/neural inspection on top where it provides clear value.

The result should be a machine that is physically trustworthy first, operationally maintainable second, and increasingly intelligent without sacrificing deterministic control.
