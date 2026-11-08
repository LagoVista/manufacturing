# Feeder barcode prototype (machine-vision reference)

## Fixed physical convention

- **Vertical strip feeder:** place the marker at the **top-left**; read the long axis **top to bottom**.
- **Horizontal strip feeder:** place the marker at the **bottom-left**; read the long axis **left to right**.
- For the current vertical feeder white-corner pilot, the **upper-left white cut corner** is the optical feeder-origin reference. The first black barcode START bar is a separate internal decoding/scale reference. Do not interchange the two points.

## White cut corner: existing Feeder Origin vision workflow

For a vertical feeder, align the printed label's **upper-left white cut corner**
with the physical feeder reference. This point is **not** the top-left of the
black barcode START block. The normal Feeder Origin vision profile now feeds
the existing `CornerDetector` with raw grayscale pixels, where
`FeederWhiteCornerFinder` uses its threshold and `TargetImageRadius`
settings to find connected WHITE regions. It now rejects bright text/noise
that cannot produce **both** a sufficiently long horizontal TOP edge and a
vertical LEFT edge. It fits straight lines to edge samples **away from the
rough cut tip** and reports their mathematical intersection. The live camera
overlay shows both fitted edge segments, the intersection crosshair, raw edge
support lengths and residual fit error; **STABLE** is green and **ACQUIRING**
is orange. The regular vision objects list shows pixel position and XY offsets.

The Feeder Origin profile's **Corner** (Harris) and **Line** (Hough) sliders
are **not** part of this known-L-shape algorithm. To tune it, use
`TargetImageRadius`, threshold (`ApplyThreshold` /
`PrimaryThreshold`), focus, exposure and lighting. The specific screenshot
with the right-hand bright markings produced a fitted intersection near
`(104, 215)` pixels across grayscale thresholds 100–200 in an offline
screenshot test; this is **not yet** a live-machine accuracy claim.

The workflow reuses the UI that already existed:

1. Select the strip feeder and its staging-plate reference-hole **row and
   column**, then click **Go to Feeder Origin**. This moves to the
   **calculated** position from staging-plate hole geometry and the mounting-hole
   offset. It explicitly *ignores* the previously saved `OriginOffset` for
   this initial approach and switches to the `Feeder Origin` camera profile.
2. Tune that profile's exposure/lighting, threshold (`ApplyThreshold` and
   `PrimaryThreshold`) and `TargetImageRadius` until the green white-corner
   crosshair stays on the same corner across consecutive frames.
3. Use the **second Play** button in the position-camera controls
   (`CenterFoundItemCommand`). This consumes the existing stabilized
   `MVLocatedCorner.OffsetMM` and jogs toward the corner, bounded by
   Manual/homed/work-area checks and a 2.5 mm correction per axis. It is
   operator-triggered, not autonomous. The detector resets after the jog so
   the next camera frame must establish a new stable corner.

The corner detector uses the verified 5x black START bar as an optical
pixels/mm reference when enough of the marker is visible; otherwise it
identifies the scale as **CAMERA SCALE** and relies on the position camera's
configured pixels/mm **for display only**. The second Play button now **requires
START SCALE** before any feeder-corner jog: the nominal camera scale may differ
by nearly a factor of two from the 2.5 mm reference, causing an overshoot.
The camera overlay exposes the source and numerical pixels/mm, while a machine
status message records the optical scale, commanded relative X/Y correction,
and before/after target coordinates. After every jog, the tracker intentionally
reacquires five stable frames and may briefly show orange. Pressing Play at a
stable, already-centered corner reports a no-op.

No operation in this workflow writes `OriginOffset` or changes barcode
scanning. The existing **Set Feeder Origin** button remains the explicit place
where the operator saves an offset.

## Accuracy chain

1. Resolve the approximate feeder position from the calibrated staging plate mounting hole (initial tolerance approximately 2 mm).
2. Locate the white cut corner using the Feeder Origin profile and the existing second Play camera jog; measure the residual before saving any feeder-origin offset.
3. Use existing StripFeederViewModel first-hole geometry to approach and center the first tape hole.
4. Use feeder length and tape pitch to approach and center the last tape hole.
5. Use the two measured tape holes and existing StripFeederPickupCalculator/FeederPositionCorrection to compute pickup locations.
6. Component-in-pocket detection is **out of scope** until feeder alignment is proved.

Use the existing feeder origin, tape-hole offsets, and orientation transformations. Do not duplicate or replace those calculations.

## Current 200 x 20 prototype print

The initial print is a **20 x 2 mm** black-on-white 11-bar prototype. The four bar widths are approximately **0.4, 0.8, 1.2, and 1.6 mm** with **0.5 mm white gaps**. The 200-pixel PNG also has **1.7 mm of leading white margin** before the first bar. The current detector uses these prototype widths and only reports visible bar measurements; it does **not** yet decode identities or update the feeder.

The physical first-bar-to-origin placement must be measured independently of that white margin. Avoid baking the prototype's 1.7 mm margin into feeder geometry.

## Calibrated 5x start-bar decoder (implemented; requires a new printed marker)

The proposed eleven-bar marker uses one **5-unit start bar**, eight four-level data bars, a four-level check bar and one **0.5-unit end bar**. One base unit is **0.50 mm**, so the physical start bar is **2.50 mm**, the data widths are **0.50, 0.75, 1.00 and 1.25 mm**, and the end bar is **0.25 mm**. Keep the gaps between bars narrow and consistent, and print the marker at actual size.

The 10-pixel band and XOR transitions are unchanged. `FeederBarcodeSignal.ReadFromStart` validates the first fully visible black bar as a plausible 5x start, then checks that the **next two** measured bars fit the known width classes. The pixel scale is `startBarPixels / 5`; classification compares later bars to **1, 1.5, 2 and 2.5 units**, without relying on exact camera pixels/mm.

A clipped/missing/malformed start leaves the result explicitly **START NOT VERIFIED**, with raw widths only and no trusted symbol values. The old `vertical_barcode_01.png` is a useful legacy regression, **not a calibrated 5x marker**. The single-frame `ReadFromStart` method reports partially visible bar symbols; the separate `FeederBarcodeScan` combines captured frames into a validated 11-bar identity without changing feeder coordinates.

## Live camera preview stability and diagnostics

The orange `255px` and green `START 5x` overlays are two formatting branches of the **same** ten-pixel-band decoder, not two physical detectors. On the actual marker, the first bar measured 253-255 px and the next data bar measured 63 px; at 255 px the normalized first data width is 1.235x (accepted), while at 253 px it is 1.245x (rejected by the <0.24-unit symbol tolerance). A two-pixel threshold-edge change can flip the live start verification.

The live preview now **always shows raw pixel widths alongside relative units** when calibrated. A presentation-only `FeederBarcodeTemporalMonitor` records the last 30 raw frames' first-bar leading/trailing edges and width ranges, and annotates raw verification count (`RAW30`) to help distinguish whole-image movement from edge flicker. It displays a 3- then 5-frame odd-window binary temporal median only while the leading bar stays at the same pixel position. If the median momentarily fails, at most two old verified display frames may be shown with an explicit `DISPLAY HOLD` label; repeated failures revert to unverified. Start disappearance, sizable movement or vision-profile reset clears the history.

**Safety/data distinction:** `FeederBarcodeDetector.Observation.Signal` remains the **unmodified raw binary sample**, and the full 11-bar `FeederBarcodeScan` consumes that raw sample, not the stabilized/held overlay. The monitoring adds no camera search, hardware motion, exposure changes, or persisted feeder calibration.

## Full 11-bar reader (implemented, hardware validation pending)

- **Frame Barcode** uses the **calibrated feeder origin** (calculated row/column position **plus** the feeder's `OriginOffset`), including the adjustment made by **Set Feeder Origin**. This differs intentionally from **Go to Feeder Origin**, which approaches the uncorrected row/column location for first-time white-corner optical calibration. The existing barcode camera-scale-based length framing and +1 mm lateral shift on vertical feeders remain unchanged. If the optical corner was jogged to but **Set Feeder Origin** was not used, the offset is not yet available for this operation. Confirm the live overlay reports **START OK**, then explicitly click **Scan Barcode**. This takes up to seven fresh captures, advancing at most 3 mm (or one quarter of the frame's estimated span) along the selected feeder's known orientation. Each move waits for machine idle; cumulative advance is capped at 18 mm and must remain inside the configured work area. The first view locks the 5x start scale; subsequent views register at least two matching overlapping bars before merging them, rejecting misaligned captures instead of counting duplicate bars. The live preview may say START NOT VERIFIED when that bar has left the current frame; the scan retains its initial calibration. No background autonomous scanning.
- **FeederBarcodeScan** uses the same fixed 10-pixel median/XOR samples. The start bar calibrates symbol widths in pixels. Later frames can lack the start bar entirely; fully bounded bars are projected into machine coordinates using the measured camera location and the start-derived pixel scale, then overlapping observations are deduplicated. No panorama stitching, shape search or second detector.
- The **current 20 x 2 mm test sheet** has a 2.5 mm long start bar and eight symbols 0,1,2,3,0,1,2,3 (ID `0x1B1B`). Although its original check bar was marked *provisional*, its printed symbol 1 also matches the explicitly chosen **CRC-2** for this ID: polynomial x²+x+1, initial register 3, 16 data bits MSB-first, no reflection/final XOR. The final 0.5x bar must be present and gaps must be consistent. Two check bits are modest error detection, not proof of authenticity.
- This first machine-coordinate stitching implementation assumes the test sheet's **known physical 2.5 mm start length** when converting the optical pixel scale to millimeters for overlapping moves. Relative bar decoding itself tolerates other marker sizes, but moving between images of differently scaled prints will require passing their actual start length, or independently calibrating motion scale.
- On a complete valid read the UI shows **BARCODE VERIFIED: ID 1B1B, 11 bars, CRC-2 OK**. On failure it shows the missing bar count or specific gap/check/end error. **Do not treat 0x1B1B as a production-unique feeder identity:** every copy of this test sheet has that same ID.
- The scanner only reports a provisional decoded identity. It **does not** persist a feeder ID, update origin/tape-hole geometry, calibrate physical machine coordinates, or perform pickup. Those links remain future work after on-machine proof.

## Two separate vision profiles

- **Feeder Origin** (`feederorigin`): precisely position the camera at the first marker bar's reference corner using the current crosshairs and **Set Feeder Origin** action. For vertical feeders this is the marker's top-left corner; for horizontal feeders it is the bottom-left reference corner defined by the placement convention. This profile no longer decodes the barcode.
- **Feeder Barcode** (`feederbarcode`): uses the fixed 10-pixel, known-orientation, binary/XOR decoder. **Frame Barcode** selects this profile after finding the configured feeder origin and repositions for a longer barcode view. It does not update the feeder's origin or tape-hole geometry.
- The barcode profile appears in the existing vision-profile selector. On first selection it copies feeder-origin settings (exposure, focus, threshold, pixels/mm, etc.) and is thereafter an **independent** camera profile. Existing profile settings are left untouched.

## First implementation

- `FeederBarcodeSignal.FindBars` remains the original prototype-width detector for existing test coverage. Live `feederbarcode` now uses `ReadFromStart`, which requires a verified start before producing four-level symbols.
- `FeederBarcodeDetector` reads **only one fixed 10-pixel-wide band starting at image center**, along the **known feeder orientation** (right of center for vertical, below center for horizontal). It majority-votes the ten binary pixels at each position and XORs adjacent positions to find black/white transitions. No scanning other regions, orientation guessing, Hough/contour detection, or scoring multiple candidates. New-marker width classes are derived from the 5x start's pixel width; physical pixels/mm remains available only for rough plausibility and XY positioning.
- The barcode vision mode bypasses the **circular hole-detection mask**; the other vision modes retain their existing preprocessing. OpenCV is still used to capture and display the image, but barcode detection itself uses C# pixel comparisons and run lengths.
- `ShapeDetectionService` annotates visible bars **only** in the new `feederbarcode` profile. The existing `feederorigin` profile remains available for precise corner alignment.
- A **Frame Barcode** button explicitly moves the camera **along the barcode** to place the expected first bar at roughly 35% of the camera view: toward the top on vertical feeders, left on horizontal feeders. It uses the **actual captured frame size** and calibrated pixels/mm. For **vertical** feeders only, the calibrated origin is the **top-left of the first black bar**; Frame Barcode also shifts **+1.0 mm in machine X** to center the camera on the 2 mm-wide strip. The original **Go to Feeder Origin** retains its existing centering behavior, and horizontal framing is unchanged.
- An orange/yellow 35% framing guide is shown for the selected orientation; it is a visual target, **not an automatically detected edge**.
- A separate **Scan Barcode** button now triggers bounded overlapping machine moves and fresh image samples; **Frame Barcode** remains the explicit framing move. Neither action writes calibration settings or triggers pickup.

A regression test reads the committed `tests/LagoVista.PickAndPlace.GCodeTests/TestImages/FeederBarcode/vertical_barcode_01.png` and asserts the sampling stays inside the ten-pixel band. Start by checking repeated width measurements at fixed machine position, then scan overlapping camera positions, decode the full marker, and finally connect the decoded start bar to existing tape-hole centering.
