---
name: SpectrumEx.Constructor
title: SpectrumEx.Constructor
member: Constructor
description: Creates an extended spectrum sized for the given number of data points.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: SpectrumEx
type: Method
siblings: [SpectrumEx, Spectrum, TSpectrum]
---

<br/>
## Remarks
Count is set to the given size, and DataPoints holds that many empty points.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumEx(int count = 0) | Creates a spectrum sized for the given number of data points. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| count | int | The number of data points. |

* * *
## Example

```csharp
using Nova.Data;

SpectrumEx spectrum = new SpectrumEx(1);
spectrum.DataPoints[0] = new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000);
```
