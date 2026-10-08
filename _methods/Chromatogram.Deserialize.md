---
name: Chromatogram.Deserialize
title: Chromatogram.Deserialize
member: Deserialize
description: Turns a byte array made by Serialize back into a chromatogram.
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
Turns a byte array made by [Serialize]({{ '/methods/Chromatogram.Serialize.html' | relative_url }}) back into a chromatogram.
The existing data points are replaced. ID is not carried, so it is left as it was.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Deserialize(byte[] data) | Reads the count as a 32-bit integer, then each data point via ChromatDataPoint.Read. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| data | byte[] | A byte array produced by Serialize. |

* * *
## Example

```csharp
using Nova.Data;

Chromatogram chromat = new Chromatogram(1);
chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
byte[] payload = chromat.Serialize();

Chromatogram copy = new Chromatogram();
copy.Deserialize(payload);
Console.WriteLine(copy.Count);   // 1
```
