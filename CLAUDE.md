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
Nova/               .NET Framework 4.8 class library (old-style .csproj) — core data model + IPC
  Data/              Spectrum/Chromatogram/Precursor data types
  IPC/Pipes/         Named-pipe client/server (forked from acdvorak/named-pipe-wrapper, MIT)
NovaIO/              .NET 8 SDK-style library — file I/O, depends on Nova
  Io/Read/           FileReader facade + format readers (ThermoRaw, mzML, mzXML, MGF)
  Io/Write/          MzMLWriter + a small hand-rolled XML element tree (NovaXmlElement)
  Io/Meta/           Thermo trailer-label -> MetaClass lookup (MetaDictionary)
NovaApp/              .NET 8 console demo/smoke-test app
Test/                 .NET 8 MSTest project — integration-style tests against Test/Files/AngioNeuro4.*
Examples/             Separate solution (NovaExamples.sln): WinForms/console samples
                       (Protostar, ScanBroadcaster, ScanReceiver, ScanViewer)
docs/                 Repo-local documentation (known issues, modernization progress)
.github/workflows/    dotnet.yml (CI build+test), jekyll.yml (deploys gh-pages docs site)
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
  directory (see HYG-2), and gets it wrong if run from elsewhere. On Windows with
  `core.autocrlf=true` (common default), the `Test/Files/*.mzML`/`.mzXML` fixtures also need
  their line endings normalized back to LF before tests will pass — `.gitattributes` doesn't
  actually prevent Git from mangling them on checkout (see BUG-7); run
  `dos2unix Test/Files/AngioNeuro4.mzML Test/Files/AngioNeuro4.mzXML` first if you hit
  `XmlException: Data at the root level is invalid`. The existing tests are integration tests
  against real files in `Test/Files/` (mzML, mzXML, and RAW versions of the same acquisition)
  — they assert scan counts and MS-level tallies, not internal parsing logic in isolation.
  See `docs/known-issues.md` (TEST-1, TEST-2) for the coverage gap.
- CI (`.github/workflows/dotnet.yml`) checks out `thermofisherlsms/RawFileReader` at build
  time and adds it as a local NuGet source before restoring, since RawFileReader's native
  binaries aren't published to nuget.org. Building locally outside CI requires the same
  setup (or a pre-populated NuGet cache) before `ThermoFisher.CommonCore.RawFileReader`
  will restore.

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
- `.gitattributes` doesn't actually stop Git from mangling `Test/Files/*.mzML`/`.mzXML` line
  endings on checkout (see "Run tests" above and BUG-7) — breaks local test runs on Windows
  with default settings.
- Several files carry unused `using`s for unrelated packages (`Microsoft.AspNetCore.*`,
  `Newtonsoft.Json`, `Microsoft.VisualBasic`, `System.Formats.Tar`) that only resolve
  because they're transitively pulled in by the Thermo NuGet packages — not intentional
  dependencies, and fragile if that transitive chain ever changes.
