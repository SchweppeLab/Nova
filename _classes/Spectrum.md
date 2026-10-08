---
name: Spectrum
title: Spectrum
description: A spectrum of basic m/z and intensity data points.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [TSpectrum]
siblings: [SpectrumEx,TSpectrum]
---

<br/>
## Remarks
A spectrum of basic m/z and intensity data points. It is TSpectrum&lt;SpecDataPoint&gt;, so
its members are those of [TSpectrum&lt;T&gt;]({{ '/classes/TSpectrum.html' | relative_url }})
and its scan-level fields those of [SpectrumFoundation]({{ '/classes/SpectrumFoundation.html' | relative_url }}).

#### Implements
ISpectrum&lt;SpecDataPoint&gt;, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Spectrum(int count = 0) | Creates a spectrum sized for the given number of data points.  |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

int ms1Count = 0;
int ms2Count = 0;
FileReader reader = new FileReader("TheBestDataEver.mzML", MSFilter.MS1 | MSFilter.MS2);
foreach (Spectrum spec in reader)
{
  if (spec.MsLevel == 1)
  {
    ms1Count++;
  }
  else
  {
    ms2Count++;
  }
}
Console.WriteLine("There are " + ms1Count + " MS scans and " + ms2Count + " MS/MS scans.");
```
