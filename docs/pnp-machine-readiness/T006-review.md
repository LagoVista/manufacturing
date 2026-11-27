# T006 engineering review — November 27, 2026

Status: Integrated on master; physical verification pending.

- Source: [81bac2ff](https://github.com/LagoVista/manufacturing/commit/81bac2ff76cd718f0e85d4c1adae4c4ff72ac626); initial implementation 044b06bb8ae3e2caad5ffbd8bdb7f12d2e60f368.
- Initial headless regression tests: 556 passed, 0 failed, 0 skipped; process ce6551116fb448d79f5001c7cb6a8b44.
- Final source catalog build proof succeeded (NuGetPackage), process bf706e01668044048550da2622919efe. This is not proof of a Windows WPF executable build.
- Final source headless regression process 7da9f3797b6e4292928d66342005cd82: Succeeded; LagoVista/manufacturing:tests/LagoVista.PickAndPlace.GCodeTests/LagoVista.PickAndPlace.GCodeTests.csproj: 556 passed, 0 failed, 0 skipped (556 total). Report: journal receipt 37c5bc130b4607bce8176829cc4c76f0799aeef15f155262f26b749938f1f853. Stable DPP already matches the stable catalog.
- No publish/deploy or machine motion performed by engineering tools.

## Physical verification

1. Pull master, build the Windows application and connect to a homed, origin-calibrated machine in Manual. Save the configured feeder first, with its slot, rail and Photon UUID. Ensure the tool is clear, vacuum off, and no active job.
2. Select the saved feeder, unlock settings, enable manual testing/clearance. Ensure the Feeder Fiducial profile has circle detection, calibrated pixel scale, plausible radius range and search radius, and suitable threshold/exposure. Run Calibrate Feeder Placement. Watch the initial approach and bounded centering; abort via the machine safety controls if necessary.
3. Expect a verified candidate with measured XY/offset. Click Save explicitly. Reselect the feeder, reload if needed, and Go To Fiducial. Confirm the position-camera crosshair returns to the physical reference. Repeat for another slot/rail when available. If detection fails, adjust the existing profile and rerun; old fiducial offset remains unchanged locally. No pickup offset changes or tape advancement are expected.

## Residual limits

- Slot origin now uses rail SlotWidth rather than feeder Size.Height and rejects overlapping rails and unsupported 90-degree orientations. If previously calibrated offsets compensated for the old slot pitch, recalibrate them before production use. Existing offsets are machine-axis relative, not newly rotated.
- Existing Hough feeder-fiducial detector now rejects multiple eligible targets; auto threshold is not expanded to this profile in T006. Physical radius/tolerance and initial template approach need verification. A failed or ambiguous API write may require reload to determine persisted state. Manual and emergency-stop controls remain authoritative; no new global motion arbiter.
- No claims of physical accuracy, WPF compilation, or production acceptance until operator verification. T007 covers part presentation and pickup offset.
