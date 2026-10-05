# Modernization History (2026-08-18 / 2026-08-19)

Full archival record of the initial repo review and the modernization/bug-fix pass that
followed it. Every item below started life in [`known-issues.md`](known-issues.md) and
[`progress.md`](progress.md); all of it is now **Done**, so the detailed write-ups and the
session-by-session log live here instead, keeping the two working documents lean for
whatever comes next. `known-issues.md` keeps a one-line index with links back into this file;
`progress.md` keeps the status table.

Nothing here needs to be read to work in the repo today — it's context for *why* the code
looks the way it does, not a checklist.

---

## Architecture / Framework Targeting

### ARCH-1 — Migrate `Nova` (core) to `netstandard2.0`
**Priority:** #1 — top priority (set by repo owner 2026-08-18). **Status: Done 2026-08-18**,
packaging metadata added 2026-08-19 as part of CI-1.
**Scope:** `Nova/Nova.csproj` only. `NovaIO` deliberately stays on `net8.0`.

**Background:** Nova shipped as two disconnected framework targets — `Nova` (core data model
+ IPC) on .NET Framework 4.8, `NovaIO`/`NovaApp`/`Test` on .NET 8 — rather than one artifact
serving both net48 and net8+ consumers. This was a real problem for Helios
(`SchweppeLab/Helios`), a net48 consumer of `Nova.Data`/`Nova.IPC.Pipes` pinned to net48 by
Thermo's IAPI (not by choice), which would be stranded if Nova ever dropped net48. Helios
already solves this exact problem internally (`Helios.Bridge.Contracts` on `netstandard2.0`,
consumed by both a net48 host and a net8 client) — this item applies the same pattern to
`Nova`.

**Why it was feasible:** a full read-through of `Nova/Data/*` and `Nova/IPC/Pipes/*` found
nothing beyond plain BCL types available in `netstandard2.0` (`System.IO`,
`System.IO.Pipes`, `System.Threading`, `System.Threading.Tasks`,
`System.Collections.Concurrent`, `System.Collections.Generic`) — no WinForms, no
`ConfigurationManager`, and critically no third-party package pinning it to a runtime.
(`NovaIO` was investigated for the same move and ruled out: `ThermoFisher.CommonCore.*` only
ship platform-specific builds, never a `netstandard` asset.)

**What it unblocks:** Helios can keep consuming `Nova` without risk of a future Core-only
cut stranding it; `NovaIO`'s net8→net48 reference to `Nova` (previously working only via the
unofficial Framework-compatibility shim) becomes a fully-supported netstandard2.0 reference.

**Mechanical steps:**
1. Converted `Nova/Nova.csproj` to SDK-style (matching `NovaIO`/`NovaApp`/`Test`); dropped
   stale Framework `<Reference>` items and the unused `packages.config`.
   `Properties/AssemblyInfo.cs` kept as-is initially (`<GenerateAssemblyInfo>false</...>`,
   matching `NovaApp.csproj`'s existing pattern) — later deleted as part of step 5.
2. Changed `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` →
   `<TargetFramework>netstandard2.0</TargetFramework>`.
3. Left `LangVersion` unset and `Nullable` disabled — the SDK's `netstandard2.0` default is
   C# 7.3, identical to what the old csproj pinned explicitly, so behaviorally a no-op.
   Enabling nullable reference types was considered and deliberately deferred — it would mean
   auditing ~1,700 lines in `Data/`/`IPC/Pipes/` for null-safety, a separate piece of work.
4. Verified: `Nova/Nova.csproj` alone builds with 0 warnings/0 errors against
   `netstandard2.0`; full `Nova.sln` build clean (same 4 pre-existing `NovaIO` warnings,
   nothing new); `dotnet test` 3/3 passing against real mzML/mzXML/RAW files end-to-end.
   (Found BUG-7 as a byproduct along the way — unrelated to this change.)
5. **Done 2026-08-19, alongside CI-1:** `Nova.csproj` gained full NuGet packaging metadata
   matching `NovaIO.csproj` (`PackageId`, `Authors`, `Company`, `Description`, license, repo
   URL, `Version=1.1.0`, etc.) once packaging became necessary for `dev-nuget.yml`/
   `release.yml`.

**Known interaction:** BUG-6's fix couldn't use `Stream.ReadExactly` (.NET 7+ only) once
`Nova` targeted `netstandard2.0` — used a manual read loop instead.

**Explicitly deferred — `NovaIO` multi-targeting:** investigated in parallel, found
technically feasible (Thermo ships parallel `net48`/`net8.0` builds of
`ThermoFisher.CommonCore.RawFileReader`/`.Data` under the same package IDs with a
near-identical API surface) but bigger in scope — would need per-`$(TargetFramework)`
conditional `PackageReference` versions, a second CI NuGet source for the net48 build, and
would turn HYG-1 from optional cleanup into a build-breaker (the net48 Thermo package has a
much leaner transitive dependency tree than net8.0's). Not pursued; revisit as its own item
if needed.

---

## Bugs

### BUG-1 — `MGF` format not handled in `FileReader.OpenSpectrumFile`'s switch
**Severity:** High · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/FileReader.cs`, `OpenSpectrumFile`

`CheckFileFormat` correctly recognized `.mgf` as `FileFormat.MGF`, but `OpenSpectrumFile`'s
switch had no case for it and no `default` — execution fell through to
`fileReader.Open(fileName)` with `fileReader` still `null`, throwing `NullReferenceException`
instead of failing predictably.

**Decision (repo owner):** implement real MGF support rather than reject the format; wire up
together with BUG-2, saved for last on the priority list as the biggest remaining piece of
work.

**Fix:** added `case FileFormat.MGF: return new MGFReader(filter);` to
`FileReader.CreateReader` (the shared dispatch helper CLEAN-3 introduced). Because
`SpectrumFileReaderFactory.GetReader` already delegates to that same helper, this one change
fixed `.mgf` dispatch through both call sites at once. Verified against
`Test/Files/AngioNeuro4.mgf` via `Test/TestMgf.cs`.

---

### BUG-2 — `MGFReader` is a non-functional stub
**Severity:** High · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/MGFReader.cs`

`Open()` did real header parsing, but `GetSpectrum`/`GetSpectrumEx` were stubs that
unconditionally returned an empty `Spectrum(0)`/`SpectrumEx(0)` — no per-spectrum parsing at
all, with the class's own `//TODO: Seriously rethink supporting this format...` acknowledging
it.

**Decision (repo owner):** finish the reader against the Matrix Science MGF spec
(https://www.matrixscience.com/help/data_file_help.html), using a real fixture,
`Test/Files/AngioNeuro4.mgf` (the MS2-only subset of the same AngioNeuro4 acquisition already
used elsewhere — 6 spectra, matching mzML/mzXML/RAW's 6 MS2 scans).

**Fix — full rewrite.** Key design points:
- **No built-in index, unlike mzML/mzXML.** Rather than hand-rolling byte-offset seeking on
  top of `StreamReader` (whose internal buffering desyncs `Stream.Position` — a real
  footgun), `Open()` reads the whole file into memory once via `File.ReadAllLines` and
  indexes every `BEGIN IONS`/`END IONS` block by line number. Trades memory for correctness
  and simplicity.
- **Scan number resolution.** This fixture has no `SCANS=` tags, only `TITLE=` in the common
  msconvert convention (`AngioNeuro4.16.16.` → scan 16). `ResolveScanNumber` prefers `SCANS=`
  (per spec, first number of a range/list), falls back to the TITLE regex convention (not
  part of the Mascot spec itself, but a pervasive real-world convention this fixture uses),
  falls back to sequential numbering. Confirmed against the fixture's real scan numbers: 16,
  69, 113, 179, 230, 280.
- **`MsLevel` is always 2** — MGF has no MS1 concept.
- **`PEPMASS` → `PrecursorIon`**, one per line (spec allows multiple, for chimeric spectra) —
  `IsolationMz`/`MonoisotopicMz` both from the m/z token, `Intensity` from the optional
  second token, `Charge` from the optional third token (`"2+"`/`"3-"` notation) if present,
  else spectrum-local `CHARGE=`, else the file's global header `CHARGE=`.
- **`TotalIonCurrent`/`BasePeakMz`/`BasePeakIntensity`/`LowestMz`/`HighestMz`/`StartMz`/
  `EndMz`** all computed directly from parsed peak data — MGF has no header fields for any of
  them.
- **Per-peak charge** (optional third token on a fragment line) captured into
  `SpecDataPointEx.Charge` on the `Ex` path; unused by this fixture but real.
- **Random access and sequential reads** are driven by file order via an internal scan-order
  list, not by incrementing the literal scan number the way `MzMLReader`/`MzXMLReader` do —
  necessary because this format's scan numbers are sparse (16, 69, 113, ...), unlike
  mzML/mzXML/RAW's contiguous ones.

Also fixed a real, previously-uncatalogued bug in the old stub's header loop: its
`while (!SR.EndOfStream)` had no exit tied to its own `endOfHeader` flag (set but never
read), so it silently consumed the whole file instead of stopping at the first spectrum
block.

**Surfaced HYG-5 as a byproduct** — see below.

**Test coverage:** `Test/Files/AngioNeuro4Malformed.mgf` (header-only, no spectrum blocks)
plus `Test/TestMgf.cs` (9 tests: scan count/range/max RT, MS-level tally, sequential
file-order reads, a real spectrum's fields/peak data — expected values independently computed
from the raw file text via `awk`, not derived from the reader — precursor fields,
scan-number random access, the `Ex` path, `SpectrumFileReaderFactory` no longer throwing for
`.mgf`, and the malformed fixture returning `false` cleanly). No `.gitattributes` entry
needed for the new `.mgf` fixtures (unlike BUG-7's mzML/mzXML) since `File.ReadAllLines`
handles CRLF/LF transparently.

Verified: full solution build clean (0 new warnings — the rewrite actually removed 3 of the
4 previously-baseline warnings, all from the old stub's dead code), `dotnet test` 37/37
passing (28 pre-existing + 9 new).

---

### BUG-3 — `MzMLReader` sets `Analyzer = "OTMS"` instead of `"ITMS"`
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/MzMLReader.cs`, `ProcessCvParam`, case `"MS:1000512"`

The `ext` (SpectrumEx) branch correctly set `"ITMS"`; the non-`ext` (plain `Spectrum`) branch
had a copy-paste typo setting `"OTMS"` (not a real analyzer type). Caught live by TEST-2's
characterization test against a synthetic ITMS MS2 fixture.

**Fix:** one-character fix, `"OTMS"` → `"ITMS"`. The characterization test was renamed
`MzML_Ms2ItmsSpectrum_NonExPath_IsCorrect` and flipped to assert the correct value in the
same change. Verified: full solution build clean, `dotnet test` 28/28 passing.

---

### BUG-4 — `MzMLWriter.Write` hardcodes an absolute schema path
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Write/MzMLWriter.cs`, `Write(string filename)`

After writing, `Write()` re-opened the file and validated it against the official mzML XSD —
but the path (`D:\Data\mzML\mzML1.1.0.utf8.xsd`) was hardcoded to the original developer's
machine, throwing (file not found) everywhere else, including CI.

**Fix:** took the "optional, off by default" route — `Write` now takes
`bool validateSchema = false` and `string? schemaPath = null`. With the default, `Write`
never touches an XSD at all. `NovaApp.cs`'s existing call site needed no change (relies on
the new default) and now actually succeeds instead of throwing. Verified: full solution build
clean, `dotnet test` 28/28 passing.

---

### BUG-5 — `FileReader.Format` is dead: initialized once, never updated
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/FileReader.cs`

`public FileFormat Format { get; } = FileFormat.Unknown;` was never assigned anywhere, so it
stayed `Unknown` regardless of what file was opened, despite `OpenSpectrumFile` computing the
correct format locally (`ff`) and just not storing it.

**Fix:** `Format` is now `{ get; private set; }`; `OpenSpectrumFile` assigns `Format = ff;`
right after resolving `ff`, before attempting to open the file (so `Format` reflects the
detected type even if `Open()` subsequently fails, matching `FileName`'s existing behavior
per BUG-8). Verified: full solution build clean, `dotnet test` 28/28 passing.

---

### BUG-6 — `PipeIO.Read` doesn't handle short/partial stream reads
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `Nova/IPC/Pipes/PipesConnection.cs`, `PipeIO.Read`

`stream.Read(pm.MsgData, 0, len)`'s return value was ignored and the buffer assumed fully
populated — `Stream.Read` isn't guaranteed to fill the buffer in one call, which could
silently produce a `PipeMessage` with trailing zero bytes / truncated data.

**Fix:** added a manual read loop (`Stream.ReadExactly` isn't available — `Nova` targets
`netstandard2.0` per ARCH-1) that keeps calling `stream.Read` until all `len` bytes are
collected, throwing `EndOfStreamException` if the pipe closes early instead of silently
returning truncated data. Covered incidentally by `TestPipes.ConnectSendReceiveDisconnect`.
Verified: full solution build clean, `dotnet test` 28/28 passing.

---

### BUG-7 — `.gitattributes` doesn't actually protect line-ending-sensitive test fixtures
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `.gitattributes`, `Test/Files/AngioNeuro4.mzML`/`.mzXML`

Discovered while verifying ARCH-1: `dotnet test` failed with
`XmlException: Data at the root level is invalid` reading the mzML/mzXML fixtures.
`MzMLReader`/`MzXMLReader` do byte-offset random-access seeking based on an embedded
`<indexListOffset>` — if Git converts line endings on checkout, every byte offset past the
first converted line ending is wrong. The existing `.gitattributes` (`*.mzML text eof=lf`)
only pinned the final EOF character, not the rest of the file — on any machine with
`core.autocrlf=true` (common on Windows), Git still converted internal LF → CRLF on
checkout, corrupting the index offsets. CI worked around this with an explicit `dos2unix`
step in the old `dotnet.yml`, papering over rather than fixing the root cause.

**Fix:** `.gitattributes` now reads `*.mzML -text` / `*.mzXML -text`. Confirmed the stored
git blobs were already correct (LF) — only checkout was corrupting them. Re-normalized the
working copy (`git add --renormalize` + `dos2unix`), confirmed the result byte-for-byte
matched what was already stored. `dotnet test` passes 3/3 afterward with no other change. The
`dos2unix` CI step was removed from all three workflows that replaced `dotnet.yml` (see
CI-1).

---

### BUG-8 — `FileReader.OpenSpectrumFile` discards the underlying reader's `Open()` result
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/FileReader.cs`, `OpenSpectrumFile`

Discovered while building TEST-2's malformed-fixture tests: `MzMLReader.Open`/
`MzXMLReader.Open` both catch their own exceptions and return `false` on failure, but
`OpenSpectrumFile` never looked at that value and unconditionally returned `true` regardless.
A caller had no way to detect a failed open from the return value alone.

**Fix:** `OpenSpectrumFile` now captures and returns `Open()`'s actual result instead of an
unconditional `true`. Deliberately left `FileName`/`ScanCount` assignment-on-failure behavior
unchanged (a separate, pre-existing `CheckFile` same-file-shortcut concern, out of this bug's
scope). TEST-2's two characterization tests (`MzML_MalformedFile_OpenDoesNotThrow`/
`MzXML_MalformedFile_OpenDoesNotThrow`) were flipped from asserting the old buggy `true` to
asserting the correct `false`. Verified: `FileReader`'s constructor overload (which throws
`FileNotFoundException` if `OpenSpectrumFile` returns `false`) now actually fires instead of
being dead code. Full solution build clean, `dotnet test` 28/28 passing.

### BUG-9 — mzML/mzXML byte offsets parsed and stored as `int`, so no file over 2 GiB could be read
**Severity:** High · **Status: Done 2026-10-02**
**Location:** `NovaIO/Io/Read/MzMLReader.cs`, `NovaIO/Io/Read/MzXMLReader.cs`

Reported from the field: `Nova.Io.Read.MzMLReader` failed to open *any* mzML larger than
2 GiB (2,147,483,647 bytes), printing `Failed to open Value was either too large or too small
for an Int32.` and then — because the failure was discarded, see BUG-10 — handing back a
reader whose every `GetSpectrum` call returned a `Spectrum` with zero data points. Downstream
software (Scops's spectrum viewer) surfaced this as "Scan N was not found in &lt;file&gt;" for
every PSM, even though the scans existed. Modern Orbitrap runs routinely produce 2-3 GB mzML,
so this affected real data. Reproduced across 20 indexed mzML files from pwiz 3.0.24326: all
12 files under 2 GiB read every scan correctly; all 8 files over 2 GiB failed, with an exact
boundary (largest working `indexListOffset` 2,024,931,947; smallest failing 2,344,714,782).

The reporter's first hypothesis was the right one, and it was the *only* thing that fired —
the open blew up before any per-spectrum offset was ever parsed:

- `MzMLReader.Open` parsed `<indexListOffset>` with `int.Parse` into an `int` local.
- Both readers stored their whole offset table as `List<int>` (`scanIndex`, and `chrIndex`
  for chromatograms), filled via `Convert.ToInt32(XmlFile.ReadElementContentAsString())`.
- `MzXMLReader` had the identical bug on `<indexOffset>` — same index design, same ceiling.

**Mechanism of the silent-empty-spectra symptom:** `Open` caught, printed, and returned
`false`, leaving `scanIndex` empty and `lastScanNumber` at 0. Every later `GetSpectrum(n)` hit
the `CurrentScanNumber > lastScanNumber` guard and returned `new Spectrum(0)` — identical to
what a genuinely absent scan returns.

**Fix:** all file byte positions are now `long` end to end. `scanIndex`/`chrIndex` became
`List<long>`; every offset is parsed through a new shared helper,
[`NovaIO/Io/Read/ByteOffset.cs`](../NovaIO/Io/Read/ByteOffset.cs), which uses `long.Parse`
under `CultureInfo.InvariantCulture`. `FileStream.Seek` already took `long`, so every seek
site was fixed for free. Scan numbers deliberately stay `int` — only byte positions needed
widening. Note that a *uint* would not have fixed this; it only moves the cliff to 4 GiB,
which is why the test suite covers 2^32 as well as 2^31.

Two incidental cleanups fell out of the same lines: `offset` was being reused as both the byte
position and the *buffer* offset argument to `XmlFS.Read(bytes, offset, 200)` (harmless but
confusing, and it stopped compiling once the type widened), and the 200-byte tail read now
decodes only the bytes `Stream.Read` actually returned rather than the whole buffer. The
invariant-culture change is not cosmetic either: this repo shipped a culture-dependent
number-parsing bug once before (commit `b486948`).

**Other readers checked, per the report's request:**
- `MGFReader` — *not* affected. It indexes by **line number** into an in-memory `string[]`,
  not byte offsets, so `int` is correct there. It does carry a different large-file limit:
  `File.ReadAllLines` means a multi-GB MGF fails on memory, not on `Int32`. That is a
  pre-existing, already-documented design tradeoff (see the `lines` field comment in
  `MGFReader.cs`), not part of this bug; left alone deliberately.
- `ThermoRawReader` — no byte offsets at all, goes through Thermo's API. Nothing to do.
- `MzMLWriter` — already correct. It writes offsets from `XmlFS.Position`, which is a `long`.
  Nova could already *write* a >2 GiB indexed mzML that it could not read back.

**Verification:**
- A sparse-file test fixture generated at test time (`Test/LargeMzMLFixture.cs`), giving real
  2.5 GiB and 4.5 GiB indexed mzML files at ~0 bytes on disk and ~20 ms per test. See TEST-4.
- **Negative control:** reverting only `ByteOffset.Parse` to `int.Parse` makes 3 of the 4 new
  large-file tests fail with the exact reported message, *"Value was either too large or too
  small for an Int32."* — confirming the new tests genuinely reproduce the bug rather than
  passing vacuously. Restored afterwards.
- **Real file:** `D:\Data\Astral\20250425_MH_Hela100ng_DIDDA_30m_06.mono.mzML`, 3,416,549,519
  bytes (3.18 GiB), 332,687 spectra, `indexListOffset` 3,387,525,212. 206,605 spectra sit
  below 2^31 and 126,082 at or above it. Opens in 380 ms; scans 206605 (offset 2,147,475,087)
  and 206606 (offset 2,147,487,376) — the exact pair straddling the boundary — both read back
  with peak arrays matching an independent decode of the same byte offset to within 1e-9, as
  do the first scan, a middle scan, and the last scan (offset 3,382,833,004). 25 consecutive
  sequential reads across the boundary returned zero empty spectra. The file's chromatogram
  index (`TIC`, `Pump Pressure 1/2`, all three offsets past 2^31) also reads correctly,
  exercising `chrIndex`. This file is local-only and is deliberately **not** referenced by any
  committed test.
- Full solution build clean (0 new warnings), `dotnet test` 45/45 passing.

### BUG-10 — A failed open was swallowed, so an unreadable file was indistinguishable from a missing scan
**Severity:** High · **Status: Done 2026-10-02**
**Location:** `NovaIO/Io/Read/SpectrumFileReaderFactory.cs`, `NovaIO/Io/Read/FileReader.cs`,
all four format readers

The second half of the same field report. BUG-9 made files unreadable; this is what made it
*invisible*. `SpectrumFileReaderFactory.GetReader` called `reader.Open(file)` and **discarded
the `bool`**, returning the reader regardless — so a caller received something that looked
fine and yielded an empty `Spectrum` for every scan. The readers, meanwhile, wrote the real
reason to `Console.WriteLine` and dropped it.

This is the same defect BUG-8 fixed in `FileReader.OpenSpectrumFile` back in August; the
factory path simply kept it. Worse, auditing for it turned up **two more** instances: all
three of `FileReader`'s `ReadChromatogram`/`ReadSpectrum`/`ReadSpectrumEx` overloads call
`OpenSpectrumFile(fileName)` when handed a new file name and discard that result too, so a
failed file switch silently read on against a reader that had never opened.

**Fix:** a new public `SpectrumFileOpenException` (deriving from `IOException`, carrying the
`FileName`) and an **internal** `IOpenFailureDetail` interface, both in
[`NovaIO/Io/Read/OpenFailure.cs`](../NovaIO/Io/Read/OpenFailure.cs). The four readers
implement `IOpenFailureDetail` *explicitly* and record their failure reason instead of
printing it; `GetReader` and the three `FileReader.Read*` overloads check the open result and
throw with that reason attached.

Deliberate design choices:
- `ISpectrumFileReader` keeps **exactly** the shape it has always had, and `ThermoRawReader`
  (the one public reader) gains **no new public member** — hence the internal interface and
  explicit implementation. `Open` still returns `bool`; the existing BUG-8 tests that assert
  `OpenSpectrumFile` returns `false` are unchanged and still pass.
- The rethrow added to `FileReader`'s three catch-alls is `catch (SpectrumFileOpenException)`,
  not `catch (IOException)`. Widening it to `IOException` would also start propagating the
  `FileNotFoundException` that `CheckFile` raises, which those methods have always swallowed —
  an unrequested behavior change. The narrow type keeps this surgical.
- `ThermoRawReader.Open` gained no `try`/`catch`. Unlike the XML readers it has never
  swallowed exceptions from the Thermo API, and those keep propagating as they always have.

**One real leak found and fixed as a direct consequence:** making a failed open throw means
the caller never receives the reader and so can never call `Close()` on it. The XML readers
create their `FileStream` before the failure point, so the handle was being orphaned. Both
readers now dispose it on the failure path. This surfaced concretely — a scratch harness
couldn't delete its own temp file afterwards. It also exposed **BUG-11** (`Close()` is a
no-op in both XML readers and never releases `XmlFS` at all), which is left **open**; see
[`known-issues.md`](known-issues.md).

**Verification:** 5 new tests in `Test/TestNovaIOFixtures.cs` covering both readers' failure
paths, the `FileReader` file-switch path, and a guard that a *good* file still returns a
working reader. Against the real 3.18 GiB Astral file, an unindexed mzML now raises
`SpectrumFileOpenException: Failed to open '<path>': ...` instead of printing and returning a
dud. Full solution build clean, `dotnet test` 45/45 passing.

### BUG-11 — `MzMLReader.Close()`/`MzXMLReader.Close()` were no-ops and leaked a file handle
**Severity:** Medium · **Status: Done 2026-10-05** · *Found 2026-10-02 while fixing BUG-10.*
**Location:** `NovaIO/Io/Read/MzMLReader.cs`, `NovaIO/Io/Read/MzXMLReader.cs`

Both XML readers hold the open file in a `FileStream` field (`XmlFS`) for random access, but
`Close()` in both was an empty method whose only content was a commented-out line copied from
`ThermoRawReader` — where the same line is real and does dispose its handle. `XmlFS` was
therefore never released, so the handle survived until the finalizer ran and, on Windows, kept
the file locked against deletion or rename meanwhile.

This was reachable in normal use: `FileReader`'s `ReadSpectrum`/`ReadSpectrumEx`/
`ReadChromatogram` call `fileReader.Close()` every time the caller switches files, expecting
that to release the previous one. A process iterating over many files leaked a handle per file.

Found concretely rather than by inspection: while verifying BUG-10, a scratch harness could not
delete its own temp file after Nova had opened it.

**Why it wasn't fixed alongside BUG-10:** `Close()` doing nothing meant any downstream caller
that closed a reader and kept reading from it would have been working *by accident*, and would
break the moment `Close()` actually closed. That needed an answer before it was safe to change.
The repo owner confirmed on 2026-10-05 that no downstream software has ever called `Close()`,
which removed the concern.

**Fix:** `Close()` in both readers now disposes `XmlFile` and `XmlFS` and nulls both, making it
idempotent and safe to call before any successful `Open`. `Open()` now calls `Close()` first, so
re-opening on the same instance releases the previous handle instead of orphaning it, and
BUG-10's failure path was simplified to call `Close()` rather than repeat the disposal inline.

Two adjacent things fixed because supporting re-open made them reachable:
- `MzMLReader.Open` cleared `scanIndex` but **not** `chrIndex`, so re-opening appended a second
  copy of the chromatogram index to the first. Harmless while `Open` was effectively single-use;
  a real bug once Close/Open on one instance works.
- `MGFReader.Close()` (which holds no file handle — it reads the whole file up front) now drops
  its `lines` buffer, by far the largest thing that reader holds. Not BUG-11 itself, but the
  same principle: `Close()` is the caller's signal that they are done.

`ThermoRawReader.Close()` was checked and is correct — it genuinely disposes `RawFile`. It is
the original the other two were mis-copied from, not a third instance of the bug.

**Verification:** 3 new tests in `Test/TestNovaIOFixtures.cs` — one per XML reader asserting
that the file can be **deleted** after `Close()` (the sharpest available probe on Windows, and
the exact symptom that exposed this), plus one asserting `Close()` is idempotent and that
re-opening rebuilds the index rather than doubling it. **Negative control:** restoring
`MzMLReader.Close()` to its old no-op makes `MzML_Close_ReleasesTheFileHandle` fail with
`IOException: The process cannot access the file ... because it is being used by another
process` — the original symptom — while the mzXML test still passes, confirming the tests are
per-reader and not incidentally coupled. Build clean, `dotnet test` 48/48 passing.

---

## Dead / Redundant Code

### CLEAN-1 — `TSpectrum.GetMz` duplicates its own boundary-check logic
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `Nova/Data/ISpectrum.cs`, `TSpectrum<T>.GetMz`

After the initial "past the end"/"before the beginning" checks with early returns, the method
repeated the exact same three conditions in a second, unreachable
`if (index == 0) {...} else {...}` block — ~35 lines of dead/duplicated logic.

**Fix:** deleted the duplicated block; the surviving "check both closest points" logic (what
used to live in the dead block's `else`) is now unconditional, the only path that can still
reach it. Behavior identical — covered by TEST-1's `GetMz` boundary tests. Verified: full
solution build clean, `dotnet test` 28/28 passing.

---

### CLEAN-2 — `ThermoRawReader.ProcessSpectrumInformation` sets fields redundantly
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/ThermoRawReader.cs`, `ProcessSpectrumInformation`

Two lines after an `if (ext) {...} else {...}` block re-ran the same two Thermo API calls and
reassigned `spectrum` (never `spectrumEx`) unconditionally — including when `ext == true`,
where they had no effect on the object actually being built and just wasted two extra native
API calls per spectrum read.

**Fix:** deleted the two redundant trailing lines; the `if/else` above already covered both
cases. Verified: full solution build clean, `dotnet test` 28/28 passing (including
`TestNova`'s existing end-to-end ThermoRaw read, which exercises this method).

---

### CLEAN-3 — `SpectrumFileReaderFactory` duplicates `FileReader.OpenSpectrumFile`'s dispatch logic
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `NovaIO/Io/Read/SpectrumFileReaderFactory.cs` vs. `NovaIO/Io/Read/FileReader.cs`

Two independent "look at the file extension, construct and `Open()` the right
`ISpectrumFileReader`" implementations existed side by side, with different edge-case
behavior (e.g. `SpectrumFileReaderFactory` threw on `.mgf`/`.mzdb`, while
`FileReader.OpenSpectrumFile` would attempt to construct an `MGFReader`). Any future format
addition had to remember to update both places, and they'd already drifted.

**Fix:** extracted the shared logic into `FileReader.CheckFileFormat` (made `static`) and a
new `internal static CreateReader(FileFormat, MSFilter)` mapping a format to a freshly
constructed, unopened reader (`null` for unsupported formats). `OpenSpectrumFile` and
`SpectrumFileReaderFactory.GetReader` both call these now, each preserving its own
error-handling contract on top (`OpenSpectrumFile` still returns `false` for `Unknown`;
`SpectrumFileReaderFactory.GetReader` still throws `ArgumentException` for
unrecognized/unsupported extensions). Verified: full solution build clean, `dotnet test`
28/28 passing, including `NovaApp.cs`'s existing `SpectrumFileReaderFactory.GetReader` call
site.

---

## CI / Build Infrastructure

### CI-1 — GitHub Actions workflow: diagnosed, then replaced entirely
**Status:** Done 2026-08-19, root cause diagnosed with hard evidence via the public GitHub
API, then `dotnet.yml` retired outright and replaced with three purpose-built workflows (repo
owner: replace, don't patch).
**Old location (deleted):** `.github/workflows/dotnet.yml`
**New locations:** `ci.yml`, `dev-nuget.yml`, `release.yml`

**Root cause (confirmed via the public Actions API):** every run on `main` had failed for at
least ~2 months (back to 2026-06-18) with identical compiler errors —
`MzXMLReader.cs`/`MGFReader.cs` failing to find `Microsoft.AspNetCore`/`Newtonsoft` types.
This was HYG-1 (stray unused `using`s) — except not hypothetical, it was the *active* cause.
`dotnet.yml`'s RawFileReader checkout used a **floating, unpinned HEAD**, re-fetched fresh
every run; `NovaIO.csproj` declared a bare `Version="8.0.6"`, which NuGet treats as a
*minimum* bound, not a pin. That floating checkout currently ships `8.0.37`, which NuGet
silently took instead — and Thermo had dropped
`Microsoft.AspNetCore.Mvc.NewtonsoftJson`/`Microsoft.Extensions.Hosting` from their own
dependency tree between `8.0.6` and `8.0.37`, pulling the rug out from under HYG-1's stray
usings. (Locally, builds kept succeeding throughout — this machine's NuGet cache/config
already masked the issue.)

**What replaced it:**
- **`ci.yml`** — build + test only, no packaging. Triggers on PRs targeting `main`/`Dev` and
  direct pushes to `main`. Restores the pre-merge safety net the old workflow gave PRs.
- **`dev-nuget.yml`** — push to `Dev` + manual dispatch. Builds, tests, packs `Nova`/`Nova.IO`
  with a computed dev version, publishes a dated release (kept forever) and a rolling
  `dev-latest` release, both `prerelease: true`. Modeled on `SchweppeLab/Helios`'s
  `Dev`-branch `dev-nuget.yml` (repo owner's reference), simplified because — unlike Helios —
  both `Nova` and `NovaIO` are SDK-style post-ARCH-1, so one job on one runner suffices.
- **`release.yml`** — manual dispatch only, hard-guarded (in-workflow, not just by
  convention) to refuse running from anything but `refs/heads/main`. Packs the real project
  version (no `-dev.N` suffix), publishes one `prerelease: true` GitHub Release — promoting
  to a real release is always a separate, manual, GitHub-UI action. Guards against re-running
  over an already-promoted tag.
- Both packaging workflows produce the same bundle shape Nova's own past releases use
  (confirmed by unzipping the real `v1.0.0.18` release): `Nova.<version>.nupkg` +
  `Nova.IO.<version>.nupkg` + a `ThermoRawFileReader/` folder with the exact pinned Thermo
  packages + license + a top-level `Readme.txt`.
- The `thermofisherlsms/RawFileReader` checkout is now pinned to commit
  `b0fdf86931971d00c4576d148ecac2bc6568ba79` in all three workflows (same SHA everywhere).
  `NovaIO.csproj`'s Thermo `PackageReference`s tightened from bare `8.0.6` to exact-match
  `[8.0.6]` brackets, as defense in depth against this exact drift recurring.
- **Versioning scheme changed:** Nova moved from the old 4-part scheme (`1.0.0.18`) to 3-part
  SemVer (`major.minor.revision`), starting at `1.1.0`. `Nova.csproj` gained full NuGet
  packaging metadata to match `NovaIO.csproj` (closing ARCH-1's deferred step 5); both csprojs
  kept in lockstep on `Version`/`AssemblyVersion`/`FileVersion`.
  `Nova/Properties/AssemblyInfo.cs` was deleted (SDK now generates it from csproj properties,
  same as `NovaIO`).

**Validated locally before trusting any of it in CI:** restore/build/test against the exact
pinned source; `dotnet pack` for both projects with a dev-style version override; confirmed
`NovaIO`'s generated dependency on `Nova` resolves to the matching version; rehearsed the
full bundle-assembly PowerShell logic end-to-end, output structurally matching the real
`v1.0.0.18` release.

**Addendum — bumped the pinned target from `8.0.6` to `8.0.37`.** Repo owner asked whether CI
should package the current/latest RawFileReader. Upstream HEAD hadn't moved, so this was just
repointing at `Libs/NetCore/Net8` (`8.0.37`) instead of `Net8Old` (`8.0.6`) at the same
commit. Proved this was safe rather than assumed: bumped the exact-match pin to `[8.0.37]`
and rebuilt *before* touching HYG-1 — this reproduced the identical CI-1 failure, confirming
`8.0.37` really does drop the dependency those stray usings needed. Fixed HYG-1, rebuilt
again clean, `dotnet test` 3/3 passing against `8.0.37`. Updated all three workflows' pinned
source folder, including the bundle-staging copy step (`Net8`'s readme is `Readme.md`, not
`Net8Old`'s `Readme.txt`). `NovaIO.csproj`'s exact-match brackets stayed — that defense was
never specific to `8.0.6`.

**Follow-up, not part of the original CI-1 scope — `dev-nuget.yml` asset naming (2026-08-19,
reported directly by the repo owner, not a catalogued item):** the rolling `dev-latest`
release had been publishing packages/zip under fixed generic filenames
(`Nova-dev-latest.nupkg`, etc.) built by copying and renaming the real versioned files,
instead of just reusing the dated release's actual `Nova.<version>-dev.<run>.nupkg`
filenames. Removed the duplicate/rename step; `dev-latest` now uploads the exact same
`pack-out/*.nupkg` + `Nova.<version>.zip` files the dated release does. Since that means
asset filenames now differ every run (rather than staying fixed and simply being replaced
by `action-gh-release`), added a `Clear stale assets from the dev-latest release` step (`gh
release view --json assets` + `gh release delete-asset`) immediately before publishing, so
the rolling release never accumulates old versioned assets. Not yet verified against a real
workflow run — reasoned through `action-gh-release`/`gh` CLI behavior only; worth confirming
on the next push to `Dev`.

---

## Hygiene / Maintainability

### HYG-1 — Stray unused `using`s for unrelated packages
**Severity:** Low · **Status: Done 2026-08-19**
**Locations:** `NovaIO/Io/Read/MGFReader.cs`, `NovaIO/Io/Read/ThermoRawReader.cs`,
`NovaIO/Io/Write/MzMLWriter.cs`, `NovaApp/NovaApp.cs`, `NovaIO/Io/Read/MzXMLReader.cs`

Six unused `using`s (ASP.NET Core / Newtonsoft.Json / VisualBasic / Tar namespaces) across
five files — none of the packages providing them were referenced in any `.csproj`; the build
only succeeded because they were pulled in transitively via `ThermoFisher.CommonCore.*`'s own
dependency tree. This became load-bearing (not just hygiene) the moment CI-1's Thermo pin
bumped to `8.0.37`, which dropped that exact transitive dependency — see CI-1's addendum.

**Fix:** all six usings deleted. Verified directly, in order: rebuilt against `8.0.37` first
to reproduce the CI-1 failure exactly, then removed the usings and confirmed both a clean
build and a full `dotnet test` pass (3/3) against `8.0.37`.

---

### HYG-2 — `TestNova`'s test-data path resolution is fragile
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `Test/TestNova.cs`, constructor

Located `Test/Files/` by walking up from `Environment.CurrentDirectory`
(`Directory.GetParent(curDir).Parent.Parent`), duplicated in `TestNovaIOFixtures.cs` too —
worked with the current `bin/<config>/net8.0/` layout but was fragile to change and to the
invocation directory.

**Fix:** extracted the duplicated walk into shared `Test/TestFilePaths.cs`'s
`GetFilesDirectory()`, anchored to `AppContext.BaseDirectory` instead of
`Environment.CurrentDirectory` (invocation-directory-independent). Caught a real bug doing
this: `AppContext.BaseDirectory` carries a trailing path separator, which made the first
`Directory.GetParent` call a no-op — broke 13/28 tests with `FileNotFoundException` on the
first attempt; fixed by trimming the trailing separator before starting the walk. Verified:
full solution build clean, `dotnet test` 28/28 passing.

---

### HYG-3 — Inconsistent visibility: `internal` interfaces, `public` implementations
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `Nova/Data/IChromatogram.cs` (`IChromatogram`, `IChromatDataPoint`)

Both interfaces were `internal` while their implementations (`Chromatogram`,
`ChromatDataPoint`) were `public` — inconsistent with `ISpectrum<T>`/`ISpecDataPoint` in the
same folder, which are `public`.

**Fix:** both interfaces changed to `public`, matching `ISpectrum<T>`/`ISpecDataPoint`.
Nothing was relying on the `internal` visibility. Verified: full solution build clean,
`dotnet test` 28/28 passing.

---

### HYG-4 — Scattered TODOs marking acknowledged incomplete features
**Severity:** Low (informational) · **Status: Closed 2026-08-19**

Gathered pre-existing `//TODO`s for visibility (this item's own definition never required a
code change):
- `Nova/Data/Precursor.cs` — `IsolationWidth` doesn't support asymmetric isolation windows.
- `NovaIO/Io/Read/ISpectrumFileReader.cs` — `GetHeader()` was planned but never added
  (commented out).
- `NovaIO/Io/Read/ThermoRawReader.cs` — `ProcessTrailerExtraInformation`'s trailer-mapping
  comment notes it "needs reassessing" for multi-precursor scans.
- The MGF-viability TODO in `MGFReader.cs` was resolved by BUG-2 (finish it) and removed
  along with the rest of the old stub.

**Closed as a tracking item** — its job (surface these in one place) was complete. The three
remaining TODOs above are **still unimplemented** and were deliberately left alone: each is a
real, separate piece of design/feature work, and picking one up should be its own deliberate
item (promote it to a new BUG/FEATURE entry, the way the MGF TODO became BUG-1/BUG-2).

---

### HYG-5 — Several files use `ThermoFisher.CommonCore.Data`'s `IsNullOrEmpty` extension as if it were project-local
**Severity:** Low · **Status: Done 2026-08-19**
**Locations:** `NovaIO/Io/Read/FileReader.cs`, `NovaIO/Io/Read/MzXMLReader.cs`,
`NovaIO/Io/Write/MzMLWriter.cs`

Found while implementing BUG-2: calls like `fileName.IsNullOrEmpty()` in these three files
didn't resolve to anything defined in this repo — they resolved to an extension method
provided by `ThermoFisher.CommonCore.Data`, whose `using` these files already had for
unrelated reasons. Confirmed directly: adding the same call to `MGFReader.cs` (no Thermo
reference) failed to compile until either that `using` was added or the call was switched to
the real `string.IsNullOrEmpty`. A milder cousin of HYG-1 — live functionality rather than
dead code, but the same "compiles fine until the transitive dependency tree changes" shape.

**Fix:** added `NovaIO/StringExtensions.cs` (`internal static class StringExtensions`,
`namespace Nova.Io`, a real `IsNullOrEmpty(this string? value)` forwarding to
`string.IsNullOrEmpty`) and repointed all three files at it. This also removed the underlying
dependency entirely: once the extension calls no longer needed
`ThermoFisher.CommonCore.Data`/`.RawFileReader`/`.Data.Business`, those `using`s turned out
to be unused for anything else in any of the three files either — verified empirically
(deleted, rebuilt, confirmed nothing broke), not assumed. Verified: full solution build clean
(same 1 pre-existing warning, 0 new), `dotnet test` 37/37 passing.

### HYG-6 — `FramentationType`/`FramentationMethod` misspelled in public API
**Severity:** Low · **Status: Done 2026-10-05**
**Location:** `Nova/data/SpectrumFoundation.cs`, `Nova/data/Precursor.cs`, plus 11 call sites

Two public names in the `Nova` core package were missing a `g`: the enum
`FramentationType` and the `PrecursorIon.FramentationMethod` property that uses it. Spotted by
the repo owner. Unlike NeoPepXMLParser's deliberately odd `Cnpx*` class names, this mirrored
nothing — "framentation" is not a term of art, just a misspelling, repeated consistently
across 13 occurrences in 6 files.

**Decision: straight rename, no compatibility shims** (option A of three offered). Both names
are public API, so this is a source- *and* binary-breaking change for anything consuming the
`Nova` package — consumers must recompile. Three things made that the right call rather than
the cautious one:

1. **1.1.0 had not shipped** as a non-prerelease release when this was fixed — only
   `-dev.N` prereleases existed. This was the cheapest moment the fix would ever have; after
   1.1.0 went out the choice would have been living with the typo or waiting for 2.0.0.
2. **The IPC wire format is unaffected.** `TSpectrum.Serialize`/`Deserialize` writes only
   `IsolationMz`, `IsolationWidth`, `MonoisotopicMz` and `Charge` for each precursor —
   `FramentationMethod` was never serialized, so named-pipe compatibility with Helios and
   anything else on the IPC layer is untouched by the rename. This was checked before
   recommending the rename, not assumed.
3. The alternative (keeping a deprecated `FramentationType` enum alongside the new one, plus
   an `[Obsolete]` forwarding property casting between two identical enums) would have bought
   only a deferred recompile, at the cost of carrying two parallel enums until 2.0.0.

**Fix:** mechanical `Framentation` → `Fragmentation` across all 13 occurrences — the enum
declaration, the property, 4 sites in `MzMLReader`, 3 in `MzXMLReader`, 2 in `MzMLWriter`,
and 2 test assertions. Nothing in `Examples/` referenced either name. Lowercase
"fragmentation" already appearing correctly in prose comments was left alone (the
substitution was case-sensitive on the capital `F` stem). Verified: a case-insensitive sweep
for `framentation` across `.cs`, `.md` and `.csproj` returns zero hits; clean full-solution
rebuild with 0 new warnings; `dotnet test` 45/45 passing.

**Note for whoever cuts 1.1.0:** this is the release's one breaking change and belongs in its
release notes. `NovaIO.csproj`'s `<PackageReleaseNotes>` still reads "Initial release",
which is stale regardless and would ship that way in the 1.1.0 nupkg.

---

## Test Coverage Gaps

### TEST-1 — No unit tests for the `Nova` core library
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `Test/TestNova.cs` (the entire test project, as it stood before this)

Zero test coverage for `TSpectrum.GetMz` (binary search + tolerance-window logic, see
CLEAN-1), `Serialize`/`Deserialize` round-tripping, `PrecursorIon`, or the named-pipes IPC
layer.

**Fix:** added `Test/TestSpectrum.cs` (10 `GetMz` cases covering empty/single-point spectra,
exact matches, within/outside-ppm-tolerance at both array boundaries and mid-array — every
branch CLEAN-1 flagged as duplicated — plus a full-field `Serialize`/`Deserialize` round trip
for `Spectrum` and `SpectrumEx`) and `Test/TestPipes.cs` (one same-process
`PipesServer`/`PipesClient` connect → send both directions → disconnect test, GUID-derived
server ID since `MSTestSettings.cs` parallelizes at method level and named pipes share a
process-wide namespace). No file I/O involved.

---

### TEST-2 — No unit tests for `NovaIO` parsing logic in isolation
**Severity:** Medium · **Status: Done 2026-08-19**
**Location:** `Test/TestNova.cs`

The existing 3 tests only exercised readers end-to-end against one real acquisition,
asserting aggregate counts — nothing exercised `ProcessCvParam`/`ProcessBinaryData`/trailer
switches against small synthetic fixtures, and malformed/missing-index input was untested.

**Fix:** added `Test/TestNovaIOFixtures.cs` against four new hand-built, byte-offset-indexed
fixtures (`NovaTestFixture.mzML`/`.mzXML`, 4 spectra: MS1 FTMS / MS2 ITMS+precursor / MS1 FTMS
/ MS2 FTMS+precursor; `NovaTestFixtureMalformed.mzML`/`.mzXML`, same body with the index block
omitted), generated via an offline Python script computing exact byte offsets (see BUG-7 for
why byte-exactness matters). Went through the public `FileReader` facade since both readers
are `internal`. **Directly caught two real, previously-uncatalogued bugs in the process:
BUG-3** (pinned live as a characterization test rather than fixed here) **and BUG-8** (a new
finding), both left as characterization tests to keep TEST-2 itself a single, test-only
change.

---

### TEST-3 — Test output doesn't say what each test actually verified
**Severity:** Low · **Status: Done 2026-08-19**
**Location:** `Test/TestSpectrum.cs`, `Test/TestPipes.cs`, `Test/TestNovaIOFixtures.cs`,
`Test/TestNova.cs`

`dotnet test`'s default output only reported pass/fail counts and method names — no per-test
indication of *what* was being checked without opening the source.

**Fix:** added a one-line `testContext.WriteLine(...)` at the top of every test method across
all four classes. `TestSpectrum`/`TestPipes`/`TestNovaIOFixtures` had no `TestContext` before
this; added the same public-property + constructor-injection pattern `TestNova.cs` already
used (`public TestContext testContext { get; set; }`) rather than a private field — MSTest's
`MSTEST0005` analyzer only recognizes the public-property form, caught by a clean rebuild
surfacing 3 new warnings with the private-field version tried first. Confirmed the messages
actually surface via a real `.trx` run (`TestContext Messages:` block per test) — the console
logger silently drops `TestContext.WriteLine` for passing tests.

**Addendum — surfaced these messages in CI too, not just `.trx`.** Repo owner wanted the
per-test messages visible directly in the GitHub Actions log. The plain/minimal console
logger drops them for passing tests, but `--logger "console;verbosity=detailed"` shows them
under a `TestContext Messages:` block — added that flag (alongside the existing
`--verbosity minimal`) to the `Test` step in all three workflows.

### TEST-4 — No coverage for files larger than 2 GiB
**Severity:** Medium · **Status: Done 2026-10-02**
**Location:** `Test/LargeMzMLFixture.cs`, `Test/TestLargeFiles.cs`

BUG-9 went unnoticed because nothing in the suite read a file anywhere near 2 GiB — every
fixture in `Test/Files/` is a few MB at most. The obvious fix (check in a big file) is a
non-starter: the repo owner's standing rule is that **no additional mzML of any size goes
into the repo**, and a 2.5 GB fixture has no business in Git regardless.

**Fix:** generate the fixture at test time as a **sparse file**. `LargeMzMLFixture` marks the
file sparse (NTFS `FSCTL_SET_SPARSE`) *before* writing anything, then `SetLength`s it to
2.5 or 4.5 GiB and writes a few KB of real XML at scattered positions. The multi-gigabyte
span in between is a hole: it reads back as zeros without ever being written or occupying
disk. This works because Nova never parses these files as one document — `Open()` seeks
straight to the index and `ParseSpectrum()` seeks straight to one spectrum — so a file that
is mostly hole exercises exactly the code path under test. Measured: **0 bytes allocated on
disk**, 8-22 ms per test, whole suite still ~390 ms. Files go to the OS temp directory and
are deleted in `Dispose`; nothing lands in `Test/Files/`.

**The safety gate is the important part.** If the sparse flag ever fails to apply (non-NTFS
temp volume, a future runner image change, a P/Invoke that fails), then `SetLength` followed
by a write near the end makes NTFS zero-fill the gap *for real* — gigabytes physically
written on a CI runner with limited free space, failing for a reason that looks nothing like
its cause. So `TryCreate` verifies the `SparseFile` attribute actually took and bails out
**before** writing past the hole, and the tests report `Inconclusive` with a readable reason
rather than proceeding. **Do not remove that check.** All three .NET workflows run on
`windows-2022`, so NTFS is available in CI; the non-Windows path skips cleanly.

**Why both 2.5 and 4.5 GiB:** an offset stored as *unsigned* 32-bit would pass between 2 and
4 GiB and still fail past it, so only the larger fixture distinguishes a real 64-bit fix from
a half-fix. Note the real Astral verification file is 3.18 GiB and so could not have caught
that on its own.

4 new tests, and the negative control described under BUG-9 confirms 3 of them fail against
the unfixed parse with the exact message from the field report.

**Addendum 2026-10-05 — a skip in CI is now a failure.** The Inconclusive gate above is right
for safety, but a skip still reports green, so CI could quietly stop covering BUG-9 after a
runner image or temp-volume change and nobody would notice — uncomfortably close to the defect
this area exists to prevent. The tests now check a `NOVA_REQUIRE_LARGE_FILE_TESTS` environment
variable (any value except empty, `0` or `false`): when it is set, an unavailable fixture is an
`Assert.Fail` explaining why, instead of an `Assert.Inconclusive`. All three workflows set it on
their `Test` step, so CI is loud while local and non-Windows runs still skip gracefully.
Verified in both directions by forcing `TryCreate` to fail: without the variable, 4 skipped and
the run is green; with it, 4 failed with the explanatory message. Normal runs are unaffected —
48/48 pass identically with and without it set.

---

## Full Session Log

Chronological record of every work session on this list, preserved verbatim from
`progress.md`'s original `## Log` section.

- 2026-08-18 — Initial review pass completed; `known-issues.md` and the tracker created. No
  fixes applied yet.
- 2026-08-18 — Added `.gitignore`; untracked two accidentally-committed `obj/` files; created
  `Dev` branch (all work happens here from now on, not `main`); added CI-1 (investigate/fix
  GitHub Actions) to the todo list.
- 2026-08-18 — Discussed framework-targeting strategy (prompted by a Helios-side note and a
  check of Thermo's `RawFileReader` package structure). Decision: migrate `Nova` core to
  `netstandard2.0`, leave `NovaIO` on `net8.0` for now. Added as ARCH-1, set as #1 priority.
  Planning only, no code changed yet.
- 2026-08-18 — **ARCH-1 done.** Converted `Nova/Nova.csproj` to SDK-style targeting
  `netstandard2.0`; dropped stale Framework `<Reference>`s and the unused `packages.config`.
  Verified: standalone build 0 warnings/0 errors; full `Nova.sln` build clean; `dotnet test`
  3/3 passing against real mzML/mzXML/RAW files. Found and documented BUG-7 as a byproduct.
- 2026-08-19 — **CI-1 and BUG-7 done.** Diagnosed CI-1 with hard evidence via the public
  GitHub API: every run on `main` had failed for ~2 months, root cause HYG-1 (live, not
  hypothetical) triggered by an unpinned external checkout drifting onto a Thermo package
  version with a different dependency tree. Fixed BUG-7 properly rather than working around
  it again. Explored `SchweppeLab/Helios`'s `Dev`-branch `dev-nuget.yml` and Nova's own past
  GitHub Releases before designing anything. Deleted `dotnet.yml` outright and replaced it
  with `ci.yml` + `dev-nuget.yml` + `release.yml`. Pinned the `thermofisherlsms/RawFileReader`
  checkout to a specific commit in all three. Tightened `NovaIO.csproj`'s Thermo
  `PackageReference`s to exact-match `[8.0.6]`. Versioning scheme changed to 3-part SemVer
  starting at `1.1.0`. Validated locally before trusting any of it in CI. Nothing pushed —
  local commit only, per standing instruction never to push.
- 2026-08-19 — Repo owner pushed the CI-1 commit and watched it run: worked well. Asked
  whether CI should package current/latest RawFileReader instead of the older 8.0.6. Checked:
  upstream HEAD hadn't moved, so just repointing `Net8Old` → `Net8` at the same pinned commit.
  Proved it wasn't safe to just flip the version first — bumping the pin to `[8.0.37]` alone
  reproduced CI-1's exact failure, confirming 8.0.37 dropped the dependency HYG-1's stray
  usings needed. **Fixed HYG-1** (ahead of its scheduled turn) as a direct prerequisite, then
  confirmed clean build + `dotnet test` 3/3 against 8.0.37. Updated all three workflows'
  pinned source folder and the bundle-staging step. Kept the exact-match `[8.0.37]` brackets.
- 2026-08-19 — **TEST-1 and TEST-2 done.** Added `Test/TestSpectrum.cs` and `Test/TestPipes.cs`
  for TEST-1. For TEST-2, hand-built four new indexed fixtures via an offline Python script
  and added `Test/TestNovaIOFixtures.cs` against them. Directly reproduced BUG-3 live (pinned
  as a characterization test) and surfaced a new bug, BUG-8, also pinned rather than fixed
  here, to keep this a single, reviewable, test-only change. All 28 tests (3 pre-existing +
  25 new) pass.
- 2026-08-19 — **BUG-3 done.** One-character fix in `MzMLReader.ProcessCvParam` (`"OTMS"` →
  `"ITMS"`, line 770). Flipped the characterization test to assert the now-correct value.
  Verified: full solution build clean, `dotnet test` 28/28 passing.
- 2026-08-19 — Added **TEST-3** to the backlog (no code changed): make test output
  self-explanatory via a one/two-line `TestContext.WriteLine(...)` per test. Queued as step 7.
- 2026-08-19 — **BUG-5 and BUG-8 done**, finishing step 3. BUG-5: `FileReader.Format` made
  settable, assigned in `OpenSpectrumFile`. BUG-8: `OpenSpectrumFile` now returns `Open()`'s
  actual result; deliberately left `FileName`/`ScanCount` assignment-on-failure behavior
  alone (out of scope). Flipped TEST-2's two malformed-file characterization tests to assert
  `false`. Verified: full solution build clean, `dotnet test` 28/28 passing.
- 2026-08-19 — **CLEAN-1 and CLEAN-2 done**, finishing step 4. CLEAN-1: deleted the
  unreachable duplicated boundary-check block in `TSpectrum.GetMz`. CLEAN-2: deleted the two
  redundant trailing lines in `ThermoRawReader.ProcessSpectrumInformation`. Verified: full
  solution build clean, `dotnet test` 28/28 passing.
- 2026-08-19 — Repo owner made the BUG-1/BUG-2 (MGF) decision: implement real MGF support,
  following the Matrix Science MGF format spec. Moved BUG-1/BUG-2 to the last item on the
  priority list. Docs only, no code changed.
- 2026-08-19 — **TEST-3 done.** Added a one-line `testContext.WriteLine(...)` at the top of
  all 28 test methods across all 4 test classes. `TestSpectrum`/`TestPipes`/
  `TestNovaIOFixtures` had no `TestContext` before this; added the public-property +
  constructor-injection pattern (rejected a private-field-only version after it triggered 3
  new `MSTEST0005` warnings). Confirmed the messages reach output via a real `.trx` run.
  Verified: full solution build clean, `dotnet test` 28/28 passing.
- 2026-08-19 — **BUG-4, BUG-6, CLEAN-3, HYG-2, HYG-3 done**, finishing step 5. BUG-4:
  `MzMLWriter.Write` gained `validateSchema`/`schemaPath` params, off/null by default. BUG-6:
  manual read loop in `PipeIO.Read`, throwing `EndOfStreamException` on an early-closed pipe.
  CLEAN-3: extracted `FileReader.CheckFileFormat`/`CreateReader` as the single dispatch source
  of truth. HYG-2: extracted the duplicated path-resolution walk into shared
  `Test/TestFilePaths.cs`, anchored to `AppContext.BaseDirectory` — caught a real bug doing
  this (trailing separator making the first `Directory.GetParent` call a no-op, breaking
  13/28 tests on the first attempt). HYG-3: `IChromatogram`/`IChromatDataPoint` made `public`.
  Verified: full solution build clean, `dotnet test` 28/28 passing.
- 2026-08-19 — Mid-session ask: surface each test's `TestContext.WriteLine` message directly
  in the GitHub Actions log, not just `.trx` (follow-up to TEST-3). Confirmed
  `--logger "console;verbosity=detailed"` shows them under a `TestContext Messages:` block —
  added that flag to the `Test` step in all three workflows.
- 2026-08-19 — **BUG-1 and BUG-2 done**, finishing step 7 (the last item on the list). Repo
  owner supplied `Test/Files/AngioNeuro4.mgf` and pointed at the Matrix Science MGF spec as
  the implementation reference. BUG-1: added the `MGF` case to `FileReader.CreateReader`.
  BUG-2: `MGFReader` fully rewritten — no built-in index, `Open()` reads the whole file into
  memory once and indexes `BEGIN IONS`/`END IONS` blocks by line number; scan numbers resolved
  from `SCANS=`, falling back to the TITLE convention, falling back to sequential numbering;
  implemented per spec (`PEPMASS=`, `CHARGE=`, `RTINSECONDS=`, peak lines); `MsLevel` always
  2; stats computed from parsed peaks; random access/sequential reads driven by file order.
  Also fixed a real bug in the old stub's header loop along the way. New finding, not fixed
  here: **HYG-5**. Added `Test/Files/AngioNeuro4Malformed.mgf` and `Test/TestMgf.cs` (9
  tests). Verified: full solution build clean (0 new warnings), `dotnet test` 37/37 passing.
  Updated `CLAUDE.md`'s "Known Gotchas" section.
- 2026-08-19 — **HYG-4 and HYG-5 done**, finishing step 8 and closing the whole list. HYG-4:
  closed as a tracking item — its own definition was always "no code change needed", already
  satisfied. HYG-5: added `NovaIO/StringExtensions.cs` and repointed `FileReader.cs`/
  `MzXMLReader.cs`/`MzMLWriter.cs` at it; also removed the now-genuinely-unused Thermo
  `using`s from all three files, verified empirically. Verified: full solution build clean
  (same 1 pre-existing warning, 0 new), `dotnet test` 37/37 passing.
- 2026-08-19 — **`dev-nuget.yml` "Latest dev build" asset naming fixed**, not a catalogued
  known-issues item, just a workflow usability bug reported directly. See CI-1's follow-up
  note above for the full detail.
- 2026-08-19 — **Docs reorganized.** Every item on the list was Done; `known-issues.md` and
  `progress.md` had grown to ~900 and ~325 lines respectively, almost entirely archival detail
  with no more open work to track. Created this file (`docs/history.md`) holding the full
  per-issue write-ups and the complete session log verbatim. Slimmed `known-issues.md` down to
  the severity guide, a one-line resolved-item index (for ID numbering continuity), and the
  contributing-instructions footer. Slimmed `progress.md` down to the summary table and a
  pointer here. No code changed — documentation-only reorganization.
- 2026-10-02 — **BUG-9, BUG-10 and TEST-4 done**, from a field report that mzML files over
  2 GiB could not be read. Confirmed the reporter's diagnosis exactly: `int.Parse`/
  `Convert.ToInt32` on byte offsets in `MzMLReader`/`MzXMLReader`. Widened every file byte
  position to `long` behind a new shared `ByteOffset.Parse` (invariant culture); made
  `SpectrumFileReaderFactory.GetReader` and `FileReader`'s three `Read*` overloads stop
  discarding the open result, throwing a new `SpectrumFileOpenException` instead, without
  changing `ISpectrumFileReader` or adding any public member to `ThermoRawReader`. Verified
  three ways: sparse-file fixtures at 2.5 and 4.5 GiB (0 bytes on disk, ~20 ms each), a
  negative control proving the new tests fail against the unfixed parse with the exact
  reported error text, and a real 3.18 GiB / 332,687-spectrum Astral mzML read end to end
  with peaks matching an independent decode. Found and fixed an orphaned `FileStream` on the
  new failure path; found and logged **BUG-11** (`Close()` is a no-op in both XML readers),
  left open. Tests 37 → 45, all passing; build clean, no new warnings. No version bump —
  `1.1.0` had not shipped as a non-prerelease release.
- 2026-10-05 — **HYG-6 done.** Repo owner spotted the misspelled public `FramentationType` /
  `PrecursorIon.FramentationMethod`. Renamed across all 13 occurrences, straight rename with
  no compatibility shims, chosen because 1.1.0 had not shipped and the IPC wire format never
  serialized that property (checked, not assumed). Breaking for package consumers; build
  clean, 45/45 passing.
- 2026-10-05 — **1.1.0 release notes written** into both `Nova.csproj` (which had none) and
  `NovaIO.csproj` (which still said "Initial release" and would have shipped that way).
  Verified by packing and reading the resulting `.nuspec`, not by reading the csproj XML. Also
  corrected `README.md`, which still told readers Nova must be on .NET Framework 4.8 for IAPI
  work — untrue since ARCH-1, and the first thing anyone reads before downloading. Noted while
  drafting: the IPC wire format changed in this release (commit `fb8dfd8` added
  `ScanDescription` to `Spectrum` serialization), so 1.1.0 and 1.0.0.18 desynchronize over a
  named pipe — a runtime failure, unlike the HYG-6 rename's compile error, so the notes lead
  with it.
- 2026-10-05 — **BUG-11 done**, plus the TEST-4 CI guard. Repo owner confirmed no downstream
  software has ever called `Close()`, which cleared the one question blocking the fix. Both
  XML readers now release their handle; `Open()` releases a prior one; `chrIndex` is cleared
  on re-open; `MGFReader.Close()` drops its line buffer. Large-file tests now fail rather than
  skip when `NOVA_REQUIRE_LARGE_FILE_TESTS` is set, and all three workflows set it. Both
  changes verified with negative controls — the old no-op `Close()` reproduces the original
  "file in use" error, and a forced-unavailable fixture skips without the variable and fails
  with it. Tests 45 → 48, all passing. **This closes every open item in the repo.**
