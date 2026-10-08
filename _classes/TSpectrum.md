---
name: TSpectrum
title: TSpectrum&lt;T>
description: The spectrum implementation shared by Spectrum and SpectrumEx.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [SpectrumFoundation]
siblings: [Spectrum,SpectrumEx,SpectrumFoundation]
---

<br/>
## Remarks
The spectrum implementation shared by Spectrum and SpectrumEx: the scan-level fields of
SpectrumFoundation plus the data points. T is the data point type, SpecDataPoint or
SpecDataPointEx.

#### Implements
ISpectrum&lt;T&gt;, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| TSpectrum(int count = 0) | Creates a spectrum sized for the given number of data points.  |

* * *
## Properties

{% include members.html of="TSpectrum" section="properties" %}

#### Inherited from [SpectrumFoundation]({{ '/classes/SpectrumFoundation.html' | relative_url }})

{% include members.html of="SpectrumFoundation" section="properties" %}

* * *
## Methods

{% include members.html of="TSpectrum" section="methods" %}

* * *
## Example

```csharp
using Nova.Data;

Spectrum spectrum = new Spectrum();

// Assigning DataPoints directly does not update Count, so size with Resize.
spectrum.Resize(2);
spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);

byte[] payload = spectrum.Serialize();

Spectrum copy = new Spectrum();
copy.Deserialize(payload);
Console.WriteLine(copy.Count);
```
