---
name: Chromatogram.Serialize
title: Chromatogram.Serialize
member: Serialize
description: Converts the chromatogram to a byte array.
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
Converts the chromatogram to a byte array, for storage or for sending to another process.
[Deserialize]({{ '/methods/Chromatogram.Deserialize.html' | relative_url }}) turns the array back into a chromatogram.

ID is not carried.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Serialize() | Writes the count as a 32-bit integer, then each data point via ChromatDataPoint.Write. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| byte[] | The serialized chromatogram. |

* * *
## Example

```csharp
using Nova.Data;

Chromatogram chromat = new Chromatogram(2);
chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };

byte[] payload = chromat.Serialize();   // store it, or send it in a PipeMessage
```
