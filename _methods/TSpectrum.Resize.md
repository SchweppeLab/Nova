---
name: TSpectrum.Resize
title: TSpectrum&lt;T>.Resize
member: Resize
description: Replaces the data points with a new, empty array of the given size.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: TSpectrum
type: Method
siblings: [TSpectrum, Spectrum, SpectrumEx]
---

<br/>
## Remarks
Existing points are discarded, and Count is set to the new size. Use Resize rather than assigning DataPoints directly, which does not update Count.

Inherited by [Spectrum]({{ '/classes/Spectrum.html' | relative_url }}) and [SpectrumEx]({{ '/classes/SpectrumEx.html' | relative_url }}).

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Resize(int sz) | Replaces the data points with a new, empty array of the given size. Existing points are discarded.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| sz  | int   | The new number of data points.   |

* * *
## Example

```csharp
using Nova.Data;

Spectrum spectrum = new Spectrum();

// Assigning DataPoints directly does not update Count, so size with Resize.
spectrum.Resize(2);
spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);
Console.WriteLine(spectrum.Count);          // 2
```
