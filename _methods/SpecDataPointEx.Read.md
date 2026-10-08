---
name: SpecDataPointEx.Read
title: SpecDataPointEx.Read
member: Read
description: Reads an extended data point from a BinaryReader.
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
Reads Mz, Intensity, Noise and Baseline as doubles, Charge as a 32-bit integer, then
Resolution as a double, overwriting all six. This is the layout
[Write]({{ '/methods/SpecDataPointEx.Write.html' | relative_url }}) produces.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Read(BinaryReader reader) | Reads an extended mass spectrum data point. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| reader | BinaryReader | A reader positioned at the start of this data point's values. |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

byte[] bytes;
using (MemoryStream stream = new MemoryStream())
using (BinaryWriter writer = new BinaryWriter(stream))
{
  new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000).Write(writer);
  bytes = stream.ToArray();
}

SpecDataPointEx peak = new SpecDataPointEx();
using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
{
  peak.Read(reader);
}
```
