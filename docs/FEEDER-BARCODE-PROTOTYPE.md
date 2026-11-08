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

## Two separate vision profiles

- **Feeder Origin** (`feederorigin`): precisely position the camera at the first marker bar's reference corner using the current crosshairs and **Set Feeder Origin** action. For vertical feeders this is the marker's top-left corner; for horizontal feeders it is the bottom-left reference corner defined by the placement convention. This profile no longer decodes the barcode.
- **Feeder Barcode** (`feederbarcode`): uses the fixed 10-pixel, known-orientation, binary/XOR decoder. **Frame Barcode** selects this profile after finding the configured feeder origin and repositions for a longer barcode view. It does not update the feeder's origin or tape-hole geometry.
- The barcode profile appears in the existing vision-profile selector. On first selection it copies feeder-origin settings (exposure, focus, threshold, pixels/mm, etc.) and is thereafter an **independent** camera profile. Existing profile settings are left untouched.

## First implementation

- `FeederBarcodeSignal.FindBars` detects bounded black runs, measures width in mm, and classifies the four prototype widths.
- `FeederBarcodeDetector` reads **only one fixed 10-pixel-wide band starting at image center**, along the **known feeder orientation** (right of center for vertical, below center for horizontal). It majority-votes the ten binary pixels at each position and XORs adjacent positions to find black/white transitions. No scanning other regions, orientation guessing, Hough/contour detection, or scoring multiple candidates. `FeederBarcodeSignal` normalizes widths using calibrated pixels/mm and classifies the four prototype widths.
- The barcode vision mode bypasses the **circular hole-detection mask**; the other vision modes retain their existing preprocessing. OpenCV is still used to capture and display the image, but barcode detection itself uses C# pixel comparisons and run lengths.
- `ShapeDetectionService` annotates visible bars **only** in the new `feederbarcode` profile. The existing `feederorigin` profile remains available for precise corner alignment.
- A **Frame Barcode** button explicitly moves the camera **along the barcode** to place the expected first bar at roughly 35% of the camera view: toward the top on vertical feeders, left on horizontal feeders. It uses the **actual captured frame size** and calibrated pixels/mm. The original **Go to Feeder Origin** retains its existing centering behavior.
- An orange/yellow 35% framing guide is shown for the selected orientation; it is a visual target, **not an automatically detected edge**.
- No automatic multi-frame sweep, source-of-truth calibration writes, or pickup operations have been added. The Frame Barcode button is the only new motion and is explicitly user-triggered.

A regression test reads the committed `tests/LagoVista.PickAndPlace.GCodeTests/TestImages/FeederBarcode/vertical_barcode_01.png` and asserts the sampling stays inside the ten-pixel band. Start by checking repeated width measurements at fixed machine position, then scan overlapping camera positions, decode the full marker, and finally connect the decoded start bar to existing tape-hole centering.
