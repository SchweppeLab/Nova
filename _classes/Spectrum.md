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
A spectrum of basic m/z and intensity data points. It is TSpectrum&lt;SpecDataPoint&gt;.

#### Implements
ISpectrum&lt;SpecDataPoint&gt;, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| [Spectrum(int count = 0)]({{ '/methods/Spectrum.Constructor.html' | relative_url }}) | Creates a spectrum sized for the given number of data points.  |

* * *
## Properties

#### Inherited from [TSpectrum&lt;T&gt;]({{ '/classes/TSpectrum.html' | relative_url }})

{% include members.html of="TSpectrum" section="properties" t="SpecDataPoint" %}

#### Inherited from [SpectrumFoundation]({{ '/classes/SpectrumFoundation.html' | relative_url }})

{% include members.html of="SpectrumFoundation" section="properties" %}

* * *
## Methods

#### Inherited from [TSpectrum&lt;T&gt;]({{ '/classes/TSpectrum.html' | relative_url }})

{% include members.html of="TSpectrum" section="methods" t="SpecDataPoint" %}

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
