# Known Issues

This document catalogs bugs, dead/redundant code, and hygiene problems found during an
initial full-repo review (2026-08-18). Each item has an ID used for cross-reference in
[`progress.md`](progress.md), where fix status is tracked.

Severity guide: **High** = incorrect behavior or crash reachable in normal use. **Medium** =
incorrect/misleading behavior in a less common path, or a real (if narrow) correctness risk.
**Low** = code quality, dead code, or maintainability concern with no behavioral impact today.
Architecture items use **Priority** instead of Severity — they're initiatives, not defects.

---

## Architecture / Framework Targeting

### ARCH-1 — Migrate `Nova` (core) to `netstandard2.0`
**Priority:** **#1 — top priority** (set by repo owner 2026-08-18)
**Scope:** `Nova/Nova.csproj` only — the `Data/` and `IPC/Pipes/` code. **Explicitly out of
scope for now:** `NovaIO` stays on `net8.0` as-is; its own multi-targeting is deferred, not
abandoned (see below).

**Background:** Nova currently ships as two disconnected framework targets — `Nova` (core
data model + IPC) on .NET Framework 4.8, `NovaIO`/`NovaApp`/`Test` on .NET 8 — rather than
one artifact that works for both net48 and net8+ consumers. This surfaced as a real problem
for Helios (`SchweppeLab/Helios`), a net48 consumer of `Nova.Data` and `Nova.IPC.Pipes` that
is pinned to net48 by Thermo's IAPI (not by choice) and would be stranded if Nova ever
dropped net48 outright. Helios already solves this exact problem internally —
`Helios.Bridge.Contracts` targets `netstandard2.0` and is consumed as-is by both a net48 host
and a net8 client, no forking. This item applies the same pattern to `Nova`.

**Why this is feasible:** a full read-through of `Nova/Data/*` and `Nova/IPC/Pipes/*` found
nothing beyond plain BCL types available in `netstandard2.0` — `System.IO`,
`System.IO.Pipes`, `System.Threading` (`AutoResetEvent`, `SynchronizationContext`),
`System.Threading.Tasks` (`TaskScheduler`), `System.Collections.Concurrent`
(`BlockingCollection`), `System.Collections.Generic`. No WinForms, no `ConfigurationManager`,
no remoting/AppDomain tricks, and — critically, unlike `NovaIO` — **no third-party package
that pins it to a specific runtime**. (`NovaIO` was investigated for the same move and ruled
out specifically: `ThermoFisher.CommonCore.RawFileReader`/`.Data` only ship platform-specific
`net471`/`net48`/`net5.0`/`net8.0` builds, never a `netstandard` asset, and NuGet won't let a
`netstandard2.0` project consume a platform-specific-only package. `Nova` core has no such
dependency, so that blocker doesn't apply to it.)

**What this unblocks:**
- Helios can keep consuming `Nova` without risk of being stranded by a future Core-only cut.
- Replaces `NovaIO`'s current `net8.0` → `net48` reference to `Nova`, which today only works
  via .NET's unofficial Framework-compatibility shim, with a fully-supported netstandard2.0
  reference. (Platform-specific projects can always consume `netstandard2.0` — that
  direction was never the problem; only the reverse is blocked.)

**Mechanical steps (not yet started):**
1. Convert `Nova/Nova.csproj` from the old-style project format to SDK-style
   (`<Project Sdk="Microsoft.NET.Sdk">`), matching `NovaIO`/`NovaApp`/`Test`.
2. Change `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` →
   `<TargetFramework>netstandard2.0</TargetFramework>`.
3. Decide on `LangVersion`/nullable-reference-types: the current net48 config pins
   `LangVersion 7.3` with no nullable annotations; `netstandard2.0` supports much newer C#
   versions, so decide whether to bring `Nova` in line with the `Nullable`-enabled net8.0
   projects or leave it as-is for now.
4. Verify `Nova.sln` still builds cleanly with `NovaIO`/`NovaApp`/`Test` referencing the new
   `netstandard2.0` `Nova` — should be strictly easier than today's shim-based reference, not
   harder, so a regression here would be a red flag worth investigating.
5. Decide whether `Nova` should also gain NuGet packaging metadata (`PackageId`, `Version`,
   etc.) as part of this — it currently has none at all, unlike `NovaIO` which already ships
   as the `Nova.IO` package. Needed if Helios/others are meant to consume it as a published
   package rather than a project reference; can be deferred to a follow-up if out of scope.

**Known interaction:** BUG-6's suggested fix (`Stream.ReadExactly`) is .NET 7+ only and won't
be available once `Nova` targets `netstandard2.0` — that fix will need a manual read-loop
instead. Flag this when BUG-6 is picked up (after ARCH-1, since the target framework affects
the fix).

**Explicitly deferred — `NovaIO` multi-targeting:** investigated in parallel and found
technically feasible (not blocked, just bigger in scope): Thermo publishes parallel
`net48`/`net8.0` builds of `ThermoFisher.CommonCore.RawFileReader`/`.Data` under the same
package IDs, with a near-identical API surface (confirmed by diffing Thermo's own official
Framework vs. NetCore example programs). Would require per-`$(TargetFramework)` conditional
`PackageReference` versions, a CI change to add a second local NuGet source for the net48
build (alongside the existing net8.0 source in `dotnet.yml`), and — importantly — HYG-1
(stray unused `using`s) would stop being optional cleanup and become a build-breaker, since
the net48 Thermo package has a much leaner transitive dependency tree (no
`Microsoft.AspNetCore.Mvc.NewtonsoftJson`, no `Microsoft.Extensions.Hosting`) than the net8.0
one those stray usings accidentally compile against today. Not being pursued right now by
explicit decision; revisit as its own item later if needed.

---

## Bugs

### BUG-1 — `MGF` format not handled in `FileReader.OpenSpectrumFile`'s switch
**Severity:** High
**Location:** [`NovaIO/Io/Read/FileReader.cs`](../NovaIO/Io/Read/FileReader.cs), `OpenSpectrumFile`, the `switch (ff)` block (around the `ThermoRaw`/`MzML`/`MzXML`/`Unknown` cases)

`CheckFileFormat` correctly recognizes `.mgf` and returns `FileFormat.MGF`, but the `switch`
inside `OpenSpectrumFile` only has cases for `ThermoRaw`, `MzML`, `MzXML`, and `Unknown`.
When `ff == FileFormat.MGF`, none of the cases match, there's no `default`, and execution
falls through to `fileReader.Open(fileName)` with `fileReader` left at whatever it was
before the call (`null` on a fresh `FileReader`). This throws a `NullReferenceException`
instead of failing predictably.

**Suggested fix:** add a `case FileFormat.MGF: fileReader = new MGFReader(Filter); break;`
(contingent on BUG-2 being resolved one way or the other — see below), or explicitly reject
MGF with a clear exception if it's staying unsupported.

---

### BUG-2 — `MGFReader` is a non-functional stub
**Severity:** High
**Location:** [`NovaIO/Io/Read/MGFReader.cs`](../NovaIO/Io/Read/MGFReader.cs)

`MGFReader.Open()` does real work — it reads and parses the MGF global header (CHARGE, MASS,
etc.) — but `GetSpectrum` and `GetSpectrumEx` are stubs that unconditionally
`return new Spectrum(0)` / `return new SpectrumEx(0)`. No per-spectrum parsing is
implemented at all. The class carries its own acknowledgment of this:
`//TODO: Seriously rethink supporting this format, since it allows for conflicting
meta-information conventions.`

Because `FileFormat.MGF` is a public enum member, and (once BUG-1 is fixed) `FileReader`
would successfully "open" an MGF file, callers have no signal that reads will silently
return empty spectra forever.

**Suggested fix:** either finish the reader, or remove `FileFormat.MGF` /
`SpectrumFileReaderFactory`'s MGF branch and document MGF as unsupported, so failure is
loud (an exception) rather than silent (empty data).

---

### BUG-3 — `MzMLReader` sets `Analyzer = "OTMS"` instead of `"ITMS"`
**Severity:** Medium
**Location:** [`NovaIO/Io/Read/MzMLReader.cs`](../NovaIO/Io/Read/MzMLReader.cs), `ProcessCvParam`, case `"MS:1000512"` (filter string)

```csharp
else if (val.Contains("ITMS"))
{
  if (ext) spectrumEx.Analyzer = "ITMS";
  else spectrum.Analyzer = "OTMS";   // <-- should be "ITMS"
}
```

The `ext` (SpectrumEx) branch correctly sets `"ITMS"`; the non-`ext` (plain `Spectrum`)
branch has a copy-paste typo and sets `"OTMS"`, which is not a real analyzer type. Any
consumer of `FileReader.ReadSpectrum(...)` (the non-Ex path) reading an ion-trap scan from
an mzML file gets a wrong/garbage `Analyzer` value.

**Suggested fix:** one-character fix, `"OTMS"` → `"ITMS"`. Worth a regression test once
TEST-2 exists, since this is exactly the kind of copy-paste bug that a parameterized
Spectrum/SpectrumEx test would catch automatically.

---

### BUG-4 — `MzMLWriter.Write` hardcodes an absolute schema path
**Severity:** Medium (breaks the writer entirely for anyone but the original author)
**Location:** [`NovaIO/Io/Write/MzMLWriter.cs`](../NovaIO/Io/Write/MzMLWriter.cs), `Write(string filename)`

```csharp
checkSettings.Schemas.Add("http://psi.hupo.org/ms/mzml", "D:\\Data\\mzML\\mzML1.1.0.utf8.xsd");
```

After writing the mzML file, `Write()` re-opens it and validates it against the official
mzML XSD — but the XSD path is a hardcoded local path on the original developer's machine.
On any other machine (including CI), this throws (file not found), so `MzMLWriter.Write`
cannot currently be used successfully outside that one machine. There's no test coverage
that would have caught this, since nothing currently calls `MzMLWriter.Write` in the test
suite.

**Suggested fix:** either ship the XSD as an embedded resource, make the path configurable
(constructor param / property with a sensible default), or make schema validation optional
and off by default.

---

### BUG-5 — `FileReader.Format` is dead: initialized once, never updated
**Severity:** Low (misleading public API, not a crash)
**Location:** [`NovaIO/Io/Read/FileReader.cs`](../NovaIO/Io/Read/FileReader.cs), line ~57

```csharp
public FileFormat Format { get; } = FileFormat.Unknown;
```

This is a public, get-only auto-property initialized to `Unknown`. Nothing in
`OpenSpectrumFile` (or anywhere else) ever assigns it, so it is permanently `Unknown` no
matter what file is opened — despite `OpenSpectrumFile` computing the correct `FileFormat`
locally (as `ff`) and just not storing it anywhere.

**Suggested fix:** make it a regular settable property (or backing field + read-only
property) and assign `Format = ff;` in `OpenSpectrumFile`.

---

### BUG-6 — `PipeIO.Read` doesn't handle short/partial stream reads
**Severity:** Medium (latent correctness risk, not observed to fail in current usage)
**Location:** [`Nova/IPC/Pipes/PipesConnection.cs`](../Nova/IPC/Pipes/PipesConnection.cs), `PipeIO.Read`

```csharp
pm.MsgData = new byte[len];
stream.Read(pm.MsgData, 0, len);
```

`Stream.Read` is not guaranteed to fill the buffer in one call — it can return fewer bytes
than requested, including for named pipes under load or with larger messages split across
OS-level packet boundaries. This code ignores the return value and assumes the buffer is
fully populated, which can silently produce a `PipeMessage` with trailing zero bytes /
truncated data instead of throwing or retrying.

**Suggested fix:** loop until `len` bytes are read (or use
`stream.ReadExactly(pm.MsgData, 0, len)`, available in modern .NET) instead of a single
`Read` call. Note this file lives in the net48 `Nova` project, so confirm `ReadExactly`
availability or provide a manual loop compatible with net48.

---

## Dead / Redundant Code

### CLEAN-1 — `TSpectrum.GetMz` duplicates its own boundary-check logic
**Severity:** Low
**Location:** [`Nova/Data/ISpectrum.cs`](../Nova/Data/ISpectrum.cs), `TSpectrum<T>.GetMz`

After computing `index = ~index` and `indexB = index - 1`, the method does an initial pass
of "past the end" / "before the beginning" checks with early returns, and then — a few
lines later — repeats the exact same three conditions again inside a second
`if (index == 0) {...} else {...}` block. The second copy is unreachable in practice because
the first pass's early `return`s already handle every case it checks. It's not causing
wrong behavior, just ~35 lines of dead/duplicated logic that makes the method harder to
read and modify safely.

**Suggested fix:** delete the second duplicated block; keep only the first pass (with the
early returns), or consolidate into one clean set of boundary checks.

---

### CLEAN-2 — `ThermoRawReader.ProcessSpectrumInformation` sets fields redundantly
**Severity:** Low
**Location:** [`NovaIO/Io/Read/ThermoRawReader.cs`](../NovaIO/Io/Read/ThermoRawReader.cs), `ProcessSpectrumInformation`

```csharp
if (ext)
{
  spectrumEx.RetentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);
  spectrumEx.ScanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber).ToString();
}
else
{
  spectrum.RetentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);
  spectrum.ScanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber).ToString();
}
spectrum.RetentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);   // <-- runs unconditionally
spectrum.ScanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber).ToString(); // <-- even when ext == true
```

The two lines after the `if/else` re-run the same two Thermo API calls and reassign
`spectrum` (never `spectrumEx`) unconditionally — including when `ext == true`, where they
have no effect on the object actually being built (`spectrumEx`) and just waste two extra
native API calls per spectrum read.

**Suggested fix:** delete the two trailing unconditional lines; the `if/else` above already
covers both cases correctly.

---

### CLEAN-3 — `SpectrumFileReaderFactory` duplicates `FileReader.OpenSpectrumFile`'s dispatch logic
**Severity:** Low
**Location:** [`NovaIO/Io/Read/SpectrumFileReaderFactory.cs`](../NovaIO/Io/Read/SpectrumFileReaderFactory.cs) vs. [`NovaIO/Io/Read/FileReader.cs`](../NovaIO/Io/Read/FileReader.cs) `OpenSpectrumFile`

Two independent implementations of "look at the file extension, construct and `Open()` the
right `ISpectrumFileReader`" exist side by side, with different behavior on edge cases —
e.g. `SpectrumFileReaderFactory` throws `ArgumentException` on `.mgf`/`.mzdb`, while
`FileReader.OpenSpectrumFile` would (once BUG-1 is fixed) actually attempt to construct an
`MGFReader`. `NovaApp.cs` uses `SpectrumFileReaderFactory` directly for its second demo,
while everything else goes through `FileReader`. Any future format addition has to
remember to update both places, and they can silently drift (as they already have).

**Suggested fix:** have `FileReader.OpenSpectrumFile` delegate to
`SpectrumFileReaderFactory.GetReader` (or vice versa) so there's one source of truth for
extension-to-reader mapping.

---

## CI / Build Infrastructure

### CI-1 — Investigate and fix GitHub Actions workflow
**Severity:** TBD (needs investigation)
**Location:** [`.github/workflows/dotnet.yml`](../.github/workflows/dotnet.yml)

Added to the list per request, not yet diagnosed. The workflow builds `Nova.sln` in
Release|x64 on `windows-2022` and then runs `dotnet test`. It depends on checking out
`thermofisherlsms/RawFileReader` at build time and registering
`D:\a\Nova\Nova\RawFileReader\Libs\NetCore\Net8` as a local NuGet source so
`ThermoFisher.CommonCore.RawFileReader` can restore — this is a real external dependency on
another org's repo staying available and structured the same way, which is worth confirming
still works. No `gh` CLI was available in this environment to pull recent run history, so
current pass/fail status is unconfirmed.

**Suggested fix:** first pull the actual run history/logs (via `gh run list` /
`gh run view --log-failed`, or the Actions tab on GitHub) to see what's actually failing
before changing anything — don't guess at a fix without seeing a real error. Points worth
checking: whether the RawFileReader checkout/NuGet-source step still works as expected,
whether recently added files (BUG-1 through BUG-6 fixes, new tests from TEST-1/TEST-2) will
need CI updates, and whether the workflow should also build `Examples/NovaExamples.sln`
(currently untouched by CI).

---

## Hygiene / Maintainability

### HYG-1 — Stray unused `using`s for unrelated packages
**Severity:** Low (build currently succeeds only because these resolve transitively)
**Locations:**
- [`NovaIO/Io/Read/MGFReader.cs`](../NovaIO/Io/Read/MGFReader.cs) — `Microsoft.AspNetCore.Http.HttpResults`, `Newtonsoft.Json.Linq`
- [`NovaIO/Io/Read/ThermoRawReader.cs`](../NovaIO/Io/Read/ThermoRawReader.cs) — `System.Formats.Tar`
- [`NovaIO/Io/Write/MzMLWriter.cs`](../NovaIO/Io/Write/MzMLWriter.cs) — `Microsoft.VisualBasic`
- [`NovaApp/NovaApp.cs`](../NovaApp/NovaApp.cs) — `Microsoft.AspNetCore.Mvc.ApplicationModels`

None of these namespaces are used anywhere in their respective files, and none of the
packages that provide them (`Microsoft.AspNetCore.*`, `Newtonsoft.Json`) are referenced in
any `.csproj` in this repo. The build only succeeds today because they're pulled in
transitively via the `ThermoFisher.CommonCore.*` NuGet packages' own dependency trees. This
is fragile: a Thermo package update that drops or changes those transitive dependencies
would break the build with confusing "type or namespace not found" errors that have nothing
to do with the actual change being made.

**Suggested fix:** delete all of the above `using` statements; verify `dotnet build` still
succeeds afterward (it should — none are referenced).

---

### HYG-2 — `TestNova`'s test-data path resolution is fragile
**Severity:** Low
**Location:** [`Test/TestNova.cs`](../Test/TestNova.cs), constructor

The constructor locates `Test/Files/` by walking up from `Environment.CurrentDirectory`
(`Directory.GetParent(curDir).Parent.Parent`) with several null checks to avoid nullable
warnings, rather than using MSTest's `[DeploymentItem]` attribute or a path derived from
the test assembly's location. It happens to work with the current `bin/<config>/net8.0/`
output layout, but will silently resolve to the wrong directory (and then fail with
somewhat unclear file-not-found errors) if the output path structure ever changes.

**Suggested fix:** use `[DeploymentItem("Files", "Files")]` on the test class, or derive
the path from `typeof(TestNova).Assembly.Location` instead of `CurrentDirectory` and a
fixed number of `Parent` hops.

---

### HYG-3 — Inconsistent visibility: `internal` interfaces, `public` implementations
**Severity:** Low
**Location:** [`Nova/Data/IChromatogram.cs`](../Nova/Data/IChromatogram.cs) (`IChromatogram`, `IChromatDataPoint`)

Both interfaces are declared `internal` while the classes implementing them
(`Chromatogram`, `ChromatDataPoint`) are `public`. This doesn't currently cause a problem
(nothing outside the assembly needs to reference the interfaces directly), but it's
inconsistent with `ISpectrum<T>`/`ISpecDataPoint` in the same folder, which are `public`.
Worth deciding deliberately rather than leaving as an accident of how each file was
originally written.

**Suggested fix:** make `IChromatogram`/`IChromatDataPoint` `public` for consistency, unless
there's a deliberate reason to keep chromatogram abstraction internal-only — in which case
a short comment saying so would help.

---

### HYG-4 — Scattered TODOs marking acknowledged incomplete features
**Severity:** Low (informational — not new findings, just collected in one place)

Existing `//TODO` comments that mark known-incomplete areas, gathered here for visibility:
- `Nova/Data/Precursor.cs` — `IsolationWidth` doesn't support asymmetric isolation windows.
- `NovaIO/Io/Read/ISpectrumFileReader.cs` — `GetHeader()` was planned but never added to the
  interface (commented out).
- `NovaIO/Io/Read/ThermoRawReader.cs` — `ProcessTrailerExtraInformation`'s comment notes
  trailer-value-to-precursor mapping "needs reassessing" for scans with multiple precursors.
- `NovaIO/Io/Read/MGFReader.cs` — top-of-class TODO questioning whether MGF support is worth
  keeping at all (see BUG-2).

**Suggested fix:** no code change needed; these are here so they're visible in one place
instead of only surfacing when someone happens to open the specific file.

---

## Test Coverage Gaps

### TEST-1 — No unit tests for the `Nova` core library
**Severity:** Medium (risk multiplier — makes every other fix on this list riskier to land)
**Location:** [`Test/TestNova.cs`](../Test/TestNova.cs) (the entire test project)

There is zero test coverage for anything in the `Nova` project: `TSpectrum.GetMz` (binary
search + tolerance-window logic, see CLEAN-1), `TSpectrum.Serialize`/`Deserialize`
round-tripping, `PrecursorIon`, or the named-pipes IPC layer
(`PipesServer`/`PipesClient`/`PipesConnection`/`TaskQueue`). These have real, branchy logic
(especially `GetMz` and the pipe handshake/reconnect flow) that unit tests would catch
regressions in far faster than the current integration-only suite.

**Suggested fix:** add an MSTest (or xunit, if preferred) project/class targeting `Nova`
directly, covering at minimum: `GetMz` at/near/outside tolerance boundaries and on an empty
spectrum; a `Serialize`→`Deserialize` round trip for `Spectrum` and `SpectrumEx`; and a
basic same-process `PipesServer`/`PipesClient` connect-send-receive-disconnect test.

---

### TEST-2 — No unit tests for `NovaIO` parsing logic in isolation
**Severity:** Medium
**Location:** [`Test/TestNova.cs`](../Test/TestNova.cs)

The existing 3 tests only exercise the readers end-to-end against one real acquisition
(`AngioNeuro4`, in mzML/mzXML/RAW form) and assert aggregate counts (scan counts, MS-level
tallies). There's no test that exercises `ProcessCvParam`, `ProcessBinaryData`, or the
trailer-processing switch statements against small, hand-built/synthetic fixtures — the
kind of test that would have caught BUG-3 (the `"OTMS"` typo) directly instead of relying on
it happening to affect an assertion in the one real file used today. Malformed/missing-index
mzML or mzXML input is also entirely untested (`Open()` catches exceptions and returns
`false`, but nothing verifies that path).

**Suggested fix:** build small synthetic mzML/mzXML fixtures (a handful of spectra, not a
full real acquisition) to unit-test individual `cvParam`/binary-array/trailer code paths in
isolation, independent of the large integration fixture files already in `Test/Files/`.

---

## Contributing to This List

If you find a new issue while working in this repo, add it here with the next available ID
in its category, then add a corresponding row in [`progress.md`](progress.md). Keep entries
concrete: what's wrong, where, why it matters, and a suggested fix — not just "this looks
odd."
