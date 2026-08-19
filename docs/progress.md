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
| Bugs | 8 | 2 | 0 | 6 |
| Dead / Redundant Code | 3 | 0 | 0 | 3 |
| CI / Build Infrastructure | 1 | 1 | 0 | 0 |
| Hygiene / Maintainability | 4 | 1 | 0 | 3 |
| Test Coverage | 2 | 2 | 0 | 0 |

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
| [BUG-1](known-issues.md#bug-1--mgf-format-not-handled-in-filereaderopenspectrumfiles-switch) | `FileReader.OpenSpectrumFile` switch has no `MGF` case; can null-ref | High | Not Started | Depends on the BUG-2 decision (finish MGF or drop it) |
| [BUG-2](known-issues.md#bug-2--mgfreader-is-a-non-functional-stub) | `MGFReader` always returns empty spectra | High | Not Started | Needs a decision: finish it or remove `FileFormat.MGF` |
| [BUG-3](known-issues.md#bug-3--mzmlreader-sets-analyzer--otms-instead-of-itms) | `MzMLReader` typo sets `Analyzer = "OTMS"` instead of `"ITMS"` | Medium | **Done** | 2026-08-19. One-character fix; TEST-2's characterization test flipped to assert the correct `"ITMS"` value in the same change. |
| [BUG-4](known-issues.md#bug-4--mzmlwriterwrite-hardcodes-an-absolute-schema-path) | `MzMLWriter.Write` hardcodes `D:\Data\mzML\...xsd` | Medium | Not Started | Blocks `MzMLWriter` from working on any machine but the author's |
| [BUG-5](known-issues.md#bug-5--filereaderformat-is-dead-initialized-once-never-updated) | `FileReader.Format` never assigned, permanently `Unknown` | Low | Not Started | |
| [BUG-6](known-issues.md#bug-6--pipeioread-doesnt-handle-shortpartial-stream-reads) | `PipeIO.Read` assumes `Stream.Read` fills the buffer in one call | Medium | Not Started | Latent risk; not observed failing yet |
| [BUG-7](known-issues.md#bug-7--gitattributes-doesnt-actually-protect-line-ending-sensitive-test-fixtures) | `.gitattributes` doesn't actually stop Git from corrupting mzML/mzXML fixture byte offsets on Windows checkout | Medium | **Done** | 2026-08-19. `-text` set for both extensions; working copy renormalized; confirmed stored blobs were already correct (checkout was the only corruption point). No more `dos2unix` needed anywhere. |
| [BUG-8](known-issues.md#bug-8--filereaderopenspectrumfile-discards-the-underlying-readers-open-result) | `FileReader.OpenSpectrumFile` always returns `true`, ignoring whether the underlying reader's `Open()` actually succeeded | Medium | Not Started | Newly found 2026-08-19 while building TEST-2's malformed-fixture tests; pinned as a characterization test there |

## Dead / Redundant Code

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CLEAN-1](known-issues.md#clean-1--tspectrumgetmz-duplicates-its-own-boundary-check-logic) | `TSpectrum.GetMz` has an unreachable duplicated boundary-check block | Low | Not Started | |
| [CLEAN-2](known-issues.md#clean-2--thermorawreaderprocessspectruminformation-sets-fields-redundantly) | `ProcessSpectrumInformation` re-sets `spectrum` fields unconditionally after the `if/else` already did | Low | Not Started | |
| [CLEAN-3](known-issues.md#clean-3--spectrumfilereaderfactory-duplicates-filereaderopenspectrumfiles-dispatch-logic) | Two independent format-dispatch implementations (`FileReader` vs `SpectrumFileReaderFactory`) that can drift | Low | Not Started | |

## CI / Build Infrastructure

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [CI-1](known-issues.md#ci-1--github-actions-workflow-diagnosed-then-replaced-entirely) | `dotnet.yml` replaced entirely with `ci.yml` + `dev-nuget.yml` + `release.yml` | — | **Done** | 2026-08-19. Root cause was HYG-1 (live, not hypothetical) plus an unpinned external checkout; see known-issues.md for the full chain. New 3-workflow structure implements goals A/B/C from repo owner, modeled on `SchweppeLab/Helios`'s `dev-nuget.yml`. Versioning scheme changed to 3-part SemVer starting at 1.1.0. |

## Hygiene / Maintainability

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [HYG-1](known-issues.md#hyg-1--stray-unused-usings-for-unrelated-packages) | Unused `using`s for ASP.NET Core / Newtonsoft.Json / VisualBasic / Tar across 5 files | Low | **Done** | 2026-08-19. Became load-bearing (not just hygiene) when the Thermo pin bumped to 8.0.37 — see CI-1 addendum. All 6 stray usings removed; verified against the failure it would've caused, then against the fix. |
| [HYG-2](known-issues.md#hyg-2--testnovas-test-data-path-resolution-is-fragile) | `TestNova` locates test files via fragile parent-directory walking | Low | Not Started | |
| [HYG-3](known-issues.md#hyg-3--inconsistent-visibility-internal-interfaces-public-implementations) | `IChromatogram`/`IChromatDataPoint` are `internal` while their implementations are `public` | Low | Not Started | |
| [HYG-4](known-issues.md#hyg-4--scattered-todos-marking-acknowledged-incomplete-features) | Collected pre-existing TODOs (asymmetric isolation windows, `GetHeader()`, trailer mapping, MGF viability) | Low | Not Started | Informational; no single fix — track sub-items as they're addressed |

## Test Coverage

| ID | Summary | Severity | Status | Notes |
|---|---|---|---|---|
| [TEST-1](known-issues.md#test-1--no-unit-tests-for-the-nova-core-library) | No unit tests for `Nova` core (`GetMz`, serialization, Pipes IPC) | Medium | **Done** | 2026-08-19. `Test/TestSpectrum.cs` (10 `GetMz` cases + Spectrum/SpectrumEx round-trip serialization) and `Test/TestPipes.cs` (same-process connect/send/receive/disconnect). No file I/O. |
| [TEST-2](known-issues.md#test-2--no-unit-tests-for-novaio-parsing-logic-in-isolation) | No unit tests for `NovaIO` parsing logic against synthetic fixtures | Medium | **Done** | 2026-08-19. `Test/TestNovaIOFixtures.cs` against 4 new hand-built indexed fixtures in `Test/Files/` (2 valid + 2 malformed, mzML+mzXML). Directly caught BUG-3 live and surfaced a new bug, BUG-8 — both pinned as characterization tests, not fixed here. |

---

## Suggested Order of Attack

0. ~~**ARCH-1 — Nova core to `netstandard2.0`.**~~ **Done 2026-08-18.**
1. ~~**CI-1 + BUG-7 — GitHub Actions / line-ending fixture bug.**~~ **Done 2026-08-19.**
2. ~~**TEST-1 / TEST-2.**~~ **Done 2026-08-19.** Directly caught BUG-3 live and surfaced a new
   bug, BUG-8 — both pinned as characterization tests, not fixed yet (see below).
3. ~~**BUG-3**~~ **Done 2026-08-19.** **BUG-5, BUG-8** remain — trivial, low-risk fixes.
   BUG-8 has a characterization test in `Test/TestNovaIOFixtures.cs` whose expected value
   flips in the same change that fixes it (same pattern BUG-3 just followed).
4. **CLEAN-1, CLEAN-2** — pure cleanup, no behavior change, easy wins now that tests exist to
   confirm nothing shifted. (~~HYG-1~~ done 2026-08-19, ahead of schedule — forced by the
   Thermo package bump to 8.0.37, see CI-1.)
5. **BUG-1 / BUG-2** together — requires a real decision on MGF's fate first (see
   known-issues.md); don't fix BUG-1 without resolving BUG-2, or you'll wire up a working
   dispatch path to a reader that still silently returns nothing.
6. **BUG-4, BUG-6, CLEAN-3, HYG-2, HYG-3** — round out once the above is settled. Note BUG-6's
   fix approach depends on ARCH-1 having landed (already has — `netstandard2.0` doesn't have
   `Stream.ReadExactly`, so it needs a manual read-loop instead).

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
