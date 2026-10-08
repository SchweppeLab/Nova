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

*Written from the implementation. Nova carries no comments for this member.*

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Read(BinaryReader reader) | Reads the Mz and Intensity value from a BinaryReader.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| reader  | BinaryReader   | The reader to read from. Reading starts at its current position.      |

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
