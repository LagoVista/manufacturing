# Manufacturing calibration run history (T003)

## Purpose

Store **what was observed** during a typed machine operation, separately from
the authoritative machine/feeder calibration settings. This is not a motion
scripting engine and never picks a component or modifies `StripFeeder`,
`StripFeederRow`, inventory or machine offsets.

Each operator action records a versioned `MachineOperationRunSnapshot`:
definition key/version, machine/feeder/row/current-part identity, step progress,
timestamps, observed camera profile/scale, machine XY, whether values were
measured or confirmed saved, operator-vs-camera verification and errors.

The current `StripFeederCalibrationRunner` reports `Saved=true` **only after**
the existing Manufacturing feeder PUT acknowledges the physical calibration
setting. Saving this run-history document does not establish `Saved=true`.
A failed history PUT is reported separately and can be retried from WPF.

## Authenticated REST API

All routes require an authenticated confirmed user and are scoped to the
current user's organization, not to ownership fields supplied by the client.

| Method | Route | Purpose |
| --- | --- | --- |
| PUT | `/api/mfg/calibration/run` | Idempotently upsert one run evidence snapshot |
| GET | `/api/mfg/calibration/run/{runId}` | Fetch full durable evidence and definition |
| GET | `/api/mfg/calibration/runs/machine/{machineId}?feederId={feederId}` | Bounded, paged summaries by machine, optionally feeder |

The PUT body is `{"snapshot": <MachineOperationRunSnapshot>, "definition":
<MachineOperationDefinition>}`. The `RunId` stays stable from the C# operation
session. The Mongo/document-record `Id` is the same GUID normalized uppercase
for platform entity ID requirements. The contained `Snapshot.RunId` and
each evidence receipt retain their original identity.

The service projects and validates the snapshot, then writes a single
`MachineOperationRunRecord` through Manufacturing's normal document repository.
The GET response includes `RecordSchemaVersion`, `Definition`, `Snapshot`
and machine/feeder/row query fields. Bounded list responses use the existing
`ListRequest` pagination contract with a maximum of 50 records per page.

## Delivery and safe retries

1. The WPF guided operation records local, typed evidence as before.
2. After a guided action/retry/cancel, the view-model sends a *copy* of the
   resulting snapshot to the history endpoint. During an in-flight write, it
   disables the normal Next and Retry step actions.
3. A successful REST acknowledgement updates the history status line. Failed
   transport/server writes leave the physical calibration state unchanged,
   display a warning, and enable **Retry History Save**.
4. Re-delivery of the same or older `UpdatedAtUtc` does not roll back newer
   history. A previously passed step cannot revert, and recorded saved
   evidence cannot disappear. A new operation creates a new `RunId`.

This is an **evidence/history store**. It is not an offline durable queue: if
the server is unreachable and the WPF process exits before retry, the unsent
latest snapshot is not guaranteed to be recorded. Earlier acknowledged
snapshots remain queryable after restart. Resuming a machine motion workflow
after process restart is intentionally not implemented here.

## Testing and rollout boundaries

- Unit tests cover snapshot/definition validation, cross-organization
  protection, normalized stable identifiers, serialization of saved-vs-measured
  evidence, timestamp idempotence, stale delivery and failed physical saves.
- The existing operation-session tests also ensure steps cannot complete from
  stale/missing evidence.
- Canonical Linux .NET source/build/tests do not compile or visually validate
  the Windows-only WPF application. Test the visual history line and the
  secured read/write API in DEV after both client and Manufacturing REST
  packages are installed.
- T005 pocket verification and a real seven-step machine run remain separate
  physical acceptance gates. No headless motion or automatic picking is added.
