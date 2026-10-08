---
name: SpecDataPointEx.Write
title: SpecDataPointEx.Write
member: Write
description: Writes an extended data point to a BinaryWriter.
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
Writes Mz, Intensity, Noise and Baseline as doubles, Charge as a 32-bit integer, then
Resolution as a double. This is the layout
[Read]({{ '/methods/SpecDataPointEx.Read.html' | relative_url }}) expects.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Write(BinaryWriter writer) | Writes an extended mass spectrum data point. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| writer | BinaryWriter | A writer positioned where this data point's values should go. |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

SpecDataPointEx peak = new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000);

using (MemoryStream stream = new MemoryStream())
using (BinaryWriter writer = new BinaryWriter(stream))
{
  peak.Write(writer);
  byte[] bytes = stream.ToArray();   // 44 bytes
}
```
