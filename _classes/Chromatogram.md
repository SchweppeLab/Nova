---
name: Chromatogram
title: Chromatogram
description: A run of retention time and intensity data points.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: [IChromatogram]
classes: []
siblings: [IChromatogram, ChromatDataPoint, FileReader]
---

<br/>
## Remarks
A chromatogram: a run of retention time and intensity data points, with the identifier
the source gave it.

FileReader.ReadChromatogram returns one, and FileReader.ChromatCount says how many the
open file holds.

#### Implements
IChromatogram

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Chromatogram(int count = 0) | Creates a chromatogram sized for the given number of data points.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Count        | int      | The number of data points.   |
| DataPoints   | ChromatDataPoint[]   | The data points. Assigning a new array here directly does not update Count; use Resize.   |
| ID           | string   | The identifier the source gave this chromatogram, such as "TIC". Not carried by Serialize.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Deserialize(byte[] data)      | void   | Reads the count as a 32-bit integer, then each data point via ChromatDataPoint.Read.   |
| Dispose()      | void   | A no-op beyond marking the instance disposed. Present so a chromatogram can be used the same way as the spectrum types.   |
| Resize(int sz)      | void   | Replaces the data points with a new, empty array of the given size. Existing points are discarded.   |
| Serialize()      | byte[]   | Writes the count as a 32-bit integer, then each data point via ChromatDataPoint.Write.   |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.mzML");

for (int i = 0; i < reader.ChromatCount; i++)
{
  Chromatogram chromat = reader.ReadChromatogram("", i);
  Console.WriteLine(chromat.ID + ": " + chromat.Count + " points");

  for (int j = 0; j < chromat.Count; j++)
  {
    ChromatDataPoint point = chromat.DataPoints[j];
    Console.WriteLine(point.RT + "\t" + point.Intensity);
  }
}
```

Assigning DataPoints directly does not update Count, so size a new chromatogram with
Resize.

```csharp
Chromatogram chromat = new Chromatogram();
chromat.Resize(3);
chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
```
