---
name: ISpecDataPoint.Read
title: ISpecDataPoint.Read
member: Read
description: Populates the data point from its binary representation, as written by Write.
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
Reads the data point's values from the reader's current position, overwriting them, in
the layout [Write]({{ '/methods/ISpecDataPoint.Write.html' | relative_url }}) produces. Each point type has its own layout:
see [SpecDataPoint.Read]({{ '/methods/SpecDataPoint.Read.html' | relative_url }}) and [SpecDataPointEx.Read]({{ '/methods/SpecDataPointEx.Read.html' | relative_url }}).

TSpectrum&lt;T&gt;.Deserialize uses it to restore each data point.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Read(BinaryReader reader) | Populates the data point from its binary representation, as written by Write. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| reader | BinaryReader | A reader positioned at the start of this data point's values. |

* * *
## Example

```csharp
using System.IO;
using Nova.Data;

// Works for either point type.
static T ReadPoint<T>(BinaryReader reader) where T : ISpecDataPoint, new()
{
  T point = new T();
  point.Read(reader);
  return point;
}
```
