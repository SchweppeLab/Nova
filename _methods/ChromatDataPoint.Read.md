---
name: ChromatDataPoint.Read
title: ChromatDataPoint.Read
member: Read
description: Reads the data point from a BinaryReader as two doubles.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: ChromatDataPoint
type: Method
siblings: [ChromatDataPoint, IChromatDataPoint, Chromatogram]
---

<br/>
## Remarks
Reads 16 bytes from the reader's current position into RT and then Intensity,
overwriting both. This is the layout [Write]({{ '/methods/ChromatDataPoint.Write.html' | relative_url }}) produces.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Read(BinaryReader reader) | Reads the retention time then the intensity from the stream, each as a double. |

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
  new ChromatDataPoint { RT = 10.5, Intensity = 5400 }.Write(writer);
  bytes = stream.ToArray();
}

ChromatDataPoint point = new ChromatDataPoint();
using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
{
  point.Read(reader);
}
```
