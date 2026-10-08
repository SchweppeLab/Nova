---
name: ChromatDataPoint.Write
title: ChromatDataPoint.Write
member: Write
description: Writes the data point to a BinaryWriter as two doubles.
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
Writes 16 bytes at the writer's current position: RT and then Intensity. This is the
layout [Read]({{ '/methods/ChromatDataPoint.Read.html' | relative_url }}) expects.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Write(BinaryWriter writer) | Writes the retention time then the intensity to the stream, each as a double. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| writer | BinaryWriter | A writer positioned where this data point's values should go. |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

ChromatDataPoint point = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };

using (MemoryStream stream = new MemoryStream())
using (BinaryWriter writer = new BinaryWriter(stream))
{
  point.Write(writer);
  byte[] bytes = stream.ToArray();   // 16 bytes
}
```
