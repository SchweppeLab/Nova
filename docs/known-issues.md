# Known Issues

Tracks currently-open bugs, dead/redundant code, and hygiene problems in this repo. When you
fix something here, update its status in [`progress.md`](progress.md) in the same change.

Severity guide: **High** = incorrect behavior or crash reachable in normal use. **Medium** =
incorrect/misleading behavior in a less common path, or a real (if narrow) correctness risk.
**Low** = code quality, dead code, or maintainability concern with no behavioral impact today.
Architecture items use **Priority** instead of Severity — they're initiatives, not defects.

Everything from the initial 2026-08-18 review pass was resolved by 2026-08-19, and the
2026-10-02 large-file pass (BUG-9, BUG-10, TEST-4) is resolved too. The full write-up for each
— background, decision, fix, and verification — lives in [`history.md`](history.md), organized
by the same categories as below. Add new items here as they come up; the next available ID in
each category is noted below so IDs don't collide with the archived ones.

| Category | Resolved IDs (see history.md) | Next ID |
|---|---|---|
| Architecture / Framework Targeting | ARCH-1 | ARCH-2 |
| Bugs | BUG-1 – BUG-10 | BUG-12 |
| Dead / Redundant Code | CLEAN-1 – CLEAN-3 | CLEAN-4 |
| CI / Build Infrastructure | CI-1 | CI-2 |
| Hygiene / Maintainability | HYG-1 – HYG-6 | HYG-7 |
| Test Coverage | TEST-1 – TEST-4 | TEST-5 |

---

## Open Items

### BUG-11 — `MzMLReader.Close()`/`MzXMLReader.Close()` are no-ops and never release the file handle
**Severity:** Medium · **Status: Not Started** · *Found 2026-10-02 while fixing BUG-10.*
**Location:** `NovaIO/Io/Read/MzMLReader.cs`, `NovaIO/Io/Read/MzXMLReader.cs`, `Close()`

Both readers hold the open mzML/mzXML in a `FileStream` field (`XmlFS`) for random access, but
`Close()` in both is an empty method whose only content is a commented-out line copied from
`ThermoRawReader`:

```csharp
public void Close()
{
  //if (RawFile != null) RawFile.Dispose();
}
```

`XmlFS` is therefore never disposed, and the handle survives until the finalizer runs. This is
reachable in normal use: `FileReader.ReadSpectrum`/`ReadChromatogram`/`ReadSpectrumEx` call
`fileReader.Close()` every time the caller switches to a different file, expecting that to
release the previous one. A long-running process that iterates over many files leaks a handle
per file, and on Windows the leaked handle also keeps the file locked against deletion or
rename by anything else.

This was found concretely: a scratch harness could not delete its own temp file after Nova had
opened it. The *failure* path was fixed under BUG-10 (both readers now dispose `XmlFS` when
`Open` fails, because a failed open now throws and the caller never gets the reader back), but
the **success** path — the normal one — is untouched and still leaks.

**Suggested fix:** dispose `XmlFS` (and null it) in `Close()` in both readers, and make
`Open()` dispose any stream left over from a previous open on the same instance. Worth
checking whether anything downstream (Helios, GoDig, Scops) currently calls `Close()` and then
keeps reading from the same reader — that works today purely because `Close()` does nothing,
and would start failing once it actually closes. That question is why this is logged rather
than fixed in the same pass as BUG-10.

---

## Contributing to This List

If you find a new issue while working in this repo, add it here with the next available ID
in its category (see the table above), then add a corresponding row in
[`progress.md`](progress.md). Keep entries concrete: what's wrong, where, why it matters, and
a suggested fix — not just "this looks odd." Once an item is fixed and verified, move its
full write-up into [`history.md`](history.md) and leave just its ID in the table above (bump
the "Next ID" accordingly), so this file stays a lean, current-state view of what's actually
open.
