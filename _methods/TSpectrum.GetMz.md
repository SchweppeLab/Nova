---
name: TSpectrum.GetMz
title: TSpectrum&lt;T>.GetMz
member: GetMz
description: Finds the data point at an m/z, or the nearest one within a tolerance.
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
Uses a binary search, so DataPoints must be sorted by ascending m/z, the order the point types' CompareTo defines. If both neighbors of a miss are within tolerance, the lower-m/z one is returned.

Inherited by [Spectrum]({{ '/classes/Spectrum.html' | relative_url }}) and [SpectrumEx]({{ '/classes/SpectrumEx.html' | relative_url }}).

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| GetMz(double mz, double ppm = 0) | Finds the data point at an m/z, or the nearest one within a tolerance.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| mz  | double   | The m/z to look for.   |
| ppm  | double   | Tolerance in parts per million; 0 means an exact match only.   |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| int | The index into DataPoints, or -1 if the spectrum is empty or no point is within tolerance.  |

* * *
## Example

```csharp
using Nova.Data;

Spectrum spectrum = new Spectrum(3);
spectrum.DataPoints[0] = new SpecDataPoint(445.1200, 10523.0);
spectrum.DataPoints[1] = new SpecDataPoint(522.7700, 850.0);
spectrum.DataPoints[2] = new SpecDataPoint(600.3000, 2000.0);

int exact = spectrum.GetMz(522.7700);       // 1
int near = spectrum.GetMz(522.7720, 10);    // 1: 3.8 ppm away
int miss = spectrum.GetMz(522.7800, 10);    // -1: 19 ppm away
```
