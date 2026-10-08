---
name: SpecDataPoint.CompareTo
title: SpecDataPoint.CompareTo
member: CompareTo
description: Orders two data points by m/z. Intensity is ignored.
date: 2026-10-06 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: SpecDataPoint
type: Method
siblings: [SpecDataPoint, SpecDataPointEx]
---

<br/>
## Remarks
Orders by Mz. This is the IComparable implementation, so Array.Sort and List.Sort put a
set of points into m/z order without a comparer being supplied.

Points with equal Mz compare equal.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| CompareTo(SpecDataPoint x) | Orders data points by m/z alone. Two points at the same m/z compare as equal.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| x  | SpecDataPoint   | The data point to compare against.      |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| int | Negative, zero or positive as this point's m/z is less than, equal to or greater than x's.  |

* * *
## Example

```csharp
using System;
using Nova.Data;

SpecDataPoint[] peaks =
{
  new SpecDataPoint(600.30, 2000.0),
  new SpecDataPoint(445.12, 10523.0),
  new SpecDataPoint(522.77, 850.0)
};

// IComparable orders by Mz, so no comparer is needed.
Array.Sort(peaks);
```
