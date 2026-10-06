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
Reads 16 bytes from the reader's current position and assigns them to Mz and Intensity,
in that order. It overwrites both properties, so it replaces the contents of the point
rather than adding to them.

The layout is the one [SpecDataPoint.Write](/methods/SpecDataPoint.Write.html)
produces. Reading bytes written by SpecDataPointEx gives silently wrong values, because
that type writes six fields and the first two are not enough to tell them apart.

Nothing validates the stream. A reader positioned at the wrong offset returns whatever
the next sixteen bytes happen to mean.

> The library carries no documentation comments for this member. This page is written
> from the implementation at the release tag.
{: .prompt-info }

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
