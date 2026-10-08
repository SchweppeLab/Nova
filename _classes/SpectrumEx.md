---
name: SpectrumEx
title: SpectrumEx
description: A spectrum of extended data points with per-peak attributes from a Thermo centroid stream.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [TSpectrum]
siblings: [Spectrum,TSpectrum]
---

<br/>
## Remarks
A spectrum of extended data points carrying the per-peak attributes a Thermo centroid stream
provides. It is TSpectrum&lt;SpecDataPointEx&gt;, so its members are those of
[TSpectrum&lt;T&gt;]({{ '/classes/TSpectrum.html' | relative_url }}) and its scan-level fields
those of [SpectrumFoundation]({{ '/classes/SpectrumFoundation.html' | relative_url }}).

#### Implements
ISpectrum&lt;SpecDataPointEx&gt;, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumEx(int count = 0) | Creates a spectrum sized for the given number of data points.  |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS1);
SpectrumEx spectrum = reader.ReadSpectrumEx("", 1);

int charged = 0;
for (int i = 0; i < spectrum.Count; i++)
{
  if (spectrum.DataPoints[i].Charge > 0) charged++;
}
Console.WriteLine(charged + " of " + spectrum.Count + " peaks have an assigned charge.");
```
