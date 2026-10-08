---
name: SpecDataPoint.Read
title: SpecDataPoint.Read
member: Read
description: Reads the data point from a BinaryReader as two doubles.
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
Reads 16 bytes from the reader's current position into Mz and then Intensity,
overwriting both.

This is the layout [SpecDataPoint.Write]({{ '/methods/SpecDataPoint.Write.html' | relative_url }}) produces.
SpecDataPointEx uses a different one, with six fields.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Read(BinaryReader reader) | Reads the m/z then the intensity from the stream, each as a double.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| reader  | BinaryReader   | A reader positioned at the start of this data point's values.      |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

SpecDataPoint peak = new SpecDataPoint();

using (MemoryStream stream = new MemoryStream(bytes))
using (BinaryReader reader = new BinaryReader(stream))
{
  // Reads in the same order Write produced, from the current position.
  peak.Read(reader);
}
```
