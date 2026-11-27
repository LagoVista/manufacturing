# Guided Strip-Feeder Calibration Operation

**Planning Workstream:** [PnP Guided Calibration Operations](MACHINE-CALIBRATION-OPERATIONS-WORKSTREAM.md)
(Forge ID: `pnp-guided-calibration-operations`). This document describes
the feeder's technical steps; the Workstream file is the durable plan and
session-handoff authority.

Status: T001–T004 implemented and published; T005 automatic procedure
execution is in machine acceptance (2026-11-13). The seven-step runner now
includes a first-cut automatic rectangle-based pocket observation. The Windows
WPF compile and complete physical seven-step acceptance remain outstanding.

## Implemented procedural execution (2026-11-13)

The existing `MachineOperationDefinition` and `MachineOperationSession`
form the reusable ordered-step **container**. The new
`MachineOperationProcedureExecutor` runs the strongly typed actions from
the existing `StripFeederCalibrationRunner` and checkpoints the existing
Manufacturing operation journal after each transition. The WPF view renders
the session's seven step snapshots; it does not own any motion logic.

The operator-facing procedure has **Start** and **Reset**. Start moves,
vision-centers (up to five jogs within the active profile tolerance),
rechecks, saves the feeder origin, centers first/last tape holes and commits
their measured pair, then advances to the current/previous pocket checks.
No individual Next, Retry, Confirm, Cancel or history-save buttons are shown
in the procedure panel. Reset is enabled only after an operation stops, and
records cancellation of an incomplete run before creating a new session.
A queued machine movement may still complete; the machine's separate
emergency-stop control is authoritative for immediate physical intervention.

A failed machine move, unavailable camera observation, rejected profile/geometry,
timeout, or failed durable checkpoint stops further actions. Calibration values
are committed only after fresh vision evidence. The operator's **Start** is
the authorization to save verified offsets, not permission to guess results.

**New pocket observation pilot:** version 2 of the client procedure requires
`VisionVerified` evidence in steps 6 and 7, while the previously published
version 1 operation remains unchanged. After camera arrival and a fresh
detector reset, PRESENT requires at least one stabilized, centered rectangle
using the selected part-in-tape profile. EMPTY requires at least 15 freshly
processed camera frames spanning at least 850 ms, with **no rectangles at all**
in that observation window. The camera/profile/feeder/machine must remain
unchanged. Any detected rectangle prevents an EMPTY decision. No fresh frames,
stale frames, intermittent candidates, unexpected presence/absence, or a
timed-out acquisition block the procedure. Multiple nested rectangle contours
may originate from one part, so they are not treated as separate parts.
The detector reports dimensions/offsets and records evidence for each result.
This is not a pick operation and never increments or decrements inventory.

The current black-tape camera has demonstrated stable rectangle detection of
a present component, but the absence behavior and all-seven-step sequence
must still be verified on the Windows machine. Please **supervise the first
run**; do not claim production acceptance from unit tests alone.

## Feeder-wide calibration procedure — new T005 desktop pilot (2026-11-14)

A single **Calibrate Feeder** button on the saved Strip Feeder editor opens
`FeederCalibrationProgressWindow`. The former inline step panel has been
removed; the existing manual frame/scan barcode tools are collapsed under
**Manual barcode diagnostics**.

The window shows an ordered, scrolling progress list with each stop's status,
error/evidence text, elapsed duration and retry count. It starts automatically
when opened, remains open while machine motion is in progress, and offers
**Run Again** only after stopping. The machine's emergency stop remains
independent of the modal, which does not cancel queued motion on close.

The feeder-wide procedure runs:

1. Move once to the feeder's **calculated** origin.
2. Find and center the optical feeder origin using existing bounded camera
   jogs and fresh stable vision proof (up to five centering corrections).
3. Read the **full 11-bar barcode** using the existing overlapping-frame
   scan/CRC-2 verification and compare exactly against the persisted four-
   character hexadecimal `StripFeeder.FeederId`. Until the comparison passes,
   the newly measured origin offset is only provisional, not saved. After
   a match, return to the measured corner and reacquire vision proof before
   saving the shared feeder origin.
4. For each installed component row in RowIndex order, run the existing typed
   four-step tape operation: first/top tape hole, last/bottom tape hole,
   current component PRESENT, previous pocket EMPTY. Rows without configured
   components or explicitly marked Empty are excluded. A current part index
   above one is necessary to verify the previous pocket.

The existing `StripFeederCalibrationRunner` now has separate `FeederOrigin`
and `InstalledTape` scopes, with versioned phase-specific operation evidence
persisted through the existing Manufacturing API. The client-only feeder
coordinator does not create a new server model or scripting language.
Successful step checkpoint writes are required before the next move;
barcode mismatch, missing vision, unsafe motion and failed persistence stop
the process. A modal Run Again is a **whole-procedure new run**, not a claim
that a hardware emergency stop or in-flight movement can be reset.

This is a first-cut integration awaiting **Windows WPF compilation and
supervised physical testing**, especially the origin→barcode→origin return,
first-to-last tape-hole calculation, and transitions across rows. No stable
NuGet publication or API deployment is required for this desktop change.

## Goal

Calibrate the physical position of a strip feeder, its first and last tape holes,
and validate the loaded/empty boundary before using the feeder to pick parts.
Use the existing camera calibration, staging-plate geometry, StripFeeder
model and pickup calculations. Keep barcode decoding independent.

This is a guided *operation* with observable evidence and deliberate state
transitions. It is NOT seven unrelated buttons and it is NOT permission to
move the machine or overwrite calibration from any background video frame.

## Steps and current building blocks

| Step | Proposed completion condition | Existing building block |
| --- | --- | --- |
| 1. Approach calculated origin | Machine idle at calculated row/column position | `GoToFeederOriginCommand`, `FindLocation(FeederOrigin)` intentionally excludes saved offset |
| 2. Center feeder optical fiducial | White-cut corner stable **and centered** after any jog, profile `feederorigin` | `CenterFoundItemCommand`, `ICornerLocatedHandler`, `MVLocatedCorner.PositionStable/Centered` |
| 3. Commit feeder origin | Read back actual machine XY after idle; calculate `OriginOffset = actual - calculated`; `SaveAsync(false)` succeeds | `SetLocation(FeederOrigin)` computes offset, but current Set button does not persist it itself |
| 4. First tape hole | Select the first usable hole **below/past** feeder origin for this row and feed direction; stable centered observation, machine XY captured | `GoToFirstFeederReferenceHoleCommand`, `SetFirstFeederReferenceHoleCommand`, tape-hole vision profile |
| 5. Last tape hole | Approach from feeder length + pitch and captured first-hole alignment; stable centered observation, machine XY captured | `FindLastFeederReferenceHoleCommand`, `SetLastFeederReferenceHoleCommand` |
| 6. Current part | Navigate without picking, verify **present** at current part index | `GoToCurrentPartCommand`, part-in-tape profile; a trustworthy presence classifier is still required |
| 7. Previous part | Navigate to index - 1 without decrementing inventory, verify **absent** | `GoToPreviousPartCommand`; a trustworthy absence classifier is still required |

Step 2's green marker currently means **PositionStable**, not necessarily
`Centered`. The operation must require both, plus the correct active profile,
selected feeder/row identity, idle/homed/manual machine, and a fresh post-jog
observation. Never equate green alone with authorization to persist a new
offset. Step 3 should *capture and save* in one explicit operator action after
those checks, not save on every video frame.

For first and last tape holes, record physical machine coordinates only after
the move finishes and stable vision confirms the center. Use the existing
orientation transformations in `SetLocation` and `FindLocation`. Persist
`StripFeederRow.FirstTapeHoleOffset` and `LastTapeHoleOffset` together after
both observations have passed, rather than destroying previous good alignment
on a partial run. Retain current feeder and row identity across the operation.

For step 6/7, movement is not a verification. Initially the operator may
confirm each camera image; represent this honestly as `OperatorConfirmed`
rather than `VisionVerified`. Add automated pocket-presence classification
only after collecting real filled/empty examples for this camera height and
part/tape configuration. Require index > 1 for the preceding-pocket check.
Do not decrement `CurrentRow.CurrentPartIndex` as a side effect of calibration.

## Small reusable operation pattern

Follow the proven `StagingPlateCalibrationRunner` approach. Start with a
`StripFeederCalibrationRunner` / presentation state owned by
`StripFeederViewModel`; do **not** create a generic scripting framework yet.

An operation needs:
- **Identity**: machine, feeder id, row index, starting current-part index,
  orientation and vision-profile identity.
- **Steps**: fixed ordered names, instructions, allowed action, completion
  evidence, explicit state (Pending, Active, AwaitingOperator, Passed,
  Blocked, Failed), and a retry action.
- **Observations**: machine XY measured *after* idle, active camera/profile,
  pixel-to-mm scale, corner/circle sample stability, centering offset, and
  operator confirmation or algorithm evidence.
- **Controls**: Start, Continue/Confirm, Retry Current Step, Cancel. Only one
  active calibration operation per machine; changing feeder, row, profile,
  machine mode or connection invalidates outstanding observations.
- **Persistence**: commit only a verified checkpoint, report save errors,
  preserve last known-good calibration after failures, and clearly distinguish
  measured values from successfully saved values.

The runner should reuse existing `ILocatorViewModel` callbacks:
`ICornerLocatedHandler` for the white corner and `ICircleLocatedHandler`
for tape holes, with bounded waits and fresh detector history after each
motion. Machine moves remain subject to manual-mode, homing, work-area and
per-move bounds. Never chain through an unverified camera target.

The UI can show a simple vertical seven-step panel on the strip feeder screen,
with one highlighted active step, current evidence, and one primary action.
In a later pass, share only the step-state/evidence/presentation primitives
with staging-plate, nozzle, and camera calibration. The operation-specific
machine motion, vision rules, and persistence stay strongly typed.

## Suggested implementation order

1. Expose a guided seven-step progress panel and stage 1/2 commands.
2. Add the gated step-3 **Capture & Save Feeder Origin**, with post-jog
   readback and existing `StripFeeder.OriginOffset` persistence.
3. Add step-4/5 stable hole centering and capture with the existing
   row geometry, saving the pair only after both succeed.
4. Add manual camera-image confirmation for steps 6/7, then a separately
   validated part-presence detector.

First get a real feeder through the operation end-to-end, then generalize
only the parts actually reused by staging plates and other calibrations.
