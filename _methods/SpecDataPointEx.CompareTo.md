---
name: SpecDataPointEx.CompareTo
title: SpecDataPointEx.CompareTo
member: CompareTo
description: Orders data points by m/z alone.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: SpecDataPointEx
type: Method
siblings: [SpecDataPointEx, SpecDataPoint, ISpecDataPoint]
---

<br/>
## Remarks
This is the IComparable implementation, so Array.Sort and List.Sort put a set of points
into m/z order without a comparer being supplied.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| CompareTo(SpecDataPointEx x) | Orders data points by m/z alone. Two points at the same m/z compare as equal. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| x | SpecDataPointEx | The data point to compare against. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| int | Negative, zero or positive as this point's m/z is less than, equal to or greater than x's. |

* * *
## Example

```csharp
using System;
using Nova.Data;

SpecDataPointEx[] peaks =
{
  new SpecDataPointEx(600.30, 2000.0),
  new SpecDataPointEx(445.12, 10523.0),
  new SpecDataPointEx(522.77, 850.0)
};

Array.Sort(peaks);
```
