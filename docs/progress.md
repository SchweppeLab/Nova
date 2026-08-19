# Modernization Progress

Tracks status on the items catalogued in [`known-issues.md`](known-issues.md), plus any
broader modernization work on the repo. Update this file in the same change that fixes an
item — flip the status, add the date, add a one-line note (what changed / PR or commit if
relevant). Don't rewrite history here; append.

**Status legend:** `Not Started` · `In Progress` · `Done` · `Won't Fix` (with reason)

## Summary

| Category | Total | Done | In Progress | Not Started |
|---|---|---|---|---|
| Architecture / Framework Targeting | 1 | 0 | 0 | 1 |
| Bugs | 6 | 0 | 0 | 6 |
| Dead / Redundant Code | 3 | 0 | 0 | 3 |
| CI / Build Infrastructure | 1 | 0 | 0 | 1 |
| Hygiene / Maintainability | 4 | 0 | 0 | 4 |
| Test Coverage | 2 | 0 | 0 | 2 |

_(Update this table by hand when you flip a status below — it's a quick-glance summary, not
generated.)_

---

## Architecture / Framework Targeting

| ID | Summary | Priority | Status | Notes |
|---|---|---|---|---|
| [ARCH-1](known-issues.md#arch-1--migrate-nova-core-to-netstandard20) | Migrate `Nova` core (`Data/` + `IPC/Pipes/`) from net48 to `netstandard2.0`; `NovaIO` stays net8.0 for now | **#1 — top priority** | Not Started | Set by repo owner 2026-08-18. Unblocks Helios; `NovaIO` multi-targeting deferred (see known-issues.md) |

## Bugs

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [BUG-1](known-issues.md#bug-1--mgf-format-not-handled-in-filereaderopenspectrumfiles-switch) | `FileReader.OpenSpectrumFile` switch has no `MGF` case; can null-ref | High | Not Started | Depends on the BUG-2 decision (finish MGF or drop it) |
| [BUG-2](known-issues.md#bug-2--mgfreader-is-a-non-functional-stub) | `MGFReader` always returns empty spectra | High | Not Started | Needs a decision: finish it or remove `FileFormat.MGF` |
| [BUG-3](known-issues.md#bug-3--mzmlreader-sets-analyzer--otms-instead-of-itms) | `MzMLReader` typo sets `Analyzer = "OTMS"` instead of `"ITMS"` | Medium | Not Started | One-line fix; add regression test alongside TEST-2 |
| [BUG-4](known-issues.md#bug-4--mzmlwriterwrite-hardcodes-an-absolute-schema-path) | `MzMLWriter.Write` hardcodes `D:\Data\mzML\...xsd` | Medium | Not Started | Blocks `MzMLWriter` from working on any machine but the author's |
| [BUG-5](known-issues.md#bug-5--filereaderformat-is-dead-initialized-once-never-updated) | `FileReader.Format` never assigned, permanently `Unknown` | Low | Not Started | |
| [BUG-6](known-issues.md#bug-6--pipeioread-doesnt-handle-shortpartial-stream-reads) | `PipeIO.Read` assumes `Stream.Read` fills the buffer in one call | Medium | Not Started | Latent risk; not observed failing yet |

## Dead / Redundant Code

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CLEAN-1](known-issues.md#clean-1--tspectrumgetmz-duplicates-its-own-boundary-check-logic) | `TSpectrum.GetMz` has an unreachable duplicated boundary-check block | Low | Not Started | |
| [CLEAN-2](known-issues.md#clean-2--thermorawreaderprocessspectruminformation-sets-fields-redundantly) | `ProcessSpectrumInformation` re-sets `spectrum` fields unconditionally after the `if/else` already did | Low | Not Started | |
| [CLEAN-3](known-issues.md#clean-3--spectrumfilereaderfactory-duplicates-filereaderopenspectrumfiles-dispatch-logic) | Two independent format-dispatch implementations (`FileReader` vs `SpectrumFileReaderFactory`) that can drift | Low | Not Started | |

## CI / Build Infrastructure

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CI-1](known-issues.md#ci-1--investigate-and-fix-github-actions-workflow) | Investigate and fix GitHub Actions workflow (`dotnet.yml`) | TBD | Not Started | Requested by user 2026-08-18; no failure diagnosed yet, needs run history/logs |

## Hygiene / Maintainability

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [HYG-1](known-issues.md#hyg-1--stray-unused-usings-for-unrelated-packages) | Unused `using`s for ASP.NET Core / Newtonsoft.Json / VisualBasic / Tar across 4 files | Low | Not Started | Only resolves today via Thermo package's transitive deps |
| [HYG-2](known-issues.md#hyg-2--testnovas-test-data-path-resolution-is-fragile) | `TestNova` locates test files via fragile parent-directory walking | Low | Not Started | |
| [HYG-3](known-issues.md#hyg-3--inconsistent-visibility-internal-interfaces-public-implementations) | `IChromatogram`/`IChromatDataPoint` are `internal` while their implementations are `public` | Low | Not Started | |
| [HYG-4](known-issues.md#hyg-4--scattered-todos-marking-acknowledged-incomplete-features) | Collected pre-existing TODOs (asymmetric isolation windows, `GetHeader()`, trailer mapping, MGF viability) | Low | Not Started | Informational; no single fix — track sub-items as they're addressed |

## Test Coverage

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [TEST-1](known-issues.md#test-1--no-unit-tests-for-the-nova-core-library) | No unit tests for `Nova` core (`GetMz`, serialization, Pipes IPC) | Medium | Not Started | Do before/alongside riskier fixes above, so regressions get caught |
| [TEST-2](known-issues.md#test-2--no-unit-tests-for-novaio-parsing-logic-in-isolation) | No unit tests for `NovaIO` parsing logic against synthetic fixtures | Medium | Not Started | Would have caught BUG-3 directly |

---

## Suggested Order of Attack

0. **ARCH-1 — Nova core to `netstandard2.0`.** Explicit top priority set by the repo owner
   2026-08-18; supersedes the default ordering below. Do this first.
1. **TEST-1 / TEST-2** next, or at least started — every fix below is safer to make once
   there's a test harness that isn't limited to one real integration file. Also worth doing
   right after ARCH-1 specifically because a target-framework change is exactly the kind of
   thing you want a safety net in place for before touching further.
2. **BUG-3, BUG-5** — trivial, low-risk, high-value fixes.
3. **CLEAN-1, CLEAN-2, HYG-1** — pure cleanup, no behavior change, easy wins once tests exist
   to confirm nothing shifted.
4. **BUG-1 / BUG-2** together — requires a real decision on MGF's fate first (see
   known-issues.md); don't fix BUG-1 without resolving BUG-2, or you'll wire up a working
   dispatch path to a reader that still silently returns nothing.
5. **BUG-4, BUG-6, CLEAN-3, HYG-2, HYG-3** — round out once the above is settled. Note BUG-6's
   fix approach depends on ARCH-1 having landed first (`Stream.ReadExactly` isn't available
   on `netstandard2.0`, see ARCH-1's notes).

Everything below ARCH-1 is a suggestion, not a mandate — reorder freely based on what you're
actually working on next.

## Log

Append a line here each time you complete a work session on this list, even a short one —
it's the changelog for this document.

- 2026-08-18 — Initial review pass completed; `known-issues.md` and this tracker created.
  No fixes applied yet.
- 2026-08-18 — Added `.gitignore`; untracked two accidentally-committed `obj/` files;
  created `Dev` branch (all work happens here from now on, not `main`); added CI-1
  (investigate/fix GitHub Actions) to the todo list.
- 2026-08-18 — Discussed framework-targeting strategy (prompted by a Helios-side note in
  `D:\Software\Claude\Notes` and a check of Thermo's `RawFileReader` package structure in
  `D:\Software\Claude\RawFileReader`). Decision: migrate `Nova` core to `netstandard2.0`,
  leave `NovaIO` on `net8.0` for now. Added as ARCH-1, set as #1 priority. No code changed
  yet — planning only.
