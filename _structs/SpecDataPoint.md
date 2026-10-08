---
name: SpecDataPoint
title: SpecDataPoint
description: The basic mass spectrum data point, an m/z and intensity pair.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Struct
interfaces: [ISpecDataPoint]
siblings: [SpecDataPointEx]
---

<br/>
## Remarks
The basic mass spectrum data point: an m/z and intensity pair. This is the point type of
Spectrum.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| [SpecDataPoint(double mz = 0, double intensity = 0)]({{ '/methods/SpecDataPoint.Constructor.html' | relative_url }}) | Creates a data point, with both values defaulting to zero.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Mz           | double   | The mass to charge ratio (m/z) value for this data point.   |
| Intensity    | double   | The intensity value for this data point.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [CompareTo(SpecDataPoint x)]({{ '/methods/SpecDataPoint.CompareTo.html' | relative_url }}) | int    | Orders data points by m/z alone. Two points at the same m/z compare as equal.   |
| [Read(BinaryReader reader)]({{ '/methods/SpecDataPoint.Read.html' | relative_url }})      | void   | Reads the m/z then the intensity from the stream, each as a double.   |
| [Write(BinaryWriter writer)]({{ '/methods/SpecDataPoint.Write.html' | relative_url }})     | void   | Writes the m/z then the intensity to the stream, each as a double.   |

* * *
## Example

```csharp
using Nova.Data;

SpecDataPoint[] peaks =
{
  new SpecDataPoint(600.30, 2000.0),
  new SpecDataPoint(445.12, 10523.0),
  new SpecDataPoint(522.77, 850.0)
};

// IComparable orders by Mz, so no comparer is needed.
Array.Sort(peaks);

foreach (SpecDataPoint peak in peaks)
{
  Console.WriteLine(peak.Mz);
}
```
