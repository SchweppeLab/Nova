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

*Written from the implementation. Nova carries no comments for this member.*

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| CompareTo(SpecDataPoint x) | Performs the CompareTo function on the Mz of two SpecDataPoints to identify the lower value.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| x  | SpecDataPoint   | The data point to compare against.      |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| int | Less than zero if this point has the lower m/z, zero if the two are equal, greater than zero if this point has the higher m/z.  |

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
