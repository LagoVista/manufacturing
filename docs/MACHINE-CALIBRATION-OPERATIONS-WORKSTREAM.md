# PnP Guided Calibration Operations — Workstream Authority and Handoff

**Status (updated 2026-11-13):** T001–T004 are implemented and the stable
Manufacturing history API is deployed to DEV. T005 is in progress: the
seven-step feeder runner has been exercised on the machine, and a Start/Reset
procedure coordinator is now in Windows acceptance. The first five steps can
run automatically; current/previous pocket presence cannot yet be classified
reliably and must fail closed rather than invent an observation.
The remainder of this document preserves the original planning rationale.

**Authoritative Forge Workstream ID:** `pnp-guided-calibration-operations`
**Repository:** `LagoVista/Manufacturing` (master)
**Companion Workstream:** [PnP Vision and Machine Calibration](https://buildv2.nuviot.com/workstreams/pnp-vision-machine-calibration)
**Detailed feeder algorithm:** [FEEDER-CALIBRATION-OPERATION.md](FEEDER-CALIBRATION-OPERATION.md)
**Starting point:** commit `e3e2d5d5d688cd206518ad0dc058f3a830c0e4a2` on master. Re-inspect current default HEAD
before editing. Do not reset recent camera calibration, dynamic barcode decoding,
or feeder corner alignment changes.

## Revised operation UX and reuse boundary (November 13, 2026)

For the operator-facing procedure, the intended controls are **Start** and
**Reset** only. Start executes from the first step through success or a
verifiable failure, using the existing `MachineOperationDefinition`,
`MachineOperationSession`, strongly typed runners, the established camera
profiles and machine jog commands, and the Manufacturing operation history API.
Step progress, measurements and errors render from the existing session
snapshot. Normal steps do not require Next/Confirm buttons.

A small reusable `MachineOperationProcedureExecutor` sequences C# operations
and persists each checkpoint; it does **not** interpret strings as motion,
invent a scripting language, or bypass homing, work-area, profile, vision or
serial-idle checks. Reset creates a fresh session after an operation stops,
without interrupting a queued physical move. Emergency stop remains an
independent machine action.

Saving feeder origin and paired tape-hole offsets is authorized by Start
but requires new stable, in-tolerance vision evidence and a successful
Manufacturing feeder save. A failed save prevents further movement. The
current/previous pocket checks remain a missing *automatic classifier*:
the procedure moves to the current pocket, then blocks and records the gap
instead of asserting PRESENT/EMPTY. Full unattended completion must wait
for that classifier and hardware acceptance. Do not present this new
source revision as machine-tested until the Windows WPF build and physical
procedure are verified.

## Product intent

Prove one complete, operator-supervised strip-feeder calibration **operation**:
move, observe, align, verify, persist and report. Build the minimum reusable
strongly typed C# operation contract and a compact WPF progress/evidence view.
Represent definitions and execution snapshots in data-ready serializable
contracts from day one, without making motion a generic data-script engine.
Move operation-run history into the Manufacturing service/platform in a later
task once the first operation's happy path has been demonstrated.

### In-scope
- Strongly typed `IMachineOperation` (or equivalent) and serializable
  `MachineOperationDefinition`, `MachineOperationRun`, steps, measurements,
  event/evidence snapshots and explicit completion sources.
- A runner for strip feeder calibration; reuse current locator callbacks,
  machine controls, `StripFeeder.OriginOffset`, `StripFeederRow` tape-hole
  offsets and `StripFeederPickupCalculator`.
- An operator-visible seven-step panel (active step, status, action, failure
  and retry, live measurement, saved checkpoint) in the production WPF app.
- Confirmed and saved feeder origin; measured first and last tape-hole
  locations; operator-confirmed current-present/previous-empty checks.
- Durable, versioned operation-run data in the existing Manufacturing API/data
  architecture, eventually consumable by the IoT platform/MCP; no second
  competing calibration source of truth.

### Out of scope for this first workstream
- A generic declarative/scripting execution engine, arbitrary dynamic
  machine movement, background motion, autonomous picking, and AI-only
  classification of part presence.
- Implementing the *other* full calibration procedures in this workstream.
  Board fiducials, camera/tool offsets, rotational nozzle runout, and automatic
  feeder adjustment are explicit follow-on operation consumers. The existing
  `pnp-vision-machine-calibration` workstream remains the authority for its
  already-drafted camera/tool and nozzle calibration tasks.
- New edge/web runtime or production deployment.
- DevOps activities without a separately discussed infrastructure blocker.

## Strongly typed runtime, data-driven presentation

The initial common contract should expose:
`Key`, `Version`, `Title`, `TargetIdentity`, ordered `Steps`,
`CurrentStep`, `Status`, `AvailableActions`, `Evidence`,
`Execute/Confirm/Retry/Cancel`, and `LastError`.
The exact C# shapes belong to T001 implementation review.

The **C# runner** owns movement, device reads, vision gates, retries and
persistence. A **serializable description** owns title, display instructions,
measurement keys/units, validation summary, links to image/evidence, and
workflow version. A **run snapshot** records both the requested action and
the observed result; never mark a step verified solely because a G-code
command was sent. A common WPF renderer should not know feeder geometry.

Minimum step states: Pending, Active, AwaitingOperator, Passed, Blocked,
Failed, Cancelled. Distinguish `VisionVerified` from
`OperatorConfirmed`, and `Measured` from `Saved`. Every run records
machine, selected feeder, row, starting index, profile, scale, timestamp,
active operation version, and proof of any committed physical offset.

## First concrete operation: Strip Feeder Calibration

| Step | Required proof / durable checkpoint |
| --- | --- |
| 1. Move to calculated feeder origin | Position camera at row/column + mounting-hole geometry, **ignoring previous OriginOffset** for the approach; machine idle |
| 2. Jog to white-cut fiducial | Correct `feederorigin` profile; new stable AND centered corner after the jog; use `EffectivePixelsPerMM`, not START-bar width |
| 3. Capture & save `OriginOffset` | Read actual machine XY after idle, compute delta against calculated origin, require selected feeder unchanged; explicit confirm + successful backend save |
| 4. Capture first tape hole | Locate first applicable hole past origin for tape/feed orientation; stable centered observation; preserve captured machine XY |
| 5. Capture last tape hole | Approach from length/pitch, center and capture; commit BOTH tape-hole offsets only after both observations pass |
| 6. Verify current pocket has a part | Move to current index WITHOUT picking, operator confirms present on captured camera image |
| 7. Verify preceding pocket empty | Move to index-1 ONLY if index>1, operator confirms absent; DO NOT change inventory index |

Existing primitives: `StagingPlateCalibrationRunner` (machine/vision
supervision), `ICornerLocatedHandler`, `ICircleLocatedHandler`,
`StripFeederViewModel` (`GoToFeederOriginCommand`,
`SetFeederOriginCommand`, `GoToFirstFeederReferenceHoleCommand`,
`FindLastFeederReferenceHoleCommand`, `GoToCurrentPartCommand`,
`GoToPreviousPartCommand`) and `ImageCaptureService.CenterFoundItem`.
The camera's green crosshair means stable, **not** necessarily centered.

Keep the START-bar-relative scale only for barcode decoding. Existing
recent calibration UI fixes let a profile override nullable px/mm from its
camera baseline. Never reintroduce a barcode-derived physical jog scale.

## Proposed Planner task plan (task identities are Forge-owned)

1. **Operation contracts + evidence DTOs.** Small C# contract, serializable
   definition/run/snapshot types, deterministic step state transitions and
   contract tests. No source-driven runtime motions.
2. **Feeder steps 1–3 + operator progress view.** Integrate the C# runner,
   conditional motion, stable + centered corner gate, explicit save to the
   authoritative `StripFeeder.OriginOffset`; first end-to-end checkpoint.
3. **Durable execution records.** Persist operation-run and step evidence in
   the Manufacturing backend, read-side history and clear saved-vs-measured
   state. Make the output ready for later IoT/MCP consumers.
4. **Feeder first/last tape-hole alignment.** Steps 4–5 reuse current math and
   profiles; capture and persist both holes atomically after successful
   measurement.
5. **Filled/empty boundary and full supervised rehearsal.** Steps 6–7
   operator confirmations, no automatic picking/inventory mutation; desktop
   tests and on-machine evidence, retry/cancel; document remaining gaps.

Tasks should be created as **draft** and reviewed before ready/assignment.
No DevOps activities, workspace provisioning, branches, code execution,
publishing or deployment is implied by planning. Avoid duplicating existing
tasks in the companion calibration workstream.

## Definition of done

- At least one real strip feeder completes all seven steps with visible
  evidence and explicit saved outcomes; no silent step advancement.
- Camera movement remains bounded and operator-supervised; recalibration
  doesn't corrupt previous saved values after a partial or failed run.
- The C# contract and serialized run model can describe at least the
  forthcoming board-fiducial, camera/tool-offset and nozzle-runout operations
  without making their physical control dynamic.
- The WPF progress view is usable on the machine's existing window and
  preserves the manual calibration tools.
- Run history is readable via the authoritative Manufacturing API or a
  clearly documented incremental migration path; no new independent store.
- Focused tests cover state transitions, verification gates, geometry,
  index invariants, and persistence failure behavior.
- No build/publish/deploy without the normal Workstream proof and approval.

## Re-entry / session-handoff protocol

1. Look up Forge Workstream `pnp-guided-calibration-operations` and list its
   tasks; use **Forge data**, not a remembered list, for current lifecycle.
2. Read this file and `docs/FEEDER-CALIBRATION-OPERATION.md`. Inspect
   current `LagoVista/Manufacturing` master HEAD, latest Task workspace
   status and unresolved attention before source changes.
3. Resume the first active/ready task (if any). If all tasks remain draft,
   review and ready the first task, then provision/assign it separately.
4. Resume T005's Start/Reset procedure acceptance against the current
   Task workspace commit and Windows WPF application. Do not overwrite
   the completed T001–T004 work or assume latest source is merged.
5. The seven-step runner exists and is machine-tested in manual form; the
   new Start/Reset coordinator is not yet machine-tested. Do not infer
   automatic vision-based pocket verification.
6. For physical validation, use a single known feeder/row and current
   camera calibration at pick height; capture measured position and a
   small, safe jog before any new autonomous move.
7. At session end, commit code/docs through the approved Workstream task
   workflow and put exact observed outcome, source commit, blockers and
   next action into durable Task/Workstream notes.

### Current known working baseline (November 12, 2026)

- White-cut corner finder and jog use active camera/profile px/mm, with
  observed centered image around X +0.01, Y -0.01 mm at 90.9 px/mm.
- The camera/profile calibration UI, decimal-entry fix and Calibration
  subtabs were previously committed to master.
- The corner overlay typography was reduced and moved down slightly in
  commit `e3e2d5d5d688cd206518ad0dc058f3a830c0e4a2`; the seven-step runner is **not yet implemented**.
- Existing `StagingPlateCalibrationRunner` is a concrete operation-like
  precedent; `BoardAlignmentManager` and `NozzleTipCalibrationViewModel`
  are follow-on foundations, with historical hard-coded 20 px/mm conversions
  that must be revised before live calibration use.

## Related pointers

- [Feeder calibration technical design](FEEDER-CALIBRATION-OPERATION.md)
- [Vision prototype and barcode split](FEEDER-BARCODE-PROTOTYPE.md)
- [Staging plate runner](../src/LagoVista.PickAndPlace/ViewModels/Machine/StagingPlateCalibrationRunner.cs)
- [Board alignment manager](../src/LagoVista.PickAndPlace/Managers/PcbFab/BoardAlignmentManager.cs)
- [Nozzle calibration view-model](../src/LagoVista.PickAndPlace/ViewModels/PickAndPlace/NozzleTipCalibrationViewModel.cs)
- [Build Server Planner plan contract](https://github.com/softwarelogistics/build-server-v2/blob/main/docs/PLANNER-PLAN-CONTRACT.md)
