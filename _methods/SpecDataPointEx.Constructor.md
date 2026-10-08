---
name: SpecDataPointEx.Constructor
title: SpecDataPointEx.Constructor
member: Constructor
description: Creates an extended data point, with every value defaulting to zero.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: SpecDataPointEx
type: Method
siblings: [SpecDataPointEx, SpecDataPoint, ISpecDataPoint]
---

<br/>
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpecDataPointEx(double mz = 0, double intensity = 0, double noise = 0, double baseline = 0, int charge = 0, double resolution = 0) | Creates a data point, with every value defaulting to zero. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| mz | double | The m/z value. |
| intensity | double | The intensity value. |
| noise | double | The noise level at the peak. |
| baseline | double | The baseline at the peak. |
| charge | int | The assigned charge state. |
| resolution | double | The resolution at the peak. |

* * *
## Example

```csharp
using Nova.Data;

SpecDataPointEx peak = new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000);

// Named arguments set only what is needed.
SpecDataPointEx simple = new SpecDataPointEx(mz: 445.12, intensity: 10523.0);
```
