# Modernization Progress

Tracks status on the items catalogued in [`known-issues.md`](known-issues.md), plus any
broader modernization work on the repo. Update this file in the same change that fixes an
item — flip the status, add the date, add a one-line note (what changed / PR or commit if
relevant). Don't rewrite history here; append.

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

---

## Architecture / Framework Targeting

| ID | Summary | Priority | Status | Notes |
|---|---|---|---|---|
| [ARCH-1](known-issues.md#arch-1--migrate-nova-core-to-netstandard20) | Migrate `Nova` core (`Data/` + `IPC/Pipes/`) from net48 to `netstandard2.0`; `NovaIO` stays net8.0 for now | **#1 — top priority** | **Done** | 2026-08-18. `Nova.csproj` converted to SDK-style + `netstandard2.0`, 0 warnings/errors standalone; full `Nova.sln` build + `dotnet test` (3/3 pass) verified against real files. Packaging metadata was deferred at the time, added 2026-08-19 as part of CI-1. |

## Bugs

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [BUG-1](known-issues.md#bug-1--mgf-format-not-handled-in-filereaderopenspectrumfiles-switch) | `FileReader.OpenSpectrumFile` switch has no `MGF` case; can null-ref | High | **Done** | 2026-08-19. `FileReader.CreateReader` now has an `MGF` case; fixes both `OpenSpectrumFile` and `SpectrumFileReaderFactory.GetReader` at once (CLEAN-3 unification). |
| [BUG-2](known-issues.md#bug-2--mgfreader-is-a-non-functional-stub) | `MGFReader` always returns empty spectra | High | **Done** | 2026-08-19. Fully rewritten against the Matrix Science MGF spec; real per-spectrum parsing, tested against `Test/Files/AngioNeuro4.mgf`. |
| [BUG-3](known-issues.md#bug-3--mzmlreader-sets-analyzer--otms-instead-of-itms) | `MzMLReader` typo sets `Analyzer = "OTMS"` instead of `"ITMS"` | Medium | **Done** | 2026-08-19. One-character fix; TEST-2's characterization test flipped to assert the correct `"ITMS"` value in the same change. |
| [BUG-4](known-issues.md#bug-4--mzmlwriterwrite-hardcodes-an-absolute-schema-path) | `MzMLWriter.Write` hardcodes `D:\Data\mzML\...xsd` | Medium | **Done** | 2026-08-19. Schema validation now opt-in (`validateSchema`/`schemaPath` params, both off/null by default); `NovaApp.cs`'s call site now actually succeeds instead of throwing. |
| [BUG-5](known-issues.md#bug-5--filereaderformat-is-dead-initialized-once-never-updated) | `FileReader.Format` never assigned, permanently `Unknown` | Low | **Done** | 2026-08-19. `Format` made settable, assigned `Format = ff;` in `OpenSpectrumFile`. |
| [BUG-6](known-issues.md#bug-6--pipeioread-doesnt-handle-shortpartial-stream-reads) | `PipeIO.Read` assumes `Stream.Read` fills the buffer in one call | Medium | **Done** | 2026-08-19. Manual read loop (netstandard2.0 has no `Stream.ReadExactly`); throws `EndOfStreamException` on an early-closed pipe instead of returning truncated data. |
| [BUG-7](known-issues.md#bug-7--gitattributes-doesnt-actually-protect-line-ending-sensitive-test-fixtures) | `.gitattributes` doesn't actually stop Git from corrupting mzML/mzXML fixture byte offsets on Windows checkout | Medium | **Done** | 2026-08-19. `-text` set for both extensions; working copy renormalized; confirmed stored blobs were already correct (checkout was the only corruption point). No more `dos2unix` needed anywhere. |
| [BUG-8](known-issues.md#bug-8--filereaderopenspectrumfile-discards-the-underlying-readers-open-result) | `FileReader.OpenSpectrumFile` always returns `true`, ignoring whether the underlying reader's `Open()` actually succeeded | Medium | **Done** | 2026-08-19. `OpenSpectrumFile` now returns `Open()`'s actual result. TEST-2's two characterization tests flipped to assert `false` on a malformed/index-less file. |

## Dead / Redundant Code

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CLEAN-1](known-issues.md#clean-1--tspectrumgetmz-duplicates-its-own-boundary-check-logic) | `TSpectrum.GetMz` has an unreachable duplicated boundary-check block | Low | **Done** | 2026-08-19. Deleted the dead `if (index == 0) {...} else {...}` duplicate; behavior unchanged. |
| [CLEAN-2](known-issues.md#clean-2--thermorawreaderprocessspectruminformation-sets-fields-redundantly) | `ProcessSpectrumInformation` re-sets `spectrum` fields unconditionally after the `if/else` already did | Low | **Done** | 2026-08-19. Deleted the two redundant trailing lines. |
| [CLEAN-3](known-issues.md#clean-3--spectrumfilereaderfactory-duplicates-filereaderopenspectrumfiles-dispatch-logic) | Two independent format-dispatch implementations (`FileReader` vs `SpectrumFileReaderFactory`) that can drift | Low | **Done** | 2026-08-19. Unified into `FileReader.CheckFileFormat`/`CreateReader`; both call sites keep their own error-handling contract on top. |

## CI / Build Infrastructure

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CI-1](known-issues.md#ci-1--github-actions-workflow-diagnosed-then-replaced-entirely) | `dotnet.yml` replaced entirely with `ci.yml` + `dev-nuget.yml` + `release.yml` | — | **Done** | 2026-08-19. Root cause was HYG-1 (live, not hypothetical) plus an unpinned external checkout; see known-issues.md for the full chain. New 3-workflow structure implements goals A/B/C from repo owner, modeled on `SchweppeLab/Helios`'s `dev-nuget.yml`. Versioning scheme changed to 3-part SemVer starting at 1.1.0. |

## Hygiene / Maintainability

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [HYG-1](known-issues.md#hyg-1--stray-unused-usings-for-unrelated-packages) | Unused `using`s for ASP.NET Core / Newtonsoft.Json / VisualBasic / Tar across 5 files | Low | **Done** | 2026-08-19. Became load-bearing (not just hygiene) when the Thermo pin bumped to 8.0.37 — see CI-1 addendum. All 6 stray usings removed; verified against the failure it would've caused, then against the fix. |
| [HYG-2](known-issues.md#hyg-2--testnovas-test-data-path-resolution-is-fragile) | `TestNova` locates test files via fragile parent-directory walking | Low | **Done** | 2026-08-19. Extracted to shared `Test/TestFilePaths.cs`, anchored to `AppContext.BaseDirectory` instead of `Environment.CurrentDirectory`. |
| [HYG-3](known-issues.md#hyg-3--inconsistent-visibility-internal-interfaces-public-implementations) | `IChromatogram`/`IChromatDataPoint` are `internal` while their implementations are `public` | Low | **Done** | 2026-08-19. Both made `public`, matching `ISpectrum<T>`/`ISpecDataPoint`. |
| [HYG-4](known-issues.md#hyg-4--scattered-todos-marking-acknowledged-incomplete-features) | Collected pre-existing TODOs (asymmetric isolation windows, `GetHeader()`, trailer mapping, MGF viability) | Low | **Done** | 2026-08-19. Closed as a tracking item — its own definition is "no code change needed" (visibility only), which was already satisfied. The 3 non-MGF TODOs remain unimplemented in code by design; promote any one to its own item if it's ever picked up. |
| [HYG-5](known-issues.md#hyg-5--several-files-use-thermofishercommoncoredatas-isnullorempty-extension-as-if-it-were-project-local) | `FileReader.cs`/`MzXMLReader.cs`/`MzMLWriter.cs` use `ThermoFisher.CommonCore.Data`'s `IsNullOrEmpty` extension as if it were project-local | Low | **Done** | 2026-08-19. Added `NovaIO/StringExtensions.cs`; repointed all three files. Also removed 3 more now-genuinely-unused Thermo `using`s (verified empirically, not assumed). |

## Test Coverage

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [TEST-1](known-issues.md#test-1--no-unit-tests-for-the-nova-core-library) | No unit tests for `Nova` core (`GetMz`, serialization, Pipes IPC) | Medium | **Done** | 2026-08-19. `Test/TestSpectrum.cs` (10 `GetMz` cases + Spectrum/SpectrumEx round-trip serialization) and `Test/TestPipes.cs` (same-process connect/send/receive/disconnect). No file I/O. |
| [TEST-2](known-issues.md#test-2--no-unit-tests-for-novaio-parsing-logic-in-isolation) | No unit tests for `NovaIO` parsing logic against synthetic fixtures | Medium | **Done** | 2026-08-19. `Test/TestNovaIOFixtures.cs` against 4 new hand-built indexed fixtures in `Test/Files/` (2 valid + 2 malformed, mzML+mzXML). Directly caught BUG-3 live and surfaced a new bug, BUG-8 — both pinned as characterization tests, not fixed here. |
| [TEST-3](known-issues.md#test-3--test-output-doesnt-say-what-each-test-actually-verified) | Test output doesn't say what each test actually verified | Low | **Done** | 2026-08-19. One-line `testContext.WriteLine(...)` added to all 28 tests across all 4 test classes; `TestSpectrum`/`TestPipes`/`TestNovaIOFixtures` gained a `TestContext` (matching `TestNova.cs`'s existing property+constructor pattern) since they didn't have one before. |

---

## Suggested Order of Attack

0. ~~**ARCH-1 — Nova core to `netstandard2.0`.**~~ **Done 2026-08-18.**
1. ~~**CI-1 + BUG-7 — GitHub Actions / line-ending fixture bug.**~~ **Done 2026-08-19.**
2. ~~**TEST-1 / TEST-2.**~~ **Done 2026-08-19.** Directly caught BUG-3 live and surfaced a new
   bug, BUG-8 — both pinned as characterization tests, not fixed yet (see below).
3. ~~**BUG-3, BUG-5, BUG-8**~~ **Done 2026-08-19.**
4. ~~**CLEAN-1, CLEAN-2**~~ **Done 2026-08-19.** (~~HYG-1~~ also done 2026-08-19, ahead of
   schedule — forced by the Thermo package bump to 8.0.37, see CI-1.)
5. ~~**BUG-4, BUG-6, CLEAN-3, HYG-2, HYG-3**~~ **Done 2026-08-19.**
6. ~~**TEST-3**~~ **Done 2026-08-19.**
7. ~~**BUG-1 / BUG-2**~~ **Done 2026-08-19.** Surfaced a new low-priority finding, HYG-5.
8. ~~**HYG-4, HYG-5**~~ **Done 2026-08-19.** Everything on the list is now closed.

Everything from step 2 on is a suggestion, not a mandate — reorder freely based on what
you're actually working on next.

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
- 2026-08-18 — **ARCH-1 done.** Converted `Nova/Nova.csproj` to SDK-style targeting
  `netstandard2.0`; dropped stale Framework `<Reference>`s and the unused `packages.config`.
  Verified: standalone build 0 warnings/0 errors; full `Nova.sln` build clean (same 4
  pre-existing `NovaIO` warnings, nothing new); `dotnet test` 3/3 passing against real
  mzML/mzXML/RAW files. Found and documented BUG-7 (`.gitattributes` line-ending bug) as a
  byproduct — real issue, unrelated to ARCH-1, not fixed here.
- 2026-08-19 — **CI-1 and BUG-7 done.** Diagnosed CI-1 with hard evidence via the public
  GitHub API (no `gh` CLI needed): every run on `main` had failed for ~2 months, root cause
  HYG-1 (live, not hypothetical) triggered by an unpinned external checkout drifting onto a
  Thermo package version with a different dependency tree. Fixed BUG-7 properly (`.gitattributes`
  `-text`, working copy renormalized) rather than working around it again. Explored
  `SchweppeLab/Helios`'s `Dev`-branch `dev-nuget.yml` (repo owner's reference example,
  publicly readable even though `mhoopmann/LandmineUI` wasn't) and Nova's own past GitHub
  Releases (unzipped `v1.0.0.18` to learn the established bundle shape) before designing
  anything. Deleted `dotnet.yml` outright (repo owner: replace, don't patch) and replaced it
  with `ci.yml` (PR/main-push build+test), `dev-nuget.yml` (`Dev`-push → dev NuGet + bundle
  via GitHub Releases, dated + rolling `dev-latest`), and `release.yml` (manual-only,
  main-only, guarded against re-publishing over an already-promoted release). Pinned the
  `thermofisherlsms/RawFileReader` checkout to a specific commit in all three (was floating).
  Tightened `NovaIO.csproj`'s Thermo `PackageReference`s to exact-match `[8.0.6]`. Versioning
  scheme changed to 3-part SemVer starting at `1.1.0` (repo owner's call) — `Nova.csproj` and
  `NovaIO.csproj` both bumped and kept in lockstep; `Nova.csproj` gained full NuGet packaging
  metadata to match `NovaIO.csproj` (closes ARCH-1's deferred step 5); `Nova/Properties/AssemblyInfo.cs`
  deleted (SDK now generates it from csproj properties, same as `NovaIO`). Validated locally
  before trusting any of it in CI: restore/build/test against the exact pinned source, pack
  for both projects with a dev-style version override, confirmed `NovaIO`'s generated
  dependency on `Nova` resolves to the matching version, and rehearsed the full
  bundle-assembly PowerShell logic end-to-end (output structurally matches the real
  `v1.0.0.18` release). YAML syntax-validated with PyYAML. Nothing pushed — local commit only,
  per standing instruction never to push.
- 2026-08-19 — Repo owner pushed the CI-1 commit and watched it run: worked well. While
  watching, asked whether CI should package current/latest RawFileReader instead of the
  older 8.0.6 it was pinned to. Checked: upstream HEAD hadn't moved, so just repointing
  `Net8Old` → `Net8` at the same pinned commit. Proved it wasn't safe to just flip the
  version first — bumping `NovaIO.csproj`'s pin to `[8.0.37]` alone reproduced CI-1's exact
  failure, confirming 8.0.37 dropped the dependency HYG-1's stray usings needed. **Fixed
  HYG-1** (ahead of its scheduled turn) as a direct prerequisite, then confirmed clean
  build + `dotnet test` 3/3 against 8.0.37. Updated all three workflows' pinned source
  folder and the bundle-staging step (`Net8`'s readme is `Readme.md`, not `Net8Old`'s
  `Readme.txt` — caught by rehearsing the copy, not assuming). Kept the exact-match
  `[8.0.37]` brackets — that part of CI-1's fix was never specific to 8.0.6.
- 2026-08-19 — **TEST-1 and TEST-2 done.** Added `Test/TestSpectrum.cs` (10 `GetMz` cases
  spanning empty/single-point spectra, exact matches, and within/outside-ppm-tolerance at
  both array boundaries and mid-array — exercising every branch CLEAN-1 flags as duplicated —
  plus full-field `Serialize`/`Deserialize` round trips for `Spectrum` and `SpectrumEx`) and
  `Test/TestPipes.cs` (one same-process `PipesServer`/`PipesClient` connect, send both
  directions, disconnect test, GUID-derived server ID to survive MSTest's method-level
  parallelization). For TEST-2, hand-built four new indexed fixtures in `Test/Files/`
  (`NovaTestFixture.mzML`/`.mzXML`, 4 spectra: MS1 FTMS / MS2 ITMS+precursor / MS1 FTMS / MS2
  FTMS+precursor; `NovaTestFixtureMalformed.mzML`/`.mzXML`, same body with the index block
  omitted) via an offline Python script that computes exact byte offsets for the
  `<indexListOffset>`/`<indexOffset>` blocks `MzMLReader`/`MzXMLReader` random-access-seek
  against, since that has to be byte-exact (see BUG-7) — these fixtures are pure ASCII/LF and
  covered by the existing `-text` `.gitattributes` rules. Added `Test/TestNovaIOFixtures.cs`
  against them, going through the public `FileReader` facade since both readers are
  `internal`. This directly reproduced BUG-3 live (pinned as a characterization test, not
  fixed) and surfaced a new bug, BUG-8 (`FileReader.OpenSpectrumFile` unconditionally
  returning `true` regardless of whether the underlying reader's `Open()` actually
  succeeded), also pinned as a characterization test rather than fixed here, to keep this a
  single, reviewable, test-only change. All 28 tests (3 pre-existing + 25 new) pass:
  `dotnet build Nova/Nova.sln --configuration Release -p:Platform=x64` clean (same 4
  pre-existing warnings), `dotnet test Test/Test.csproj --configuration Release --no-build`
  from repo root, 28/28 passing.
- 2026-08-19 — **BUG-3 done.** One-character fix in `MzMLReader.ProcessCvParam`
  (`"OTMS"` → `"ITMS"`, line 770). Flipped the characterization test TEST-2 had added
  (`MzML_Ms2ItmsSpectrum_NonExPath_ReproducesBug3` → renamed
  `MzML_Ms2ItmsSpectrum_NonExPath_IsCorrect`) to assert the now-correct `"ITMS"` value.
  Verified: full solution build clean, `dotnet test` 28/28 passing.
- 2026-08-19 — Added **TEST-3** to the backlog (no code changed): make test output
  self-explanatory by adding a one/two-line `TestContext.WriteLine(...)` per test stating
  what it verifies. Moderate-low priority — queued as step 7, after the higher-value bug
  fixes and cleanup.
- 2026-08-19 — **BUG-5 and BUG-8 done**, finishing out step 3. BUG-5: `FileReader.Format`
  changed from a dead get-only auto-property to `{ get; private set; }`, assigned
  `Format = ff;` in `OpenSpectrumFile` right after the format switch resolves. BUG-8:
  `OpenSpectrumFile` now returns `fileReader.Open(fileName)`'s actual result instead of an
  unconditional `true`; deliberately left `FileName`/`ScanCount` assignment-on-failure
  behavior alone (out of this bug's scope, noted separately in known-issues.md). Flipped
  TEST-2's two malformed-file characterization tests
  (`MzML_MalformedFile_OpenDoesNotThrow` / `MzXML_MalformedFile_OpenDoesNotThrow`) from
  pinning the old buggy `true` return to asserting the correct `false`, same pattern BUG-3
  used. Verified: full solution build clean (same 4 pre-existing warnings), `dotnet test`
  28/28 passing.
- 2026-08-19 — **CLEAN-1 and CLEAN-2 done**, finishing out step 4. CLEAN-1: deleted the
  unreachable duplicated boundary-check block in `TSpectrum.GetMz`
  (`Nova/Data/ISpectrum.cs`) — the surviving "check both closest points" logic (previously
  the `else` branch of the dead duplicate) is now unconditional, since it's the only path
  left that can reach it; behavior unchanged, covered by TEST-1's existing `GetMz` boundary
  tests. CLEAN-2: deleted the two redundant trailing lines in
  `ThermoRawReader.ProcessSpectrumInformation` that unconditionally re-ran
  `RetentionTimeFromScanNumber`/`GetFilterForScanNumber` and reassigned `spectrum` (never
  `spectrumEx`) after the `if/else` above already handled both cases correctly. Verified:
  full solution build clean (same 4 pre-existing warnings), `dotnet test` 28/28 passing.
- 2026-08-19 — Repo owner made the BUG-1/BUG-2 (MGF) decision: implement real MGF support
  (not drop it), following the Matrix Science MGF format spec
  (https://www.matrixscience.com/help/data_file_help.html). Moved BUG-1/BUG-2 to the last
  item on the priority list — biggest remaining piece of work, deliberately saved for last.
  No code changed; docs only.
- 2026-08-19 — **TEST-3 done.** Added a one-line `testContext.WriteLine(...)` at the top of
  all 28 test methods across all 4 test classes (`TestSpectrum.cs`, `TestPipes.cs`,
  `TestNovaIOFixtures.cs`, and the pre-existing `TestNova.cs`), stating what each test
  verifies. `TestSpectrum`/`TestPipes`/`TestNovaIOFixtures` had no `TestContext` before this;
  added `public TestContext testContext { get; set; }` plus constructor injection, matching
  `TestNova.cs`'s existing pattern exactly — a private-field-only version was tried first and
  rejected after a clean rebuild showed 3 new `MSTEST0005` warnings (MSTest's analyzer only
  recognizes the public-property form). Confirmed the messages actually reach test output via
  a real `.trx` run (`TestContext Messages:` block per test) before calling this done, since
  the console logger silently drops `TestContext.WriteLine` for passing tests. Verified: full
  solution build clean (same 4 pre-existing warnings, 0 new), `dotnet test` 28/28 passing.
- 2026-08-19 — **BUG-4, BUG-6, CLEAN-3, HYG-2, HYG-3 done**, finishing out step 5. BUG-4:
  `MzMLWriter.Write` gained `validateSchema`/`schemaPath` params (both off/null by default),
  so it no longer touches the hardcoded `D:\Data\mzML\...xsd` path unless a caller opts in —
  `NovaApp.cs`'s existing call site now actually succeeds instead of throwing. BUG-6: added a
  manual read loop in `PipeIO.Read` (no `Stream.ReadExactly` on `netstandard2.0`), throwing
  `EndOfStreamException` on an early-closed pipe instead of silently returning truncated data.
  CLEAN-3: extracted `FileReader.CheckFileFormat` (now `static`) and a new
  `internal static CreateReader(FileFormat, MSFilter)` as the single source of truth for
  extension-to-reader dispatch; both `OpenSpectrumFile` and
  `SpectrumFileReaderFactory.GetReader` now call these, each keeping its own
  error-handling contract (BUG-1's NRE-on-MGF behavior preserved unchanged, deliberately not
  fixed here). HYG-2: extracted the duplicated `TestNova`/`TestNovaIOFixtures`
  path-resolution walk into shared `Test/TestFilePaths.cs`, anchored to
  `AppContext.BaseDirectory` instead of `Environment.CurrentDirectory` — caught a real bug
  doing this (`BaseDirectory`'s trailing separator made the first `Directory.GetParent` call
  a no-op, breaking 13/28 tests on the first attempt; fixed by trimming it first). HYG-3:
  `IChromatogram`/`IChromatDataPoint` changed from `internal` to `public`, matching
  `ISpectrum<T>`/`ISpecDataPoint`. Verified: full solution build clean (same 4 pre-existing
  warnings, 0 new), `dotnet test` 28/28 passing.
- 2026-08-19 — Mid-session ask: surface each test's `TestContext.WriteLine` message directly
  in the GitHub Actions log, not just in a downloadable `.trx` artifact (follow-up to TEST-3).
  Confirmed locally that the plain/minimal console logger drops these for passing tests, but
  `--logger "console;verbosity=detailed"` shows them under a `TestContext Messages:` block —
  added that flag (alongside the existing `--verbosity minimal`) to the `Test` step in all
  three workflows (`ci.yml`, `dev-nuget.yml`, `release.yml`).
- 2026-08-19 — **BUG-1 and BUG-2 done**, finishing out step 7 (the last item on the list).
  Repo owner supplied a real fixture, `Test/Files/AngioNeuro4.mgf` (the MS2-only subset of the
  same AngioNeuro4 acquisition already used elsewhere — 6 spectra, matching the existing
  mzML/mzXML/RAW tests' 6 MS2 scans), and pointed at the Matrix Science MGF spec
  (https://www.matrixscience.com/help/data_file_help.html) as the implementation reference.
  BUG-1: added `case FileFormat.MGF: return new MGFReader(filter);` to
  `FileReader.CreateReader` (CLEAN-3's shared dispatch helper) — fixed both
  `OpenSpectrumFile` and `SpectrumFileReaderFactory.GetReader` in one change, since the
  latter already delegates to the former. BUG-2: `MGFReader` fully rewritten. No built-in
  index (unlike mzML/mzXML), so `Open()` reads the whole file into memory once
  (`File.ReadAllLines`) and indexes every `BEGIN IONS`/`END IONS` block by line number,
  deliberately avoiding `StreamReader`-on-`FileStream` byte-offset seeking (a known footgun —
  `StreamReader`'s internal buffering desyncs `Stream.Position`). Scan numbers resolved from
  `SCANS=` where present, falling back to the common msconvert `TITLE=base.scan.scan.`
  convention (this fixture's only source, confirmed against its 6 real scan numbers: 16, 69,
  113, 179, 230, 280), falling back to sequential numbering. Implemented per the spec:
  `PEPMASS=` (one or more, → `PrecursorIon`, m/z/intensity/optional charge), `CHARGE=`
  (spectrum-local overrides file-global), `RTINSECONDS=`, and fragment peak lines
  (`m/z intensity [charge]`, third token → `SpecDataPointEx.Charge` on the `Ex` path).
  `MsLevel` is always 2 (MGF has no MS1 concept); `TotalIonCurrent`/`BasePeakMz`/base peak
  intensity/m/z range all computed from the parsed peaks, since MGF has no header fields for
  them. `GetSpectrum`/sequential reads are driven by file order via an internal scan-order
  list, not by incrementing the literal scan number the way `MzMLReader`/`MzXMLReader` do —
  necessary because this format's scan numbers are sparse, unlike mzML/mzXML/RAW's always-
  contiguous ones. Also fixed a real bug in the old stub's header loop along the way (it had
  no exit tied to its own `endOfHeader` flag, so it silently consumed the entire file instead
  of stopping at the first spectrum block).

  New finding, not fixed here: **HYG-5** — `FileReader.cs`/`MzXMLReader.cs`/`MzMLWriter.cs`
  all call an `IsNullOrEmpty()` string extension that isn't defined anywhere in this repo; it
  turns out to come from `ThermoFisher.CommonCore.Data`'s own namespace, which those files
  already `using` for unrelated reasons. Caught directly: writing the same style of call in
  `MGFReader.cs` (which needs no Thermo reference) failed to compile until either that
  `using` was added or the calls were switched to the real `string.IsNullOrEmpty(...)` — went
  with the latter in the new file. Logged as a new low-priority hygiene item; not fixed in
  the three pre-existing files, out of scope for this change.

  Added `Test/Files/AngioNeuro4Malformed.mgf` (header-only, no spectrum blocks) and
  `Test/TestMgf.cs` (9 tests: scan count/range/max RT, MS-level tally, sequential file-order
  reads, a real spectrum's fields and peak data — expected values independently computed
  from the raw file text via `awk`, not derived from the reader itself — precursor fields,
  scan-number random access, the `Ex` path, `SpectrumFileReaderFactory` no longer throwing
  for `.mgf`, and the malformed fixture returning `false` cleanly). No `.gitattributes`
  entry needed for the new `.mgf` fixtures (unlike mzML/mzXML's BUG-7) since
  `File.ReadAllLines` handles CRLF/LF transparently — there's no byte-offset fragility to
  protect. Verified: full solution build clean (0 new warnings — the rewrite actually
  *removed* 3 of the 4 previously-baseline warnings, all from the old stub's dead code),
  `dotnet test` 37/37 passing (28 pre-existing + 9 new). Updated `CLAUDE.md`'s "Known
  Gotchas" section to drop the now-fixed MGF/BUG-4 bullets and add HYG-5.
- 2026-08-19 — **HYG-4 and HYG-5 done**, finishing out step 8 and closing the whole list.
  HYG-4: closed as a tracking item rather than "fixed" in code — its own definition was
  always "no code change needed" (gather TODOs for visibility), which was already satisfied.
  The three non-MGF TODOs it lists (asymmetric isolation windows, `GetHeader()`, multi-
  precursor trailer mapping) are deliberately still unimplemented; each is separate,
  unscoped feature/design work that was never what HYG-4 itself asked for. HYG-5: added
  `NovaIO/StringExtensions.cs` (`internal static class StringExtensions`, one real
  `IsNullOrEmpty(this string? value)`) and repointed `FileReader.cs`/`MzXMLReader.cs`/
  `MzMLWriter.cs` at it. Also removed the `ThermoFisher.CommonCore.Data`/`.RawFileReader`/
  `.Data.Business` `using`s from all three files — verified empirically (deleted, rebuilt,
  confirmed nothing else broke) that none of them needed anything else from those
  namespaces, so leaving the usings in place after fixing the extension-method dependency
  would have just been a fresh copy of HYG-1's exact pattern. Verified: full solution build
  clean (same 1 pre-existing warning, 0 new), `dotnet test` 37/37 passing.
