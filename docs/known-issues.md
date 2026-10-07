# Known Issues

Tracks currently-open bugs, dead/redundant code, and hygiene problems in this repo. When you
fix something here, update its status in [`progress.md`](progress.md) in the same change.

Severity guide: **High** = incorrect behavior or crash reachable in normal use. **Medium** =
incorrect/misleading behavior in a less common path, or a real (if narrow) correctness risk.
**Low** = code quality, dead code, or maintainability concern with no behavioral impact today.
Architecture items use **Priority** instead of Severity — they're initiatives, not defects.

**Currently open: one — BUG-12, below.** Everything from the initial 2026-08-18 review pass was resolved by
2026-08-19; the 2026-10-02 large-file pass (BUG-9, BUG-10, TEST-4) and the 2026-10-05
follow-ups (HYG-6, BUG-11, HYG-7) are resolved too. The full write-up for each — background,
decision, fix, and verification — lives in [`history.md`](history.md), organized by the same
categories as below. Add new items here as they come up; the next available ID in each
category is noted below so IDs don't collide with the archived ones.

| Category | Resolved IDs (see history.md) | Next ID |
|---|---|---|
| Architecture / Framework Targeting | ARCH-1 | ARCH-2 |
| Bugs | BUG-1 – BUG-11 | BUG-13 |
| Dead / Redundant Code | CLEAN-1 – CLEAN-3 | CLEAN-4 |
| CI / Build Infrastructure | CI-1 | CI-2 |
| Hygiene / Maintainability | HYG-1 – HYG-7 | HYG-8 |
| Test Coverage | TEST-1 – TEST-4 | TEST-5 |

---

## Open Items

### BUG-12 — `MGFReader` keeps only the first of multiple listed charge states
**Severity:** Medium · **Status: Not Started** · *Found 2026-10-07 by the repo owner during the
documentation pass; deliberately deferred until that pass is complete.*
**Location:** `NovaIO/Io/Read/MGFReader.cs` — the global `CHARGE=` header parse (~line 170), the
per-spectrum `CHARGE=` parse (~line 405), and `ParseChargeToken`

The MGF spec lets a `CHARGE=` value list several charge states, e.g. `CHARGE=2+ and 3+`,
meaning the precursor charge is undetermined and the spectrum should be considered at each
listed state. Both parse sites take only the first space-delimited token (`.Split(' ')[0]`), so
`2+ and 3+` becomes one precursor with charge 2 and the 3+ possibility is dropped. The
comma-separated form some tools emit, `CHARGE=2+,3+`, fares worse: `ParseChargeToken` trims the
trailing `+`, fails to parse `2+,3`, and yields charge **0**.

The data model already supports the right answer. `SpectrumFoundation.Precursors` is a
`List<PrecursorIon>`; a spectrum with `CHARGE=2+ and 3+` should carry two `PrecursorIon`s with
the same m/z and intensity, one with `Charge = 2` and one with `Charge = 3`.

**Suggested fix:** replace the first-token parse with one that splits on ` and ` and `,` and
returns every charge state, then emit one `PrecursorIon` per state in `ParseSpectrum`. The
global-header fallback should do the same when a block has no `CHARGE` of its own. Add fixture
blocks with `CHARGE=2+ and 3+` and `CHARGE=2+,3+` to `Test/TestMgf.cs`, asserting two
precursors each.

---

## Contributing to This List

If you find a new issue while working in this repo, add it here with the next available ID
in its category (see the table above), then add a corresponding row in
[`progress.md`](progress.md). Keep entries concrete: what's wrong, where, why it matters, and
a suggested fix — not just "this looks odd." Once an item is fixed and verified, move its
full write-up into [`history.md`](history.md) and leave just its ID in the table above (bump
the "Next ID" accordingly), so this file stays a lean, current-state view of what's actually
open.
