---
name: SpecDataPoint.Constructor
title: SpecDataPoint Constructor
member: Constructor
description: Creates a data point from an m/z and an intensity.
date: 2026-10-06 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: SpecDataPoint
type: Method
siblings: [SpecDataPoint, SpecDataPointEx, ISpecDataPoint]
---

<br/>
## Remarks
Both parameters default to zero, so `new SpecDataPoint()` is valid and produces a point
at zero intensity.

SpecDataPoint is a struct, so an uninitialized array element is already a zeroed point
rather than a null reference.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpecDataPoint(double mz = 0, double intensity = 0) | Creates a data point, with both values defaulting to zero.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| mz  | double   | The m/z value.      |
| intensity  | double   | The intensity value.      |

* * *
## Example

```csharp
using Nova.Data;

SpecDataPoint peak = new SpecDataPoint(445.12, 10523.0);

// Both parameters default to zero, so this is a valid empty point.
SpecDataPoint empty = new SpecDataPoint();
```
