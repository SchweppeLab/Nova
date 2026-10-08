---
name: Chromatogram.Dispose
title: Chromatogram.Dispose
member: Dispose
description: A no-op beyond marking the instance disposed.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: Chromatogram
type: Method
siblings: [Chromatogram, ChromatDataPoint, IChromatogram]
---

<br/>
## Remarks
A chromatogram holds no unmanaged resources, so disposing it is optional. It is
IDisposable so it can be used the same way as the spectrum types.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Dispose() | A no-op beyond marking the instance disposed. Present so a chromatogram can be used the same way as the spectrum types, which share this pattern. |

* * *
## Example

```csharp
using Nova.Data;

using (Chromatogram chromat = new Chromatogram(100))
{
  Console.WriteLine(chromat.Count);
}
```
