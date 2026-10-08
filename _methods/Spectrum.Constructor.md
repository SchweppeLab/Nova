---
name: Spectrum.Constructor
title: Spectrum.Constructor
member: Constructor
description: Creates a spectrum sized for the given number of data points.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: Spectrum
type: Method
siblings: [Spectrum, SpectrumEx, TSpectrum]
---

<br/>
## Remarks
Count is set to the given size, and DataPoints holds that many empty points.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Spectrum(int count = 0) | Creates a spectrum sized for the given number of data points. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| count | int | The number of data points. |

* * *
## Example

```csharp
using Nova.Data;

Spectrum spectrum = new Spectrum(2);
spectrum.ScanNumber = 1;
spectrum.MsLevel = 1;
spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);
```
