# November 27 calibration source consolidation

Source-only recovery for local development on master; no Forge execution or publication.

- Previous master: d13e8d42d32f898884bacb03b7dad8e89d681946
- Calibration integration: fe2f7bccef645372aa85b099902ee3c6f6a65eb7
- T014: d652566c19de67d2c397259a303ca9ab37ce310d
- T015: 2aaa6116854fc0edaa4bff4d5daa22925c5e106d

Preserve current master planning documents, REST controller changes, and stable central package pins (7.0.99 for the repository's own packages). The local PnP application and its principal manufacturing dependencies use project references. No claim that the older published package versions contain these new changes; package publication is separate.

Preserve the T014 VisionProfile additions (which include all master lines), and combine T015's tiny-ring repair diagnostic with T014's existing CircleDetector changes. Other T015 changes are retained directly. All source branch ancestry is retained. Existing Forge task status is not changed.

Native Windows compilation, focused tests and operator-supervised physical acceptance remain pending. Do not treat the merge as physical calibration approval. Confirm machine configuration, hardware readiness, and existing motion interlocks before movement.

Resume [today's T001](T001.md), using the more current [T014 verification notes](../PNP-T014-SAVED-OFFSET-VERIFICATION.md) and [nozzle runout notes](../NOZZLE-RUNOUT-CALIBRATION.md). The original T014 and T015 workstreams remain historical tracking until separately reconciled.
