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
| Bugs | 7 | 1 | 0 | 6 |
| Dead / Redundant Code | 3 | 0 | 0 | 3 |
| CI / Build Infrastructure | 1 | 1 | 0 | 0 |
| Hygiene / Maintainability | 4 | 0 | 0 | 4 |
| Test Coverage | 2 | 0 | 0 | 2 |

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
| [BUG-3](known-issues.md#bug-3--mzmlreader-sets-analyzer--otms-instead-of-itms) | `MzMLReader` typo sets `Analyzer = "OTMS"` instead of `"ITMS"` | Medium | Not Started | One-line fix; add regression test alongside TEST-2 |
| [BUG-4](known-issues.md#bug-4--mzmlwriterwrite-hardcodes-an-absolute-schema-path) | `MzMLWriter.Write` hardcodes `D:\Data\mzML\...xsd` | Medium | Not Started | Blocks `MzMLWriter` from working on any machine but the author's |
| [BUG-5](known-issues.md#bug-5--filereaderformat-is-dead-initialized-once-never-updated) | `FileReader.Format` never assigned, permanently `Unknown` | Low | Not Started | |
| [BUG-6](known-issues.md#bug-6--pipeioread-doesnt-handle-shortpartial-stream-reads) | `PipeIO.Read` assumes `Stream.Read` fills the buffer in one call | Medium | Not Started | Latent risk; not observed failing yet |
| [BUG-7](known-issues.md#bug-7--gitattributes-doesnt-actually-protect-line-ending-sensitive-test-fixtures) | `.gitattributes` doesn't actually stop Git from corrupting mzML/mzXML fixture byte offsets on Windows checkout | Medium | **Done** | 2026-08-19. `-text` set for both extensions; working copy renormalized; confirmed stored blobs were already correct (checkout was the only corruption point). No more `dos2unix` needed anywhere. |

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

0. ~~**ARCH-1 — Nova core to `netstandard2.0`.**~~ **Done 2026-08-18.**
1. ~~**CI-1 + BUG-7 — GitHub Actions / line-ending fixture bug.**~~ **Done 2026-08-19.**
2. **TEST-1 / TEST-2** next, or at least started — every fix below is safer to make once
   there's a test harness that isn't limited to one real integration file.
3. **BUG-3, BUG-5** — trivial, low-risk, high-value fixes.
4. **CLEAN-1, CLEAN-2, HYG-1** — pure cleanup, no behavior change, easy wins once tests exist
   to confirm nothing shifted. HYG-1 in particular is no longer just hygiene — see CI-1;
   fixing it removes the exact landmine that broke CI for two months.
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
