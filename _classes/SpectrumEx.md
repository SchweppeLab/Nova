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
provides. It is TSpectrum&lt;SpecDataPointEx&gt;.

#### Implements
ISpectrum&lt;SpecDataPointEx&gt;, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| [SpectrumEx(int count = 0)]({{ '/methods/SpectrumEx.Constructor.html' | relative_url }}) | Creates a spectrum sized for the given number of data points.  |

* * *
## Properties

#### Inherited from [TSpectrum&lt;T&gt;]({{ '/classes/TSpectrum.html' | relative_url }})

{% include members.html of="TSpectrum" section="properties" t="SpecDataPointEx" %}

#### Inherited from [SpectrumFoundation]({{ '/classes/SpectrumFoundation.html' | relative_url }})

{% include members.html of="SpectrumFoundation" section="properties" %}

* * *
## Methods

#### Inherited from [TSpectrum&lt;T&gt;]({{ '/classes/TSpectrum.html' | relative_url }})

{% include members.html of="TSpectrum" section="methods" t="SpecDataPointEx" %}

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
