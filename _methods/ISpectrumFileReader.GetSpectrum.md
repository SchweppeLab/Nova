---
name: ISpectrumFileReader.GetSpectrum
title: ISpectrumFileReader.GetSpectrum
member: GetSpectrum
description: Reads a spectrum from the currently open MS data file.
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

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| GetSpectrum(int scanNumber = -1, bool centroid = true) | Reads a spectrum from the currently open MS data file. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| scanNumber | int | Optional scan number to grab. If omitted (or negative), the next spectrum in the file is loaded. |
| centroid | bool | Indicates the preferred peak type. Note that this preference is not guaranteed and should be checked using the Spectrum.Centroid property. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Spectrum | Spectrum object. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
Spectrum spectrum = reader.GetSpectrum();
while (spectrum.ScanNumber > 0)
{
  Console.WriteLine(spectrum.ScanNumber + ": " + spectrum.Count + " peaks");
  spectrum = reader.GetSpectrum();
}
reader.Close();
```
