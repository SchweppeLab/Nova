---
name: ChromatDataPoint
title: ChromatDataPoint
description: A retention time and intensity pair; the point type of Chromatogram.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Struct
interfaces: [IChromatDataPoint]
siblings: [Chromatogram, IChromatDataPoint]
---

<br/>
## Remarks
The chromatogram data point: a retention time and intensity pair. This is the point type
of Chromatogram.

#### Implements
IChromatDataPoint

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Intensity    | double   | The intensity value for this data point.   |
| RT           | double   | The retention time for this data point.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [Read(BinaryReader reader)]({{ '/methods/ChromatDataPoint.Read.html' | relative_url }})      | void   | Reads the retention time then the intensity from the stream, each as a double.   |
| [Write(BinaryWriter writer)]({{ '/methods/ChromatDataPoint.Write.html' | relative_url }})     | void   | Writes the retention time then the intensity to the stream, each as a double.   |

* * *
## Example

```csharp
using Nova.Data;

ChromatDataPoint point = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };
```
