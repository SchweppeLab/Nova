---
name: ISpecDataPoint.Write
title: ISpecDataPoint.Write
member: Write
description: Serializes the data point to its binary representation, as read back by Read.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: ISpecDataPoint
type: Method
siblings: [ISpecDataPoint, SpecDataPoint, SpecDataPointEx]
---

<br/>
## Remarks
Writes the data point's values at the writer's current position, in the layout
[Read]({{ '/methods/ISpecDataPoint.Read.html' | relative_url }}) expects. Each point type has its own layout: see
[SpecDataPoint.Write]({{ '/methods/SpecDataPoint.Write.html' | relative_url }}) and [SpecDataPointEx.Write]({{ '/methods/SpecDataPointEx.Write.html' | relative_url }}).

TSpectrum&lt;T&gt;.Serialize uses it to store each data point.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Write(BinaryWriter writer) | Serializes the data point to its binary representation, as read back by Read. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| writer | BinaryWriter | A writer positioned where this data point's values should go. |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

// Works for either point type.
static void WritePoints<T>(BinaryWriter writer, T[] points) where T : ISpecDataPoint
{
  foreach (T point in points)
  {
    point.Write(writer);
  }
}
```
