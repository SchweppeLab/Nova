# CLAUDE.md

Context for Claude Code (or any future contributor) working in the Nova repository.

## What Nova Is

Nova is a lightweight C# library for mass spectrometry (MS) file reading and spectral
data management, developed by the Schweppe Lab (University of Washington). It provides:

- A format-agnostic spectrum/chromatogram data model
- Readers for common MS file formats (Thermo RAW, mzML, mzXML, MGF)
- An mzML writer
- A named-pipes IPC layer for real-time communication with client processes (e.g. real-time
  acquisition software built on Thermo's IAPI)

Citation: Hoopmann, M. R.; McGann, C. D.; Rose, C. M.; Schweppe, D. K. "Nova: A Library for
Rapid Development of Mass Spectrometry Software Applications." *J. Am. Soc. Mass Spectrom.*
2025, 36(8), 1836-1839.

Published docs: https://schweppelab.github.io/Nova/ (built from the separate `gh-pages`
branch — unrelated to the `docs/` folder in this branch, which is repo-local working
documentation, not the published site).

## Repo Layout

```
Nova/               netstandard2.0 SDK-style library — core data model + IPC, packable
  Data/              Spectrum/Chromatogram/Precursor data types
  IPC/Pipes/         Named-pipe client/server (forked from acdvorak/named-pipe-wrapper, MIT)
NovaIO/              .NET 8 SDK-style library — file I/O, depends on Nova, packable as Nova.IO
  Io/Read/           FileReader facade + format readers (ThermoRaw, mzML, mzXML, MGF)
  Io/Write/          MzMLWriter + a small hand-rolled XML element tree (NovaXmlElement)
  Io/Meta/           Thermo trailer-label -> MetaClass lookup (MetaDictionary)
NovaApp/              .NET 8 console demo/smoke-test app
Test/                 .NET 8 MSTest project — integration-style tests against Test/Files/AngioNeuro4.*
Examples/             Separate solution (NovaExamples.sln): WinForms/console samples
                       (Protostar, ScanBroadcaster, ScanReceiver, ScanViewer)
docs/                 Repo-local documentation (known issues, progress tracking, and a
                       history.md archive of fully-resolved work with full detail)
.github/workflows/    ci.yml (PR/main-push build+test), dev-nuget.yml (Dev-push dev NuGet
                       releases), release.yml (manual, main-only, official releases),
                       jekyll.yml (deploys gh-pages docs site)
```

## Build & Test

Solution: `Nova/Nova.sln`.

```
dotnet build Nova/Nova.sln --configuration Release -p:Platform=x64
```

- Only `Release|x64` is exercised in practice. Don't spend time chasing Debug-config build
  issues unless specifically asked — treat Debug as unmaintained here.
- **The solution builds with zero warnings** (as of 2026-10-07, HYG-8). Both csprojs set
  `GenerateDocumentationFile`, so `Nova.xml`/`NovaIO.xml` are built and packed, and the compiler
  checks every `cref` (CS1574) and flags every undocumented public member (CS1591). A build that
  introduces a warning has regressed something; don't suppress it, fix it.
- `Nova` (core: `Data/` + `IPC/Pipes/`) targets **`netstandard2.0`** as of 2026-08-18 (ARCH-1,
  done — see `docs/history.md` for the full rationale). It was
  previously .NET Framework 4.8; the retarget was done so one build serves both net48
  consumers (e.g. Helios, pinned to net48 by Thermo's IAPI) and net8+ consumers, without
  forking into separate packages. `NovaIO`, `NovaApp`, and `Test` target **.NET 8** and are
  staying there for now — `NovaIO` multi-targeting to also support net48 was investigated and
  found technically feasible (Thermo ships parallel net48/net8.0 builds of the RawFileReader
  packages), but is deliberately deferred, not abandoned. Don't retarget `NovaIO` without
  discussing it first.
- Run tests: `dotnet test Test/Test.csproj`. Test fixtures under `Test/Files/` are located via
  `Test/TestFilePaths.cs`, anchored to `AppContext.BaseDirectory` (fixed as HYG-2, see
  `docs/history.md`) rather than the invocation directory, so it no longer matters where you
  run the command from. Tests span both integration-style reads against real files in
  `Test/Files/` (mzML, mzXML, RAW, and MGF versions of the same acquisition — scan counts and
  MS-level tallies) and unit-style tests against small synthetic fixtures for
  `Nova` core (`TestSpectrum.cs`, `TestPipes.cs`) and `NovaIO` parsing logic in isolation
  (`TestNovaIOFixtures.cs`, `TestMgf.cs`) — added as TEST-1/TEST-2, see `docs/history.md` — plus
  `TestLargeFiles.cs`, which generates multi-gigabyte sparse mzML fixtures at test time (TEST-4;
  see the gotcha below before touching it). 50 tests as of 2026-10-05.
  (`Test/Files/*.mzML`/`.mzXML` used to get corrupted by Git on Windows checkout regardless
  of `core.autocrlf` — fixed as BUG-7, `.gitattributes` now marks them `-text`. If you ever
  see `XmlException: Data at the root level is invalid` reading these files again, that fix
  regressed.)
- `ThermoFisher.CommonCore.*` packages aren't on nuget.org — restore needs a NuGet source
  pointing at a local checkout of `thermofisherlsms/RawFileReader`, **pinned to commit
  `b0fdf86931971d00c4576d148ecac2bc6568ba79`** (same SHA in all three workflows below,
  deliberately — bump it everywhere at once if it ever needs to change, not just one place).
  The old `dotnet.yml` checked that repo out at a floating, unpinned HEAD instead, which is
  exactly how it silently drifted onto a Thermo package version with a different transitive
  dependency tree and broke CI for ~2 months without anyone noticing (see CI-1). To build
  locally: `dotnet nuget add source <path-to-that-pinned-checkout>\Libs\NetCore\Net8`
  (that specific subfolder — it holds the `8.0.37` build `NovaIO.csproj` pins exactly via
  `[8.0.37]` version brackets, not a floating minimum). `8.0.37` is a deliberate target, not
  an accident: bumped 2026-08-19 from the original `8.0.6`, after first confirming that jump
  requires HYG-1 to be fixed (8.0.37 dropped a transitive dependency HYG-1's stray unused
  `using`s were quietly relying on — verified by bumping locally *before* fixing HYG-1 and
  watching it fail with the exact same errors that broke CI, then fixing HYG-1 and confirming
  both compile and all 3 tests pass against 8.0.37).

## CI/CD

Three workflows, replacing the old single `dotnet.yml` (deleted 2026-08-19 — see CI-1):

- **`ci.yml`** — build + test only, no packaging. Runs on PRs targeting `main`/`Dev` and
  direct pushes to `main`. This is the pre-merge safety net; it does not run on pushes to
  `Dev` (that's `dev-nuget.yml`'s job, which also builds and tests before packaging — no
  need to run the suite twice per push).
- **`dev-nuget.yml`** — push to `Dev`, or manual dispatch. Builds, tests, packs `Nova` +
  `Nova.IO` with a computed dev version (`<csproj version>-dev.<run number>`), publishes both
  a dated GitHub Release (kept forever, tag `dev-<run>-<sha>`) and a rolling `dev-latest`
  release (tag force-moved every run), both marked pre-release. GitHub Release assets only —
  deliberately no NuGet feed (not GitHub Packages, not anything else) so there's nothing for
  a tester to authenticate against.
- **`release.yml`** — manual dispatch only, and refuses to run from anything but
  `refs/heads/main` (checked in-workflow, not just by convention). Packs the real version
  from `Nova.csproj` (no `-dev.N` suffix), publishes one GitHub Release marked pre-release.
  **Nothing ever auto-promotes a release out of pre-release** — that's always a separate,
  deliberate action from the GitHub UI, done by the repo owner after reviewing the built
  packages. Also refuses to re-run over a tag that's already been promoted to a real release
  (checks `isPrerelease` via `gh release view` first), so a forgotten version bump can't
  silently clobber shipped assets.
- Both `dev-nuget.yml` and `release.yml` produce the same bundle shape Nova's real past
  releases already use (e.g. `v1.0.0.18`): `Nova.<version>.nupkg` + `Nova.IO.<version>.nupkg`
  + a `ThermoRawFileReader/` folder with the exact pinned Thermo packages and their license,
  so consuming `Nova.IO` never requires access to Thermo's own feed + a top-level `Readme.txt`.

## Versioning

Nova moved from an old 4-part scheme (`1.0.0.18`) to 3-part SemVer (`major.minor.revision`)
starting with this work, `1.1.0` — decided 2026-08-19, see CI-1.

**1.1.0 contains one breaking change** (HYG-6, 2026-10-05): the misspelled public
`FramentationType` enum and `PrecursorIon.FramentationMethod` property were renamed to
`FragmentationType`/`FragmentationMethod`. Consumers of the `Nova` package must recompile.
This was taken deliberately while 1.1.0 was still unreleased; the named-pipe wire format never
serialized that property, so IPC compatibility is unaffected. Put it in the release notes —
and note `NovaIO.csproj`'s `<PackageReleaseNotes>` still says "Initial release", which is
stale and would ship that way. `Nova.csproj` and
`NovaIO.csproj` are kept in lockstep (same `Version`/`AssemblyVersion`/`FileVersion` always —
they've always shipped together as a matched pair in every past release) and are the single
source of truth: `dev-nuget.yml` reads the base version from `Nova.csproj` and appends
`-dev.<run>`, `release.yml` uses it as-is. Bump both csprojs together when starting a new
version cycle; no workflow file needs editing to match.

## Working Conventions

- Every source file carries an Apache-2.0 license header block — copy the header from an
  existing `.cs` file when adding new source files.
- 2-space indentation throughout the C# codebase.
- **Doc-comment style** (settled during the HYG-8 pass, 2026-10-06/07 — see `docs/history.md`):
  - Say what the member *is* or *does* — the contract — in one terse sentence, no trailing
    period. `<param>`/`<returns>` on methods. A second sentence only for something a caller
    can't see from the declaration: a precondition, a silent no-op, a field nothing populates.
  - **No history in source.** Issue IDs, commit SHAs, and "why we fixed it this way" belong in
    `docs/history.md` and commit messages, never in `///` or `//`. A one-line `//` guardrail is
    fine where a maintainer might plausibly undo something; a paragraph is not.
  - No jargon, no tool names, no comparisons to other formats, no restating the code directly
    beneath or a sibling comment nearby. The minimum a reader needs, and nothing else.
  - If a comment would have to describe a bug as intended behavior, fix the code or flag it to
    the owner — don't document the bug as a feature. Five real fixes fell out of this rule.
  - Interface implementations use `/// <inheritdoc/>` (the four readers against
    `ISpectrumFileReader`), so the interface is the one source of truth. Every enum member gets a
    one-line `<summary>`; CS1591 requires it even when the name says it all.
  - Tests are the exception to "no history": a regression test's comment *should* name the bug
    it guards.
- Nullable reference types + implicit usings are enabled in the net8.0 projects (`NovaIO`,
  `NovaApp`, `Test`) but not in `Nova` (netstandard2.0, C# 7.3 default, no nullable
  annotations — left that way deliberately during the ARCH-1 retarget to keep it
  behavior-preserving; enabling nullable there would mean auditing the whole `Data/`/
  `IPC/Pipes/` surface, a separate piece of work).
- **All four format readers are `internal`** as of 2026-10-05 (HYG-7); `ThermoRawReader` used to
  be the odd one out. The public surface is `FileReader`, `SpectrumFileReaderFactory`,
  `ISpectrumFileReader`, `SpectrumFileOpenException`, the data types and the enums. The question
  "should the readers be public?" was asked and settled: the interface already covers every
  reader's behavior, so public classes would add only direct construction while permanently
  committing implementation types. Widening later is non-breaking if a real need appears;
  narrowing is not, which is why it went this direction. See HYG-7 in `docs/history.md`, which
  also records why a format-forcing overload was considered and deferred rather than rejected.
- Format-specific readers (`ThermoRawReader`, `MzMLReader`, `MzXMLReader`, `MGFReader`) each
  hold a single mutable `spectrum`/`spectrumEx` field that's overwritten on every read call.
  This is a deliberate simplicity tradeoff for sequential single-threaded reads, not a bug —
  but it does mean a reader instance is not safe for concurrent use, and can't hold two
  "current" spectra at once. Keep that in mind before changing the pattern; it's a design
  decision to discuss, not something to silently "fix."

## Before You Touch This Repo

Read `docs/known-issues.md` for currently-open bugs, dead/redundant code, and hygiene
problems (none open as of 2026-10-07), and `docs/progress.md` for a per-item status table. Both are lean by design — the
full write-up of everything already resolved (background, decision, fix, verification, and
the complete session log) lives in `docs/history.md`, not in the working docs. When you fix
something from `known-issues.md`, update `docs/progress.md` in the same change (status + a
one-line note), then move the full write-up into `docs/history.md` once it's done.

## Known Gotchas (see docs/history.md for full detail)

- **File byte offsets are `long`, and must stay that way.** mzML/mzXML offsets
  (`indexListOffset`, `indexOffset`, and every per-spectrum/chromatogram `<offset>`) are parsed
  through [`NovaIO/Io/Read/ByteOffset.cs`](NovaIO/Io/Read/ByteOffset.cs) and stored in
  `List<long>`. Narrowing any of this back to `int` reintroduces BUG-9 (fixed 2026-10-02): every
  mzML over 2 GiB became unreadable, and because the open failure was swallowed, every scan came
  back with zero peaks rather than an error. `uint` is not a fix either — it just moves the cliff
  to 4 GiB, which is why `Test/TestLargeFiles.cs` covers 2^32 as well as 2^31. Scan numbers stay
  `int` deliberately; only byte positions need the wider type.
- **A failed `Open` must reach the caller.** `SpectrumFileReaderFactory.GetReader` and
  `FileReader`'s `ReadSpectrum`/`ReadSpectrumEx`/`ReadChromatogram` overloads throw
  `SpectrumFileOpenException` when a file can't be opened or indexed (BUG-10, fixed 2026-10-02).
  Don't go back to discarding `Open`'s `bool` — a reader that opened unsuccessfully returns an
  empty `Spectrum` for every scan, which is indistinguishable from "that scan isn't in this file"
  and is exactly how BUG-9 reached downstream software as "Scan N was not found". The readers
  record their failure reason via the internal `IOpenFailureDetail` rather than
  `Console.WriteLine`-ing it; `ISpectrumFileReader` and `ThermoRawReader`'s public surface are
  deliberately unchanged by this.
- **The large-file tests generate a sparse fixture; don't "simplify" the safety gate away.**
  `Test/LargeMzMLFixture.cs` builds real 2.5 GiB and 4.5 GiB mzML files in the OS temp directory
  at test time, marked sparse so they cost ~0 bytes on disk and ~20 ms. It verifies the
  `SparseFile` attribute actually applied *before* writing anything past the hole, and reports
  `Inconclusive` if not. Without that check, a failed sparse flag makes NTFS zero-fill gigabytes
  for real on a CI runner. No mzML of any size is checked into this repo — that's a standing
  rule, and these fixtures exist precisely so it stays that way.
  In CI that skip is a **failure** instead: all three workflows set
  `NOVA_REQUIRE_LARGE_FILE_TESTS=1` on their `Test` step, because a skip still reports green and
  would silently drop the BUG-9 coverage. Locally, and on non-Windows, it stays a graceful skip.
- **`Close()` on a reader really closes it now** (BUG-11, fixed 2026-10-05). The XML readers'
  `Close()` used to be an empty method, so `XmlFS` leaked a handle per file switch and kept the
  file locked. It now disposes, `Open()` releases any prior handle, and `MGFReader.Close()` drops
  its line buffer. Consequence: a reader is unusable after `Close()` until re-opened. Nothing
  downstream called `Close()` when this changed (confirmed by the repo owner), which is what made
  it safe — if that ever stops being true, this is the change to look at.

- MGF support (BUG-1/BUG-2) is **fixed** (2026-08-19) — `FileReader.OpenSpectrumFile` and
  `SpectrumFileReaderFactory.GetReader` both dispatch `.mgf` to a real `MGFReader`, which
  parses actual spectrum data against the Matrix Science MGF spec
  (https://www.matrixscience.com/help/data_file_help.html), not a stub. `MGFReader` reads the
  whole file into memory once (no built-in index the way mzML/mzXML have) and resolves each
  spectrum's scan number from `SCANS=` if present, else the common msconvert-style TITLE
  convention, else sequential numbering. A `CHARGE=` value listing several states (`2+ and 3+`,
  `2+,3+`) yields one precursor per state (BUG-12, fixed 2026-10-07).
- `MzMLWriter.Write`'s schema validation is **fixed** (2026-08-19, BUG-4) — off by default via
  `validateSchema`/`schemaPath` params, no longer hardcodes a path.
- HYG-1 (stray unused `using`s riding on the Thermo package's transitive dependencies) is
  **fixed** (2026-08-19, alongside the `8.0.37` bump above) — don't reintroduce unused
  `using`s for packages not referenced in the csproj; this exact pattern is what broke CI for
  ~2 months (see CI-1/HYG-1 in `docs/history.md`) and it's a landmine specifically
  because it compiles fine right up until the transitive dependency tree changes.
- HYG-5 (`FileReader.cs`/`MzXMLReader.cs`/`MzMLWriter.cs` unknowingly depending on
  `ThermoFisher.CommonCore.Data`'s own `IsNullOrEmpty` extension instead of a project-local
  one) is **fixed** (2026-08-19) — use `"...".IsNullOrEmpty()` via
  [`NovaIO/StringExtensions.cs`](NovaIO/StringExtensions.cs) (`namespace Nova.Io`) going
  forward, not a bare `using ThermoFisher.CommonCore.Data;` for this. Fixing it also removed
  three more stray Thermo `using`s (verified empirically) that had no other purpose in those
  files — same landmine shape as HYG-1, just live rather than dead.
- HYG-4 (scattered TODOs collected for visibility) is **closed** (2026-08-19) — its own
  definition never required code changes. The three TODOs it lists (asymmetric isolation
  windows in `Precursor.cs`, `GetHeader()` in `ISpectrumFileReader.cs`, multi-precursor
  trailer mapping in `ThermoRawReader.cs`) are still real and still unimplemented; picking
  one up should be its own deliberate item, not something "closing HYG-4" implies happened.
