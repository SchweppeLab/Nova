---
name: SpecDataPoint.Write
title: SpecDataPoint.Write
member: Write
description: Writes the data point to a BinaryWriter as two doubles.
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
Writes 16 bytes at the writer's current position: Mz first, then Intensity, each as a
double. No length prefix, no type tag, no terminator.

This is the layout [SpecDataPoint.Read]({{ '/methods/SpecDataPoint.Read.html' | relative_url }}) expects.
SpecDataPointEx uses a different one, with six fields.

*Written from the implementation. Nova carries no comments for this member.*

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Write(BinaryWriter writer) | Writes the Mz and Intensity value to a BinaryWriter.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| writer  | BinaryWriter   | The writer to write to. Writing starts at its current position.      |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

SpecDataPoint peak = new SpecDataPoint(445.12, 10523.0);

using (MemoryStream stream = new MemoryStream())
using (BinaryWriter writer = new BinaryWriter(stream))
{
  peak.Write(writer);

  // 16 bytes: Mz then Intensity, each a little-endian double.
  byte[] bytes = stream.ToArray();
}
```
