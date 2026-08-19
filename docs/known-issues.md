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

**Mechanical steps — done 2026-08-18:**
1. ✅ Converted `Nova/Nova.csproj` from the old-style project format to SDK-style
   (`<Project Sdk="Microsoft.NET.Sdk">`), matching `NovaIO`/`NovaApp`/`Test`. Dropped the
   old explicit Framework `<Reference>` items (`System.Xml`, `System.Data`,
   `System.Net.Http`, etc.) — none were actually used by any file in `Data/`/`IPC/Pipes/`;
   SDK-style + `netstandard2.0` provides that surface without explicit references. Dropped
   `packages.config` too (legacy format, unused — Nova has no package dependencies).
   `Properties/AssemblyInfo.cs` kept as-is with `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`
   (same pattern `NovaApp.csproj` already uses), rather than folding metadata into the csproj —
   smallest diff, doesn't force the packaging decision in step 5.
2. ✅ Changed `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` →
   `<TargetFramework>netstandard2.0</TargetFramework>`.
3. ✅ Decided: left `LangVersion` unset and `Nullable` disabled, matching current behavior —
   the SDK's default `LangVersion` for `netstandard2.0` is C# 7.3, identical to what the old
   csproj pinned explicitly, so this is a no-op behaviorally. Enabling nullable reference
   types was considered and deliberately not done here — it would mean auditing ~1,700 lines
   in `Data/`/`IPC/Pipes/` for null-safety, which is a real piece of work on its own and not
   needed for the retarget itself. Worth a future HYG-style item if wanted.
4. ✅ Verified: `dotnet build Nova/Nova.csproj` alone succeeds with **0 warnings, 0 errors**
   against `netstandard2.0` — confirms the codebase genuinely needed nothing beyond it.
   `dotnet build Nova/Nova.sln` (all 4 projects) also succeeds, with only the same 4
   pre-existing `NovaIO` warnings as before (nothing new, nothing from `Nova`). Ran
   `dotnet test Test/Test.csproj` — all 3 tests pass, confirming `NovaIO` genuinely works
   end-to-end against the retargeted `Nova` at runtime, not just at compile time. (One
   unrelated wrinkle hit along the way: local test runs initially failed with an XML parse
   error, root-caused to a `.gitattributes` line-ending bug, not this change — see BUG-7.)
5. **Done 2026-08-19, as part of CI-1's work:** `Nova.csproj` gained full NuGet packaging
   metadata matching `NovaIO.csproj` (`PackageId=Nova`, `Authors`, `Company`, `Description`,
   license, repo URL, `Version=1.1.0`, etc.) once packaging actually became necessary for the
   new `dev-nuget.yml`/`release.yml` workflows. See CI-1 for the full versioning-scheme
   change this was bundled with.

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

**Fixed 2026-08-19.** One-character fix, `"OTMS"` → `"ITMS"` on
[`MzMLReader.cs:770`](../NovaIO/Io/Read/MzMLReader.cs#L770). TEST-2's characterization test
(`Test/TestNovaIOFixtures.cs`) had pinned the buggy `"OTMS"` value against a synthetic ITMS
MS2 fixture; renamed to `MzML_Ms2ItmsSpectrum_NonExPath_IsCorrect` and flipped to assert the
correct `"ITMS"` value in the same change, so a regression here trips the suite. Verified:
full solution build clean (same 4 pre-existing `NovaIO` warnings, nothing new),
`dotnet test Test/Test.csproj` 28/28 passing.

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

**Fixed 2026-08-19.** `Format` is now `{ get; private set; }`, and `OpenSpectrumFile` assigns
`Format = ff;` right after the format switch resolves `ff`, before attempting to open the
file (so `Format` reflects the detected type even if the subsequent `Open()` call fails —
matching how `FileName` already behaves on a failed open per BUG-8). No test added
specifically for this (trivial property assignment); covered incidentally by every existing
`OpenSpectrumFile` test continuing to pass. Verified: full solution build clean, `dotnet test`
28/28 passing.

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

### BUG-7 — `.gitattributes` doesn't actually protect line-ending-sensitive test fixtures
**Severity:** Medium (breaks local `dotnet test` on a very common Windows Git configuration;
CI works around it but doesn't fix it)
**Location:** [`.gitattributes`](../.gitattributes), [`Test/Files/AngioNeuro4.mzML`](../Test/Files/AngioNeuro4.mzML), [`Test/Files/AngioNeuro4.mzXML`](../Test/Files/AngioNeuro4.mzXML)

Discovered while verifying ARCH-1 (see below): `dotnet test` failed with
`System.Xml.XmlException: Data at the root level is invalid` reading the mzML/mzXML fixtures
— unrelated to the netstandard2.0 retarget itself. Root cause: `MzMLReader`/`MzXMLReader` do
byte-offset random-access seeking into these files based on an embedded `<indexListOffset>`
(see `MzMLReader.Open`). If Git converts line endings on checkout, every byte offset past the
first converted line ending is wrong, and the reader lands mid-document.

The current `.gitattributes`:
```
*.mzML	text eof=lf
*.mzXML	text eof=lf
```
`eof=lf` only pins the final end-of-file character — it does **not** disable normal
line-ending conversion for the rest of the file. On any machine with `core.autocrlf=true`
(a common, often-default, Windows Git setting — including this machine), Git still converts
internal `LF` → `CRLF` on checkout, corrupting the index offsets. Confirmed directly: the
checked-out file has CRLF line terminators, and running `dos2unix` on both files locally
(working-copy only, not committed) took the test suite from `Failed: 3` to `Passed: 3` with
no other change.

CI works around this exact problem with an explicit `dos2unix` step in `dotnet.yml`
immediately before running tests (see CI-1) — which papers over the root cause rather than
fixing it, and doesn't help anyone running tests locally on Windows with default settings.

**Fixed 2026-08-19.** `.gitattributes` now reads:
```
*.mzML	-text
*.mzXML	-text
```
Confirmed the stored git blobs were already correct (LF) — only checkout was corrupting them,
so no historical content needed fixing, just the attribute. Re-normalized the working copy
with `git add --renormalize` + a direct `dos2unix` pass, confirmed the result byte-for-byte
matches what was already stored (`git status` shows no diff on the fixture files themselves,
only on `.gitattributes`). `dotnet test` passes 3/3 afterward with no other change. No more
`dos2unix` step needed anywhere — removed from all three workflows that replaced `dotnet.yml`
(see CI-1).

---

### BUG-8 — `FileReader.OpenSpectrumFile` discards the underlying reader's `Open()` result
**Severity:** Medium (silently misleading return value, not a crash)
**Location:** [`NovaIO/Io/Read/FileReader.cs`](../NovaIO/Io/Read/FileReader.cs), `OpenSpectrumFile`

```csharp
FileName = fileName;
fileReader.Open(fileName);   // bool return value discarded
ScanCount = fileReader.ScanCount;
...
return true;                 // always true, regardless of whether Open() actually succeeded
```

Discovered 2026-08-19 while building TEST-2's malformed-fixture tests. `MzMLReader.Open`/
`MzXMLReader.Open` both catch their own exceptions internally and return `false` on failure
(e.g. a file with no `<indexListOffset>`/`<indexOffset>` index) — but `OpenSpectrumFile`
never looks at that return value, and unconditionally returns `true` regardless. A caller
checking `if (reader.OpenSpectrumFile(path))` has no way to detect a failed open from the
return value alone; they'd need to separately check `ScanCount == 0` (which stays at its
default because the failed `Open()` never populated the scan index) or try reading a
spectrum and notice it comes back empty. Confirmed directly:
`TestNovaIOFixtures.MzML_MalformedFile_OpenDoesNotThrow` /
`MzXML_MalformedFile_OpenDoesNotThrow` open a synthetic index-less fixture, observe
`OpenSpectrumFile` return `true`, `ScanCount == 0`, and a subsequent `ReadSpectrum` come back
as an empty, zero-numbered `Spectrum` — pinning this actual current behavior as a
characterization test rather than asserting the (arguably more correct) `false`.

**Suggested fix:** `return fileReader.Open(fileName);` instead of the unconditional
`return true;`, and decide what should happen to `FileName`/`ScanCount`/etc. on a failed
open (currently `FileName` is set even on failure, which also affects `CheckFile`'s
same-file-already-open shortcut on a later retry with the same path).

**Fixed 2026-08-19.** `OpenSpectrumFile` now captures `Open()`'s return value and returns it
instead of an unconditional `true`. Deliberately left `FileName`/`ScanCount`/etc. assignment
behavior unchanged (still set even on a failed open) — that's the pre-existing `CheckFile`
same-file-shortcut behavior called out in the suggested fix as a separate decision, not part
of this bug's scope. The two characterization tests TEST-2 had added
(`MzML_MalformedFile_OpenDoesNotThrow` / `MzXML_MalformedFile_OpenDoesNotThrow`) were flipped
from asserting the old buggy `true` to asserting the correct `false`, same pattern as BUG-3.
Verified: `FileReader`'s own constructor overload (`OpenSpectrumFile` → `throw new
FileNotFoundException` if it returns `false`) now actually fires on a failed open instead of
being permanently dead code. Full solution build clean, `dotnet test` 28/28 passing.

---

## CI / Build Infrastructure

### CI-1 — GitHub Actions workflow: diagnosed, then replaced entirely
**Status:** Done 2026-08-19. Root cause diagnosed with hard evidence (public GitHub API, no
`gh` CLI needed after all — worked directly via `curl`/`api.github.com`), then `dotnet.yml`
was retired outright and replaced with three purpose-built workflows, per repo-owner
decision: keeping the old file around to "fix" wasn't wanted — full replacement was.
**Location (old, deleted):** `.github/workflows/dotnet.yml`
**Location (new):** [`ci.yml`](../.github/workflows/ci.yml), [`dev-nuget.yml`](../.github/workflows/dev-nuget.yml), [`release.yml`](../.github/workflows/release.yml)

**Root cause (confirmed via the public Actions API):** every run on `main` had failed for
at least ~2 months (back to 2026-06-18, likely longer) with identical compiler errors:
```
NovaIO/Io/Read/MzXMLReader.cs(22): The type or namespace name 'AspNetCore' does not exist in the namespace 'Microsoft'
NovaIO/Io/Read/MGFReader.cs(22,23): 'AspNetCore' / 'Newtonsoft' could not be found
```
This is HYG-1 (stray unused `using`s) — except not hypothetical, it was the *active* cause.
Chain of events: `dotnet.yml`'s "Point NuGet to RawFileReader" step checked out
`thermofisherlsms/RawFileReader` at a **floating, unpinned HEAD**, re-fetched fresh every
run. `NovaIO.csproj` declared `Version="8.0.6"` for the Thermo packages — but in NuGet, a
bare version string is a *minimum* bound (`[8.0.6,)`), not a pin. Verified those packages
aren't on nuget.org at all (404 on the flat-container API), so the only source NuGet could
resolve them from was that fresh checkout — which currently ships **8.0.37** in the folder
the workflow pointed at. `8.0.37 >= 8.0.6`, so NuGet silently took it instead. Diffed both
versions' nuspecs directly: 8.0.6 depends on `Microsoft.AspNetCore.Mvc.NewtonsoftJson` +
`Microsoft.Extensions.Hosting`; **8.0.37 dropped both** — Thermo cleaned up their dependency
tree at some point between releases, silently pulling the rug out from under HYG-1's stray
usings. (Locally, builds kept succeeding throughout — this machine's NuGet cache/config
already had something masking it, the same "this machine can hide a CI-only failure"
pattern from other repos worked in this environment.)

**What replaced it — three workflows, plus a versioning-scheme decision:**
- **`ci.yml`** — build + test only, no packaging. Triggers on PRs targeting `main`/`Dev` and
  direct pushes to `main`. Restores the pre-merge safety net the old workflow gave PRs
  (lost if `dotnet.yml` had simply been deleted with nothing replacing its PR-check role).
- **`dev-nuget.yml`** — triggers on push to `Dev` + manual dispatch. Builds, tests, packs
  `Nova` and `Nova.IO` with a computed dev version, publishes both a dated release (kept
  forever, tag `dev-<run>-<sha>`) and a rolling `dev-latest` release (tag force-moved each
  run), both marked `prerelease: true`. Modeled directly on `SchweppeLab/Helios`'s
  `Dev`-branch `dev-nuget.yml` (repo owner pointed at it as the reference) — same "dated +
  rolling release, no NuGet feed involved, GitHub Release assets only" shape, simplified
  because (unlike Helios) both `Nova` and `NovaIO` are SDK-style post-ARCH-1, so one job on
  one runner suffices; no OS split needed.
- **`release.yml`** — manual dispatch only, hard-guarded to refuse running from anything but
  `refs/heads/main` (workflow_dispatch lets you pick any ref from the UI, so this is enforced
  in-workflow, not just by convention). Packs the *real* project version (no `-dev.N`
  suffix), publishes one GitHub Release marked `prerelease: true` — promoting it to a real
  release is always a separate, deliberate, manual action from the GitHub UI, never
  automatic. Includes a guard step (`gh release view` + `isPrerelease` check) that refuses to
  run again over a tag that's already been promoted to a real release, so a forgotten version
  bump can't silently clobber a shipped release's assets.
- **Both `dev-nuget.yml` and `release.yml` produce the same bundle shape Nova's own past
  hand-built releases already use** (confirmed by downloading and unzipping the real
  `v1.0.0.18` release asset): `Nova.<version>.nupkg` + `Nova.IO.<version>.nupkg` +
  `ThermoRawFileReader/` (the exact pinned Thermo nupkgs + their license/readme, so a
  consumer of `Nova.IO` never needs access to Thermo's own feed) + a top-level `Readme.txt`.
  Rebuilt this exact bundle shape locally (pack + stage + zip, byte-for-byte structural
  match against the real release) before writing it into the workflows.
- **The `thermofisherlsms/RawFileReader` checkout is now pinned** to commit
  `b0fdf86931971d00c4576d148ecac2bc6568ba79` in all three workflows (same SHA everywhere,
  deliberately) instead of a floating HEAD — confirmed at that commit `Libs/NetCore/Net8Old`
  holds exactly the `8.0.6` packages `NovaIO.csproj` expects. Also tightened
  `NovaIO.csproj`'s `PackageReference`s from bare `8.0.6` to exact-match `[8.0.6]` brackets,
  as defense in depth — even if the pin is ever bumped without updating the version
  constraint, restore now fails loudly instead of silently drifting again.
- **Versioning scheme changed going forward:** Nova moves from the old 4-part scheme
  (`1.0.0.18`) to 3-part SemVer (`major.minor.revision`). This work is `1.1.0` — the next
  manual `release.yml` run (once this work is merged to `main` and stabilized) is intended to
  become the official `v1.1.0`. `Nova.csproj` gained full NuGet packaging metadata to match
  `NovaIO.csproj` (it previously had none — see ARCH-1's deferred step 5, now done):
  `PackageId=Nova`, `Authors`, `Company`, `Description`, license, repo URL, etc. Both
  `Nova.csproj` and `NovaIO.csproj` now carry `Version=1.1.0`/`AssemblyVersion=1.1.0.0`/
  `FileVersion=1.1.0.0`, kept in lockstep (as they always have been across past releases).
  `Nova/Properties/AssemblyInfo.cs` was deleted — SDK-style `Nova.csproj` now generates
  assembly attributes from the csproj properties directly, same as `NovaIO` already did.

**Validated locally before trusting any of this in CI:** restore against the exact pinned
source succeeds; full solution build succeeds; `dotnet pack` for both projects with an
overridden dev version succeeds; `NovaIO`'s packed nuspec correctly generates a `<dependency
id="Nova" version="[matching dev version]">` (confirms MSBuild's command-line
`-p:PackageVersion` override propagates through the `ProjectReference` as expected); the
full bundle-assembly PowerShell logic was rehearsed end-to-end locally and its output
structurally matches the real `v1.0.0.18` release, file for file.

**Addendum 2026-08-19 — bumped the pinned target from `8.0.6` to `8.0.37`.** Repo owner asked
whether CI should package the current/latest RawFileReader instead of the older `8.0.6` it
had been pinned to. Checked first: `thermofisherlsms/RawFileReader`'s HEAD hadn't moved since
the pin was set, still exactly `b0fdf86931971d00c4576d148ecac2bc6568ba79`, so no re-pinning
needed, just pointing at `Libs/NetCore/Net8` (`8.0.37`) instead of `Libs/NetCore/Net8Old`
(`8.0.6`) at that same commit. Proved this was safe rather than assuming it: bumped
`NovaIO.csproj`'s exact-match pin to `[8.0.37]` and rebuilt *before* touching HYG-1 — this
reproduced the identical CI-1 failure (same two files, same errors), confirming 8.0.37 really
does drop the transitive dependency those stray usings needed. Fixed HYG-1 (see its entry
below) and rebuilt again: clean build, `dotnet test` 3/3 passing against `8.0.37`. Updated all
three workflows' pinned source folder from `Net8Old` to `Net8`, including the bundle-staging
copy step, which needed `Readme.md` there instead of `Net8Old`'s `Readme.txt` (different
filename, caught by rehearsing the copy against the real folder rather than assuming
parity). `NovaIO.csproj`'s exact-match brackets (`[8.0.37]`, not a bare version) stayed —
that part of CI-1's original fix wasn't specific to `8.0.6`, it's the general defense against
this exact drift happening again with whatever version is pinned.

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

**Fixed 2026-08-19.** All six stray `using`s deleted — the four listed above, plus one this
entry's original catalog had actually missed: `NovaIO/Io/Read/MzXMLReader.cs`'s
`Microsoft.AspNetCore.Mvc` (visible all along in the CI-1 failure logs, just not caught when
this list was first written). Not a routine cleanup pass — this became load-bearing the
moment CI-1 bumped the pinned Thermo package to `8.0.37` (see CI-1's addendum below), which
dropped the exact transitive dependency these usings needed. Verified directly, in order:
rebuilt against `8.0.37` first to reproduce the original CI failure exactly (same files, same
errors), then removed the usings and confirmed both a clean build and a full `dotnet test`
pass (3/3) against `8.0.37`.

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

**Fixed 2026-08-19.** Added [`Test/TestSpectrum.cs`](../Test/TestSpectrum.cs) (10 `GetMz`
cases covering empty/single-point spectra, exact matches, and within/outside-ppm-tolerance
at both array boundaries and in the middle — enough to exercise every branch CLEAN-1 flags as
duplicated — plus a full-field `Serialize`/`Deserialize` round trip for both `Spectrum` and
`SpectrumEx`, including `Precursors` and per-point extended fields) and
[`Test/TestPipes.cs`](../Test/TestPipes.cs) (one same-process `PipesServer`/`PipesClient`
connect → send (client→server) → send (server→client) → disconnect test, using a
GUID-derived server ID per test since `MSTestSettings.cs` parallelizes at method level and
named pipes are a shared, process-wide namespace). All new tests target `Nova` directly, no
file I/O involved.

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

**Fixed 2026-08-19.** Added [`Test/TestNovaIOFixtures.cs`](../Test/TestNovaIOFixtures.cs)
against four new hand-built, byte-offset-indexed fixtures in `Test/Files/`:
`NovaTestFixture.mzML`/`.mzXML` (4 spectra: MS1 FTMS, MS2 ITMS with a precursor, MS1 FTMS,
MS2 FTMS with a precursor — enough to exercise `ProcessCvParam`'s scan-level, precursor, and
binary-array cvParams, plus `ProcessBinaryData`'s 64-bit decode path for both formats,
without needing a real acquisition) and `NovaTestFixtureMalformed.mzML`/`.mzXML` (same body,
index block omitted, for the missing-index path). `MzMLReader`/`MzXMLReader` are both
`internal`, so all tests go through the public `FileReader` facade, same as the pre-existing
suite. Coverage includes: scan/MS-level counts, per-field spectrum values, decoded m/z and
intensity data points, and precursor fields (isolation m/z/width, monoisotopic m/z, charge,
fragmentation method) for both formats. Directly caught two real, previously-uncatalogued
gaps in the process: **BUG-3** (confirmed live via
`MzML_Ms2ItmsSpectrum_NonExPath_ReproducesBug3`, which pins the actual `"OTMS"` value rather
than fixing it here — see BUG-3) and **BUG-8**, a new finding (`OpenSpectrumFile` discarding
the reader's `Open()` return value — see BUG-8), both left as characterization tests rather
than fixed in this change, to keep TEST-2 itself a single, reviewable, test-only change.
Fixtures were generated with an offline Python script (not checked in — the fixtures
themselves are the durable artifact) that computes exact byte offsets for the
`<indexListOffset>`/`<indexOffset>` index blocks, matching how `MzMLReader.Open`/
`MzXMLReader.Open` random-access-seek into the file (see BUG-7 for why byte-exactness here
matters, including line-ending corruption risk — these new fixtures are pure ASCII, LF only,
and covered by the existing `*.mzML -text` / `*.mzXML -text` `.gitattributes` rules).

---

### TEST-3 — Test output doesn't say what each test actually verified
**Severity:** Low (developer-experience / diagnosability, not a correctness gap)
**Location:** [`Test/TestSpectrum.cs`](../Test/TestSpectrum.cs), [`Test/TestPipes.cs`](../Test/TestPipes.cs), [`Test/TestNovaIOFixtures.cs`](../Test/TestNovaIOFixtures.cs), [`Test/TestNova.cs`](../Test/TestNova.cs)

`dotnet test`'s default output only reports pass/fail counts and method names — there's no
per-test indication of *what* was being checked (e.g. "GetMz at the lower ppm-tolerance
boundary" vs. just `GetMz_BelowFirstPoint_WithinTolerance_ReturnsFirstIndex`). Method names
are already fairly descriptive, but a failure in CI output or a quick local run still
requires opening the source to know what broke and why it matters.

**Suggested fix:** add a short `TestContext.WriteLine(...)` (one or two lines, not a
paragraph) at the start of each test stating what it's verifying, so `dotnet test -v normal`
(or the `.trx` log) shows a human-readable line per test alongside pass/fail — e.g.
`TestContext.WriteLine("Verifies GetMz returns the nearest index when the target m/z is just
inside the ppm tolerance below the first data point.");`. Keep it terse; this is meant to aid
scanning output, not duplicate the method name or replace comments in the test body.

---

## Contributing to This List

If you find a new issue while working in this repo, add it here with the next available ID
in its category, then add a corresponding row in [`progress.md`](progress.md). Keep entries
concrete: what's wrong, where, why it matters, and a suggested fix — not just "this looks
odd."
