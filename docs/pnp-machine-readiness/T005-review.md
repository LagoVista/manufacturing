# T005 engineering review — 2026-11-27

Status: source integrated on master; headless engineering evidence passed; Windows WPF application and supervised physical acceptance pending.

## Provenance

- Task: [T005](T005.md).
- Implementation: 9a9438dce8436635ff8908a53cb90ec9b82105a5; corrected source: 8baac2decd398cc53ce115d32e86d93204da8a1d.
- Transaction tests: 6084b4d572be8251f3b2c65d0bcb5fdfa491974f (test-only delta).
- Catalog NuGetPackage build at corrected source: succeeded, process b75555d0c0f643408da7ab59d560d777.
- Focused protocol cases: 9 passed, 0 failed, process 8f825a2908be429e9a8f1363e08ab5f5.
- Full GCodeTests at transaction-test commit: 550 passed, 0 failed, 0 skipped (headless non-Integration suite); process ac698c7637e64d199eeecd8471b4752f, journal receipt 783aa6368bc83f0824911c34026ebdc42bcce66d92f914cc25a5b71b61f3a508.
- WPF executable build is not covered by the catalog NuGetPackage proof. No Windows executable or physical feeder was exercised by these tests.

## Implementation boundary

Existing M485 bridge, Photon protocol handler, AutoFeederViewModel, PhotonFeederViewModel and WPF controls retained. Frame length, CRC, packet ID, address and status validation; subscribe before dispatch; one bus sequence at a time; bounded initialization and read-only feed-status polling. A physical move is sent once, never automatically repeated after ambiguity. Ambiguous tape position blocks further moves until operator reconciliation and initialization. Manual control requires explicit safe-clearance confirmation, manual mode and idle queue, validated distance and identity; manual test commands do not write inventory. Shared NextPartAsync decrements count only after confirmed feed status. No calibration or illumination source changed. No publish/deploy.

## Physical verification

1. Build and start WPF locally from master. Connect and home/calibrate as required by existing controls. Ensure no active job, clear the tool head and use safe test tape/conditions.
2. Mount one feeder in a known slot; scan only the needed slots. Confirm UUID and slot. Select it and identify it using its indicator light.
3. Confirm clearance, initialize, enter a known supported test pitch (e.g. 4 mm if appropriate to the installed tape), confirm clearance again and advance. Observe actual tape movement; check that acknowledged and completed are distinct. Repeat using a supported retract distance.
4. Verify configured-versus-discovered identity, visible communication failures and unchanged part count. Test missing feeder with a safe, non-moving setup; do not deliberately interrupt a live mechanical move. If an ambiguous outcome occurs, inspect tape and reconcile before reinitializing. Never infer physical part presence from a protocol acknowledgment.
5. Record actual hardware/firmware revision, slot, ID, distances, outcome, screenshot/log and inventory evidence. Keep T005 open until this check is accepted.

## Residual limits

Photon uses an eight-bit packet ID; correlation rejects unrelated frames while IDs differ, but cannot mathematically distinguish a valid delayed frame with an identical wrapped ID and slot. Bridge timeout messages have no correlation ID and are treated conservatively as failure. Reconciliation state is in-memory; after application restart, inspect physical tape position before resuming any ambiguous move. Full WPF binding/build, real transport timing and UI inventory behavior remain to be verified. Catalog build is not a WPF build. No firmware changes.

## Firmware investigation (separate follow-up)

Opulo publishes different V1 and V2 firmware images. Their documented feeder update uses an external programmer and STM32CubeProgrammer, not an RS485 update command. The published Photon protocol provides normal operating commands but no documented firmware-transfer command. RS485 is a feasible transport for a purpose-built addressed bootloader, but support on the installed feeder and M485 bridge must be established before implementation. No firmware update action should be exposed in T005.
References: https://docs.opulo.io/misc/photon/ ; https://docs.opulo.io/feeders/3-software-update/ ; https://docs.opulo.io/software-updates/ .
