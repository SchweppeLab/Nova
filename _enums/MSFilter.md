---
name: MSFilter
title: MSFilter
description: Bitwise enumerator for filtering scans by type.
date: 2025-04-22 13:15:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Enums
interfaces: []
siblings: [FileReader, ISpectrumFileReader]
---

<br/>
## Remarks
Bitwise enumerator for filtering scans by type. Combine values with | to read more than one
MS level.

* * *
## Fields

| Name   | Value     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| None     | 0   | No MS levels.   |
| MS1      | 1   | MS1 spectra.   |
| MS2      | 2   | MS2 spectra.   |
| MS3      | 4   | MS3 spectra.   |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

// Read MS1 and MS2 spectra, skipping MS3.
FileReader reader = new FileReader("DDA.raw", MSFilter.MS1 | MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  Console.WriteLine(spectrum.ScanNumber + "\tMS" + spectrum.MsLevel);
}
```
