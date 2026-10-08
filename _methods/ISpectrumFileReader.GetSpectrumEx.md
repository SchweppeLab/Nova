---
name: ISpectrumFileReader.GetSpectrumEx
title: ISpectrumFileReader.GetSpectrumEx
member: GetSpectrumEx
description: Reads a spectrum, with the extended per-peak data, from the currently open MS data file.
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
With a scan number, that scan is read, or an empty spectrum if it does not pass the filter.
With -1, the next spectrum that passes the filter is read, and an empty spectrum, with
ScanNumber 0, marks the end of the file.

Only Thermo RAW files provide the extended data. Other formats leave it at zero.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| GetSpectrumEx(int scanNumber = -1, bool centroid = true) | Reads a spectrum from the currently open MS data file, with the extended per-peak data where the format provides it. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| scanNumber | int | Optional scan number to grab. If omitted (or negative), the next spectrum in the file is loaded. |
| centroid | bool | Indicates the preferred peak type. Not guaranteed; check the Spectrum.Centroid property. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumEx | SpectrumEx object. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.raw", MSFilter.MS2);
SpectrumEx spectrum = reader.GetSpectrumEx(1000);
for (int i = 0; i < spectrum.Count; i++)
{
  Console.WriteLine(spectrum.DataPoints[i].Mz + " at resolution " + spectrum.DataPoints[i].Resolution);
}
reader.Close();
```
