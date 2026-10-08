---
name: TSpectrum
title: TSpectrum&lt;T>
description: The spectrum implementation shared by Spectrum and SpectrumEx.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [SpectrumFoundation]
siblings: [Spectrum,SpectrumEx,SpectrumFoundation]
---

<br/>
## Remarks
The spectrum implementation shared by Spectrum and SpectrumEx: the scan-level fields of
SpectrumFoundation plus the data points. T is the data point type, SpecDataPoint or
SpecDataPointEx.

#### Implements
ISpectrum&lt;T&gt;, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| TSpectrum(int count = 0) | Creates a spectrum sized for the given number of data points.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Count        | int      | The number of data points. Stored rather than computed from DataPoints.Length, for speed.   |
| DataPoints   | T[]      | The data points. Assigning a new array here directly does not update Count; use Resize.   |

The scan-level properties are inherited from [SpectrumFoundation]({{ '/classes/SpectrumFoundation.html' | relative_url }}).

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Deserialize(byte[] data)      | void   | Reads a byte array into spectrum object data members.   |
| Dispose()      | void   | A no-op beyond marking the instance disposed; a spectrum holds no unmanaged resources.   |
| GetMz(double mz, double ppm = 0)    | int   | Finds the data point at an m/z, or the nearest one within a tolerance. Requires DataPoints sorted by ascending m/z, the order the point types' CompareTo defines. If both neighbors of a miss are within tolerance, the lower-m/z one is returned. Returns -1 if the spectrum is empty or no point is within tolerance.   |
| Resize(int sz) | void    | Replaces the data points with a new, empty array of the given size. Existing points are discarded.   |
| Serialize()  | byte[]    | Writes spectrum object data members into a byte array. Not every member is carried.   |

* * *
## Example

```csharp
using Nova.Data;

Spectrum spectrum = new Spectrum();

// Assigning DataPoints directly does not update Count, so size with Resize.
spectrum.Resize(2);
spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);

byte[] payload = spectrum.Serialize();

Spectrum copy = new Spectrum();
copy.Deserialize(payload);
Console.WriteLine(copy.Count);
```
