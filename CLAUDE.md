# CLAUDE.md

Context for Claude Code (or any future contributor) working in the Nova repository.

## What Nova Is

Nova is a lightweight C# library for mass spectrometry (MS) file reading and spectral
data management, developed by the Schweppe Lab (University of Washington). It provides:

- A format-agnostic spectrum/chromatogram data model
- Readers for common MS file formats (Thermo RAW, mzML, mzXML; MGF is stubbed, not functional)
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
docs/                 Repo-local documentation (known issues, modernization progress)
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
- `Nova` (core: `Data/` + `IPC/Pipes/`) targets **`netstandard2.0`** as of 2026-08-18 (ARCH-1,
  done — see `docs/progress.md`/`docs/known-issues.md` for the full rationale). It was
  previously .NET Framework 4.8; the retarget was done so one build serves both net48
  consumers (e.g. Helios, pinned to net48 by Thermo's IAPI) and net8+ consumers, without
  forking into separate packages. `NovaIO`, `NovaApp`, and `Test` target **.NET 8** and are
  staying there for now — `NovaIO` multi-targeting to also support net48 was investigated and
  found technically feasible (Thermo ships parallel net48/net8.0 builds of the RawFileReader
  packages), but is deliberately deferred, not abandoned. Don't retarget `NovaIO` without
  discussing it first.
- Run tests: `dotnet test Test/Test.csproj`, run from the **repo root** (not from inside
  `Test/`) — `TestNova`'s constructor resolves `Test/Files/` relative to the working
  directory (see HYG-2), and gets it wrong if run from elsewhere. The existing tests are
  integration tests against real files in `Test/Files/` (mzML, mzXML, and RAW versions of the
  same acquisition) — they assert scan counts and MS-level tallies, not internal parsing
  logic in isolation. See `docs/known-issues.md` (TEST-1, TEST-2) for the coverage gap.
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
  locally: `dotnet nuget add source <path-to-that-pinned-checkout>\Libs\NetCore\Net8Old`
  (that specific subfolder, not `Net8` — it holds the `8.0.6` build `NovaIO.csproj` pins
  exactly via `[8.0.6]` version brackets, not a floating minimum).

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
starting with this work, `1.1.0` — decided 2026-08-19, see CI-1. `Nova.csproj` and
`NovaIO.csproj` are kept in lockstep (same `Version`/`AssemblyVersion`/`FileVersion` always —
they've always shipped together as a matched pair in every past release) and are the single
source of truth: `dev-nuget.yml` reads the base version from `Nova.csproj` and appends
`-dev.<run>`, `release.yml` uses it as-is. Bump both csprojs together when starting a new
version cycle; no workflow file needs editing to match.

## Working Conventions

- Every source file carries an Apache-2.0 license header block — copy the header from an
  existing `.cs` file when adding new source files.
- 2-space indentation throughout the C# codebase.
- Nullable reference types + implicit usings are enabled in the net8.0 projects (`NovaIO`,
  `NovaApp`, `Test`) but not in `Nova` (netstandard2.0, C# 7.3 default, no nullable
  annotations — left that way deliberately during the ARCH-1 retarget to keep it
  behavior-preserving; enabling nullable there would mean auditing the whole `Data/`/
  `IPC/Pipes/` surface, a separate piece of work).
- Format-specific readers (`ThermoRawReader`, `MzMLReader`, `MzXMLReader`, `MGFReader`) each
  hold a single mutable `spectrum`/`spectrumEx` field that's overwritten on every read call.
  This is a deliberate simplicity tradeoff for sequential single-threaded reads, not a bug —
  but it does mean a reader instance is not safe for concurrent use, and can't hold two
  "current" spectra at once. Keep that in mind before changing the pattern; it's a design
  decision to discuss, not something to silently "fix."

## Before You Touch This Repo

Read `docs/known-issues.md` for the catalogued list of known bugs, dead/redundant code, and
hygiene problems found during the initial review pass, and `docs/progress.md` for what's
already been fixed vs. still open. When you fix something from that list, update
`docs/progress.md` in the same change (status + a one-line note, not a rewrite).

## Known Gotchas (see docs/known-issues.md for full detail)

- `FileReader.OpenSpectrumFile`'s format switch has no `MGF` case — opening a `.mgf` can
  null-reference.
- `MGFReader` is a non-functional stub; `GetSpectrum`/`GetSpectrumEx` always return an
  empty spectrum regardless of file contents.
- `MzMLWriter.Write` hardcodes a Windows path (`D:\Data\mzML\mzML1.1.0.utf8.xsd`) for
  self-validation — this only works on the original author's machine.
- Several files carry unused `using`s for unrelated packages (`Microsoft.AspNetCore.*`,
  `Newtonsoft.Json`, `Microsoft.VisualBasic`, `System.Formats.Tar`) that only resolve
  because they're transitively pulled in by the Thermo NuGet packages — **this is not
  theoretical**: it's the confirmed, exact cause of CI being broken on `main` for ~2 months
  (see CI-1/HYG-1). It's currently masked again by the version pin described under "Build &
  Test" above, but the pin is a mitigation, not a fix — the stray `using`s are still there
  and will break again the moment that pinned dependency tree ever changes. Fix HYG-1 properly
  (delete the unused `using`s) rather than treating the pin as the actual solution.
