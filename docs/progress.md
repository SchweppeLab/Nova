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
| Bugs | 12 | 11 | 0 | 1 |
| Dead / Redundant Code | 3 | 3 | 0 | 0 |
| CI / Build Infrastructure | 1 | 1 | 0 | 0 |
| Hygiene / Maintainability | 8 | 8 | 0 | 0 |
| Test Coverage | 4 | 4 | 0 | 0 |

_(Update this table by hand when you flip a status below — it's a quick-glance summary, not
generated.)_

**Everything from the initial 2026-08-18 review pass is Done as of 2026-08-19**, as is the
2026-10-02 large-file pass (BUG-9, BUG-10, TEST-4) and the 2026-10-05 follow-ups (HYG-6,
BUG-11, HYG-7). **One item is open: BUG-12** (MGF multiple charge states), ready to pick up now that the
documentation pass (HYG-8) is complete — see [`known-issues.md`](known-issues.md). See
[`history.md`](history.md) for the full write-up
of each resolved item and the complete session log. The tables below stay as a quick per-ID reference; new work should add new rows
here (and a matching entry in `known-issues.md`) rather than reopening the archived ones.

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
| [BUG-9](history.md#bug-9--mzmlmzxml-byte-offsets-parsed-and-stored-as-int-so-no-file-over-2-gib-could-be-read) | mzML/mzXML byte offsets parsed/stored as `int`, so no file over 2 GiB could be read | High | **Done** | 2026-10-02. Field report. All byte positions now `long` via `ByteOffset.Parse`. |
| [BUG-10](history.md#bug-10--a-failed-open-was-swallowed-so-an-unreadable-file-was-indistinguishable-from-a-missing-scan) | `GetReader` and `FileReader`'s `Read*` overloads discard the open result, yielding a reader that returns 0 peaks for every scan | High | **Done** | 2026-10-02. New `SpectrumFileOpenException`; no change to `ISpectrumFileReader`. |
| [BUG-11](history.md#bug-11--mzmlreaderclosemzxmlreaderclose-were-no-ops-and-leaked-a-file-handle) | `Close()` is a no-op in both XML readers; `XmlFS` is never disposed, leaking a handle per file switch | Medium | **Done** | 2026-10-05. Unblocked by the owner confirming nothing downstream has ever called `Close()`. |
| [BUG-12](known-issues.md#bug-12--mgfreader-keeps-only-the-first-of-multiple-listed-charge-states) | `MGFReader` keeps only the first of multiple listed `CHARGE=` states; `2+ and 3+` becomes one 2+ precursor, `2+,3+` becomes charge 0 | Medium | **Not Started** | Found 2026-10-07. Should be one `PrecursorIon` per state. Deferred until the documentation pass is done. |

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
| [HYG-6](history.md#hyg-6--framentationtypeframentationmethod-misspelled-in-public-api) | `FramentationType`/`FramentationMethod` misspelled (missing `g`) in public API | Low | **Done** | 2026-10-05. Straight rename, **breaking**; ships in 1.1.0. Wire format unaffected. |
| [HYG-7](history.md#hyg-7--inconsistent-reader-visibility-and-an-unreachable-chromatcount) | `ThermoRawReader` public while the other three readers are internal; `ChromatCount` public on an internal class and therefore unreachable | Low | **Done** | 2026-10-05. Narrowed rather than widened, **breaking**; `ChromatCount` added to `ISpectrumFileReader`. Format-forcing overload considered and deferred (no deadline). |
| [HYG-8](history.md#hyg-8--public-api-undocumented-no-xml-doc-file-shipped) | Most public members undocumented; no XML doc file built or packed, so no comment reached a package consumer | Low | **Done** | 2026-10-06/07. Every public member documented; `GenerateDocumentationFile` on; solution builds with zero warnings. Five code fixes fell out (see history.md). |

## Test Coverage

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [TEST-1](history.md#test-1--no-unit-tests-for-the-nova-core-library) | No unit tests for `Nova` core (`GetMz`, serialization, Pipes IPC) | Medium | **Done** | 2026-08-19. |
| [TEST-2](history.md#test-2--no-unit-tests-for-novaio-parsing-logic-in-isolation) | No unit tests for `NovaIO` parsing logic against synthetic fixtures | Medium | **Done** | 2026-08-19. Directly caught BUG-3 live and surfaced BUG-8. |
| [TEST-3](history.md#test-3--test-output-doesnt-say-what-each-test-actually-verified) | Test output doesn't say what each test actually verified | Low | **Done** | 2026-08-19. |
| [TEST-4](history.md#test-4--no-coverage-for-files-larger-than-2-gib) | No coverage for files larger than 2 GiB — why BUG-9 went unnoticed | Medium | **Done** | 2026-10-02. Sparse fixtures at 2.5/4.5 GiB, generated at test time, 0 bytes on disk. |

---

## Log

Append a line here each time you complete a work session on open items. The full log through
2026-08-19 (everything above) has been moved to [`history.md`](history.md#full-session-log)
to keep this file focused on whatever's currently in progress.

- 2026-10-02 — **BUG-9, BUG-10, TEST-4 done** from a field report of mzML files over 2 GiB
  failing to read. Byte offsets widened to `long` throughout `MzMLReader`/`MzXMLReader`;
  failed opens now reach the caller as `SpectrumFileOpenException` instead of a console
  print. Verified against sparse 2.5/4.5 GiB fixtures, a negative control, and a real
  3.18 GiB Astral mzML. Tests 37 → 45. Logged **BUG-11** (open). No version bump. See
  [`history.md`](history.md) for the full write-up.
- 2026-10-05 — **HYG-6 done.** Renamed the misspelled public `FramentationType` /
  `PrecursorIon.FramentationMethod` to `Fragmentation*` across all 13 occurrences. Straight
  rename with no compatibility shims, taken deliberately because 1.1.0 had not shipped yet
  and the IPC wire format never serialized the property. **This is a breaking change for
  consumers of the `Nova` package and is the one breaking change in 1.1.0 — it belongs in the
  release notes.** Build clean, `dotnet test` 45/45 passing.
- 2026-10-05 — **HYG-6 done.** Renamed the misspelled public `FramentationType` /
  `PrecursorIon.FramentationMethod` to `Fragmentation*` across all 13 occurrences. Straight
  rename with no compatibility shims, taken deliberately because 1.1.0 had not shipped yet
  and the IPC wire format never serialized the property. **This is a breaking change for
  consumers of the `Nova` package and is one of three in 1.1.0 — see the release notes.**
  Build clean, `dotnet test` 45/45 passing.
- 2026-10-05 — **1.1.0 release notes** added to `Nova.csproj` (had none) and `NovaIO.csproj`
  (still said "Initial release"), verified by packing and reading the resulting `.nuspec`.
  Corrected `README.md`'s stale "Nova needs to be in Framework 4.8" line. Flagged while
  drafting: the IPC wire format changed in 1.1.0 (`fb8dfd8` added `ScanDescription` to
  `Spectrum` serialization), so 1.1.0 and 1.0.0.18 cannot share a named pipe.
- 2026-10-05 — **BUG-11 done** plus the TEST-4 CI guard. Both XML readers now release their
  file handle in `Close()`; `Open()` releases a prior one; `chrIndex` is cleared on re-open;
  `MGFReader.Close()` drops its line buffer. Large-file tests now fail rather than skip when
  `NOVA_REQUIRE_LARGE_FILE_TESTS` is set, which all three workflows now do. Both verified
  with negative controls. Tests 45 → 48. **This closes every open item in the repo.**
- 2026-10-05 — **HYG-7 done.** API-shape question from the repo owner (should the XML readers be
  public?) resolved by narrowing instead: `ThermoRawReader` is now `internal` like the other
  three, since the interface already covered every reader's behavior except one stranded
  property. That property, `ChromatCount`, is now on `ISpectrumFileReader` and surfaced by
  `FileReader`. **Breaking**, and 1.1.0 was the one cheap moment for it; release notes updated.
  A format-forcing overload was considered in the same discussion and deliberately deferred —
  purely additive, so no deadline, and no demonstrated need. Tests 48 → 50.
- 2026-10-06 — **HYG-8 begun.** Documentation pass over `Nova/data/`, one file at a time with
  review. Four code fixes fell out (see history.md): `Deserialize` clears precursors;
  `PrecursorIon` copy/`Clear` completed and the XML readers' precursor aliasing fixed; `Baseline`
  populated from the Thermo centroid stream; `OpenSpectrumFile` closes the previous reader.
- 2026-10-07 — **HYG-8 done.** `NovaIO` and `Nova/IPC/Pipes/` documented; `GenerateDocumentationFile`
  on in both csprojs; issue-tracker prose stripped from eleven library files; every enum member
  documented; missing `<returns>` and empty tags swept; `MzXMLReader` null guard removed the last
  compiler warning. BUG-12 filed. Solution builds with zero warnings; 50/50 passing. Conventions
  in `CLAUDE.md`.
