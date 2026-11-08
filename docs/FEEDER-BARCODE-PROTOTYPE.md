# Feeder barcode prototype (machine-vision reference)

## Fixed physical convention

- **Vertical strip feeder:** place the marker at the **top-left**; read the long axis **top to bottom**.
- **Horizontal strip feeder:** place the marker at the **bottom-left**; read the long axis **left to right**.
- The reference is the **leading edge of the first black bar**, placed at the feeder reference corner (target placement approximately 0.1 mm). Do not treat the printed label's white margin as the origin.

## Accuracy chain

1. Resolve the approximate feeder position from the calibrated staging plate mounting hole (initial tolerance approximately 2 mm).
2. Locate the first bar; refine the feeder origin to approximately 0.5 mm or better.
3. Use existing StripFeederViewModel first-hole geometry to approach and center the first tape hole.
4. Use feeder length and tape pitch to approach and center the last tape hole.
5. Use the two measured tape holes and existing StripFeederPickupCalculator/FeederPositionCorrection to compute pickup locations.
6. Component-in-pocket detection is **out of scope** until feeder alignment is proved.

Use the existing feeder origin, tape-hole offsets, and orientation transformations. Do not duplicate or replace those calculations.

## Current 200 x 20 prototype print

The initial print is a **20 x 2 mm** black-on-white 11-bar prototype. The four bar widths are approximately **0.4, 0.8, 1.2, and 1.6 mm** with **0.5 mm white gaps**. The 200-pixel PNG also has **1.7 mm of leading white margin** before the first bar. The current detector uses these prototype widths and only reports visible bar measurements; it does **not** yet decode identities or update the feeder.

The physical first-bar-to-origin placement must be measured independently of that white margin. Avoid baking the prototype's 1.7 mm margin into feeder geometry.

## Proposed next iteration (not yet implemented)

- Eleven bars: distinct start bar, eight four-level data bars (16-bit identity), two-bit checksum bar, distinct end bar.
- Candidate four data widths: 0.50 / 0.75 / 1.00 / 1.25 mm, with small fixed gaps.
- Candidate start/end framing: 5x and 0.5x the 0.5 mm width; verify with printed samples first.
- A single camera view may contain only part of the strip. Combine **overlapping measurements in calibrated machine coordinates** rather than stitching large images.
- Verify full identity and checksum before accepting a measured origin or persisting calibration.

## First implementation

- `FeederBarcodeSignal.FindBars` detects bounded black runs, measures width in mm, and classifies the four prototype widths.
- `FeederBarcodeDetector` searches vertical/horizontal scanlines through the live position camera image.
- `ShapeDetectionService` annotates visible bars in the existing `feederorigin` vision profile.
- No automatic camera sweep, source-of-truth updates, feeder moves, or pickup operations have been added.

Start by checking repeated width measurements at fixed machine position, then scan overlapping camera positions, decode the full marker, and finally connect the decoded start bar to existing tape-hole centering.
