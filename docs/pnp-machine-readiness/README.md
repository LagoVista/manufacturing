# Pick-and-place machine readiness — November 27, 2026

Status: planning; T001–T010 are new identifiers scoped to this workstream. Existing calibration workstreams and their task IDs remain unchanged.

## Coordination and implementation

The coordination session owns prioritization, scope decisions and acceptance. A separate implementation session takes one agreed task at a time, reads its task document, checks current repository/working-machine state, and reports changes, tests, on-machine observations, remaining blockers and next actions. Do not assume completion from previous planning notes. No source changes, publication, deployment or machine motion are authorized by this planning commit.

Keep this pragmatic: reuse existing models, APIs, WPF tools, vision profiles and typed calibration operations where they fit. Avoid new infrastructure unless necessary. Engineering completion and physical acceptance are distinct; operator-supervised motion only, with existing homing, idle, clearance and safety interlocks. Verify observations before saving, preserve prior good settings on failure, and do not use stale vision data. Use actual camera/profile calibration for physical conversions.

## Task list

| ID | Task | Status |
| --- | --- | --- |
| [T000.5](T000.5.md) | Automatic vision threshold tuning | Accepted / closed |
| [T000.6](T000.6.md) | Extend automatic thresholding to additional vision detectors | Authorized |
| [T001](T001.md) | Finish tool head / camera offset calibration | Accepted / closed |
| [T001.5](T001.5.md) | On-demand top-camera illumination | Accepted / closed |
| [T002](T002.md) | Board fiducial calibration | Accepted / closed |
| [T003](T003.md) | Feeder code label printing | Accepted / closed |
| [T004](T004.md) | X gantry discussion | Accepted / closed (production follow-up deferred) |
| [T005](T005.md) | Auto feeder control panel | Accepted / closed |
| [T006](T006.md) | Auto feeder placement calibration | Integrated; physical verification pending |
| [T007](T007.md) | Auto feeder part location and presence calibration | Planned |
| [T008](T008.md) | Part inspection calibration (bottom camera) | Planned |
| [T009](T009.md) | Part on board calibration | Planned |
| [T010](T010.md) | AI job preparation | Planned |

Begin with T001. T003 and T004 can be discussed or implemented independently when agreed. Dependencies in individual tasks are proposed physical acceptance gates, not a requirement to serialize all engineering work. Confirm tolerances with the operator rather than inventing precision requirements.

## Existing references

- [Guided calibration workstream](../MACHINE-CALIBRATION-OPERATIONS-WORKSTREAM.md)
- [Feeder calibration operation](../FEEDER-CALIBRATION-OPERATION.md)
- [Feeder barcode prototype](../FEEDER-BARCODE-PROTOTYPE.md)
- [Calibration run history](../CALIBRATION-RUN-HISTORY.md)

## Task handoff

For each task: read its scope and the relevant existing code, confirm the intended behavior and physical test with the coordination session where needed, implement only that task, and return a concise review with source commit/PR, test results, operator evidence, and remaining issues. Update the task status here only when accepted. Do not reopen or replace older workstreams.
