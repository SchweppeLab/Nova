---
name: ISpectrumFileReader.GetChromatogram
title: ISpectrumFileReader.GetChromatogram
member: GetChromatogram
description: Reads a chromatogram from the currently open MS data file.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
class: ISpectrumFileReader
type: Method
siblings: [ISpectrumFileReader, SpectrumFileReaderFactory]
---

<br/>
## Remarks
ChromatCount gives the number of chromatograms in the open file. Formats that carry none
report 0.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| GetChromatogram(int chromatIndex = -1) | Reads a chromatogram from the currently open MS data file. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| chromatIndex | int | Optional chromatogram index (zero based) to read. If omitted (or negative), the next chromatogram in the file is read. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Chromatogram | Chromatogram object. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS1);
for (int i = 0; i < reader.ChromatCount; i++)
{
  Chromatogram chromat = reader.GetChromatogram(i);
  Console.WriteLine(chromat.ID + ": " + chromat.Count + " points");
}
reader.Close();
```
