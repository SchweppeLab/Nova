---
name: FileReader.ReadChromatogram
title: FileReader.ReadChromatogram
member: ReadChromatogram
description: Reads a chromatogram, opening the file first if it is not the current one.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
class: FileReader
type: Method
siblings: [FileReader, FileFormat, MSFilter]
---

<br/>
## Remarks
ChromatCount gives the number of chromatograms in the open file. Formats that carry none
report 0.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| ReadChromatogram(string fileName = "", int chromatIndex = -1) | Reads a chromatogram, opening the file first if it is not the current one. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | The file to read from, or empty for the current file. |
| chromatIndex | int | The chromatogram index (zero based), or -1 for the next one. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Chromatogram | The chromatogram, or an empty one if it could not be read. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumFileOpenException | A new file could not be opened. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.mzML");
for (int i = 0; i < reader.ChromatCount; i++)
{
  Chromatogram chromat = reader.ReadChromatogram("", i);
  Console.WriteLine(chromat.ID + ": " + chromat.Count + " points");
}
```
