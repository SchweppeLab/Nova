---
name: MzMLWriter.AddSpectrum
title: MzMLWriter.AddSpectrum
member: AddSpectrum
description: Converts a spectrum object into a spectrum element.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Write
class: MzMLWriter
type: Method
siblings: [MzMLWriter, Spectrum]
---

<br/>
## Remarks
The spectrum joins the run most recently started with AddRun, which must be called first.
Spectra are written in the order they are added.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| AddSpectrum(Spectrum spec) | Converts a spectrum object into a spectrum element. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| spec | Spectrum | The spectrum to add. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
writer.AddInstrumentConfiguration("IC1", null);
writer.AddRun("run1", "IC1");

// Keep only the MS2 spectra.
FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  writer.AddSpectrum(spectrum);
}
writer.Write("DDA_ms2.mzML");
```
