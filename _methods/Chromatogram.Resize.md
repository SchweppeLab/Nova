---
name: Chromatogram.Resize
title: Chromatogram.Resize
member: Resize
description: Replaces the data points with a new, empty array of the given size.
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
Existing points are discarded, and Count is set to the new size. Use Resize rather than
assigning DataPoints directly, which does not update Count.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Resize(int sz) | Replaces the data points with a new, empty array of the given size. Existing points are discarded. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| sz | int | The new number of data points. |

* * *
## Example

```csharp
using Nova.Data;

Chromatogram chromat = new Chromatogram();
chromat.Resize(3);
chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };
chromat.DataPoints[2] = new ChromatDataPoint { RT = 11.0, Intensity = 2100 };
Console.WriteLine(chromat.Count);   // 3
```
