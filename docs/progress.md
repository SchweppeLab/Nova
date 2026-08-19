# Modernization Progress

Tracks status on the items catalogued in [`known-issues.md`](known-issues.md), plus any
broader modernization work on the repo. Update this file in the same change that fixes an
item — flip the status, add the date, add a one-line note. Don't rewrite history here;
append. For the full detailed write-up of any resolved item (background, decision, fix,
verification) and the complete session-by-session log, see [`history.md`](history.md).

**Status legend:** `Not Started` · `In Progress` · `Done` · `Won't Fix` (with reason)

## Summary

| Category | Total | Done | In Progress | Not Started |
|---|---|---|---|---|
| Architecture / Framework Targeting | 1 | 1 | 0 | 0 |
| Bugs | 8 | 8 | 0 | 0 |
| Dead / Redundant Code | 3 | 3 | 0 | 0 |
| CI / Build Infrastructure | 1 | 1 | 0 | 0 |
| Hygiene / Maintainability | 5 | 5 | 0 | 0 |
| Test Coverage | 3 | 3 | 0 | 0 |

_(Update this table by hand when you flip a status below — it's a quick-glance summary, not
generated.)_

**Everything from the initial 2026-08-18 review pass is Done as of 2026-08-19.** See
[`history.md`](history.md) for the full write-up of each item and the complete session log.
The tables below stay as a quick per-ID reference; new work should add new rows here (and a
matching entry in `known-issues.md`) rather than reopening the archived ones.

---

## Architecture / Framework Targeting

| ID | Summary | Priority | Status | Notes |
|---|---|---|---|---|
| [ARCH-1](history.md#arch-1--migrate-nova-core-to-netstandard20) | Migrate `Nova` core (`Data/` + `IPC/Pipes/`) from net48 to `netstandard2.0`; `NovaIO` stays net8.0 for now | **#1 — top priority** | **Done** | 2026-08-18/19. See history.md for full detail. |

## Bugs

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [BUG-1](history.md#bug-1--mgf-format-not-handled-in-filereaderopenspectrumfiles-switch) | `FileReader.OpenSpectrumFile` switch has no `MGF` case; can null-ref | High | **Done** | 2026-08-19. |
| [BUG-2](history.md#bug-2--mgfreader-is-a-non-functional-stub) | `MGFReader` always returns empty spectra | High | **Done** | 2026-08-19. |
| [BUG-3](history.md#bug-3--mzmlreader-sets-analyzer--otms-instead-of-itms) | `MzMLReader` typo sets `Analyzer = "OTMS"` instead of `"ITMS"` | Medium | **Done** | 2026-08-19. |
| [BUG-4](history.md#bug-4--mzmlwriterwrite-hardcodes-an-absolute-schema-path) | `MzMLWriter.Write` hardcodes `D:\Data\mzML\...xsd` | Medium | **Done** | 2026-08-19. |
| [BUG-5](history.md#bug-5--filereaderformat-is-dead-initialized-once-never-updated) | `FileReader.Format` never assigned, permanently `Unknown` | Low | **Done** | 2026-08-19. |
| [BUG-6](history.md#bug-6--pipeioread-doesnt-handle-shortpartial-stream-reads) | `PipeIO.Read` assumes `Stream.Read` fills the buffer in one call | Medium | **Done** | 2026-08-19. |
| [BUG-7](history.md#bug-7--gitattributes-doesnt-actually-protect-line-ending-sensitive-test-fixtures) | `.gitattributes` doesn't actually stop Git from corrupting mzML/mzXML fixture byte offsets on Windows checkout | Medium | **Done** | 2026-08-19. |
| [BUG-8](history.md#bug-8--filereaderopenspectrumfile-discards-the-underlying-readers-open-result) | `FileReader.OpenSpectrumFile` always returns `true`, ignoring whether the underlying reader's `Open()` actually succeeded | Medium | **Done** | 2026-08-19. |

## Dead / Redundant Code

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CLEAN-1](history.md#clean-1--tspectrumgetmz-duplicates-its-own-boundary-check-logic) | `TSpectrum.GetMz` has an unreachable duplicated boundary-check block | Low | **Done** | 2026-08-19. |
| [CLEAN-2](history.md#clean-2--thermorawreaderprocessspectruminformation-sets-fields-redundantly) | `ProcessSpectrumInformation` re-sets `spectrum` fields unconditionally after the `if/else` already did | Low | **Done** | 2026-08-19. |
| [CLEAN-3](history.md#clean-3--spectrumfilereaderfactory-duplicates-filereaderopenspectrumfiles-dispatch-logic) | Two independent format-dispatch implementations (`FileReader` vs `SpectrumFileReaderFactory`) that can drift | Low | **Done** | 2026-08-19. |

## CI / Build Infrastructure

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CI-1](history.md#ci-1--github-actions-workflow-diagnosed-then-replaced-entirely) | `dotnet.yml` replaced entirely with `ci.yml` + `dev-nuget.yml` + `release.yml` | — | **Done** | 2026-08-19. Includes a follow-up fix to `dev-nuget.yml`'s `dev-latest` asset naming. |

## Hygiene / Maintainability

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [HYG-1](history.md#hyg-1--stray-unused-usings-for-unrelated-packages) | Unused `using`s for ASP.NET Core / Newtonsoft.Json / VisualBasic / Tar across 5 files | Low | **Done** | 2026-08-19. |
| [HYG-2](history.md#hyg-2--testnovas-test-data-path-resolution-is-fragile) | `TestNova` locates test files via fragile parent-directory walking | Low | **Done** | 2026-08-19. |
| [HYG-3](history.md#hyg-3--inconsistent-visibility-internal-interfaces-public-implementations) | `IChromatogram`/`IChromatDataPoint` are `internal` while their implementations are `public` | Low | **Done** | 2026-08-19. |
| [HYG-4](history.md#hyg-4--scattered-todos-marking-acknowledged-incomplete-features) | Collected pre-existing TODOs (asymmetric isolation windows, `GetHeader()`, trailer mapping, MGF viability) | Low | **Done** | 2026-08-19. Closed as a tracking item; 3 non-MGF TODOs remain unimplemented by design. |
| [HYG-5](history.md#hyg-5--several-files-use-thermofishercommoncoredatas-isnullorempty-extension-as-if-it-were-project-local) | `FileReader.cs`/`MzXMLReader.cs`/`MzMLWriter.cs` use `ThermoFisher.CommonCore.Data`'s `IsNullOrEmpty` extension as if it were project-local | Low | **Done** | 2026-08-19. |

## Test Coverage

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [TEST-1](history.md#test-1--no-unit-tests-for-the-nova-core-library) | No unit tests for `Nova` core (`GetMz`, serialization, Pipes IPC) | Medium | **Done** | 2026-08-19. |
| [TEST-2](history.md#test-2--no-unit-tests-for-novaio-parsing-logic-in-isolation) | No unit tests for `NovaIO` parsing logic against synthetic fixtures | Medium | **Done** | 2026-08-19. Directly caught BUG-3 live and surfaced BUG-8. |
| [TEST-3](history.md#test-3--test-output-doesnt-say-what-each-test-actually-verified) | Test output doesn't say what each test actually verified | Low | **Done** | 2026-08-19. |

---

## Log

Append a line here each time you complete a work session on open items. The full log through
2026-08-19 (everything above) has been moved to [`history.md`](history.md#full-session-log)
to keep this file focused on whatever's currently in progress.
