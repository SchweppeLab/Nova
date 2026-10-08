---
name: Chromatogram.Constructor
title: Chromatogram.Constructor
member: Constructor
description: Creates a chromatogram sized for the given number of data points.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: Chromatogram
type: Method
siblings: [Chromatogram, ChromatDataPoint, IChromatogram]
---

<br/>
## Remarks
Count is set to the given size, and DataPoints holds that many empty points.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Chromatogram(int count = 0) | Creates a chromatogram sized for the given number of data points. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| count | int | The number of data points. |

* * *
## Example

```csharp
using Nova.Data;

Chromatogram chromat = new Chromatogram(2);
chromat.ID = "TIC";
chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };
```
