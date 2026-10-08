---
name: ISpectrum
title: ISpectrum&lt;T>
description: The contract for a mass spectrum as a series of data points of type T.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Interface
siblings: [TSpectrum, Spectrum, SpectrumEx, ISpecDataPoint]
---

<br/>
## Remarks
The contract for a mass spectrum as a series of data points of type T, which is
SpecDataPoint or SpecDataPointEx. Implemented by TSpectrum&lt;T&gt;, and through it by
Spectrum and SpectrumEx.

#### Implements
IDisposable

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Count        | int      | The number of data points.   |
| DataPoints   | T[]      | The data points.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [Deserialize(byte[] data)]({{ '/methods/TSpectrum.Deserialize.html' | relative_url }})      | void   | Loads the spectrum from a Serialize payload.   |
| [GetMz(double mz, double ppm = 0)]({{ '/methods/TSpectrum.GetMz.html' | relative_url }})      | int   | Finds the data point at an m/z, or the nearest one within a tolerance in parts per million.   |
| [Resize(int sz)]({{ '/methods/TSpectrum.Resize.html' | relative_url }})      | void   | Replaces the data points with a new, empty array of the given size. Existing points are discarded.   |
| [Serialize()]({{ '/methods/TSpectrum.Serialize.html' | relative_url }})      | byte[]   | Serializes the spectrum to a byte array, suitable as a pipe message payload. Read back by Deserialize.   |

* * *
## Example

```csharp
using Nova.Data;

// Nearest point within 10 ppm of the target, or -1.
int index = spectrum.GetMz(445.1200, 10);
if (index >= 0)
{
  Console.WriteLine(spectrum.DataPoints[index].Intensity);
}
```

Code written against the interface handles both spectrum types.

```csharp
static double TotalIntensity<T>(ISpectrum<T> spectrum) where T : struct, ISpecDataPoint
{
  double total = 0;
  for (int i = 0; i < spectrum.Count; i++)
  {
    total += spectrum.DataPoints[i].Intensity;
  }
  return total;
}
```
