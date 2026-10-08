---
name: FileReader.ReadSpectrumEx
title: FileReader.ReadSpectrumEx
member: ReadSpectrumEx
description: Reads a spectrum with the extended per-peak data, opening the file first if it is not the current one.
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
With a scan number, that scan is read, or an empty spectrum if it does not pass the filter.
With -1, the next spectrum that passes the filter is read, and an empty spectrum, with
ScanNumber 0, marks the end of the file.

Only Thermo RAW files provide the extended data. Other formats leave it at zero.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| ReadSpectrumEx(string fileName = "", int scanNumber = -1, bool centroid = true) | Reads a spectrum with the extended per-peak data where the format provides it, opening the file first if it is not the current one. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | The file to read from, or empty for the current file. |
| scanNumber | int | The scan number, or -1 for the next spectrum. |
| centroid | bool | The preferred peak type. Not guaranteed; check the Spectrum.Centroid property. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumEx | The spectrum, or an empty one if it could not be read. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumFileOpenException | A new file could not be opened. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader(MSFilter.MS1);

// The first call names the file; later calls keep reading it.
SpectrumEx spectrum = reader.ReadSpectrumEx("DDA.raw");
while (spectrum.ScanNumber > 0)
{
  Console.WriteLine(spectrum.ScanNumber + ": " + spectrum.Count + " peaks");
  spectrum = reader.ReadSpectrumEx();
}
```
