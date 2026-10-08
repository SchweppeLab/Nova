---
name: SpectrumFoundation
title: SpectrumFoundation
description: The scan-level fields of a mass spectrum, independent of its data point type.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: []
classes: []
siblings: [TSpectrum, Spectrum, SpectrumEx, PrecursorIon]
---

<br/>
## Remarks
The scan-level fields of a mass spectrum, independent of its data point type. It is the
base of TSpectrum&lt;T&gt;, so every field here is available on Spectrum and SpectrumEx.

This class is abstract and is not constructed directly.

* * *
## Properties

{% include members.html of="SpectrumFoundation" section="properties" %}

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  Console.WriteLine(spectrum.ScanNumber + "\t" + spectrum.RetentionTime
    + "\tMS" + spectrum.MsLevel + "\t" + spectrum.TotalIonCurrent);
}
```
