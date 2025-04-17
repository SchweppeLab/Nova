---
name: Spectrum
title: Spectrum
description: A collection of data points acquired during ion analysis.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: [favicon]
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [TSpectrum]
siblings: [SpectrumEx,TSpectrum]
---

<br/>
## Remarks
This class is a convenient wrapper around TSpectrum<SpecDataPoint> to simplify the coding process
and make code more easily readable. It is intended for use to manage most mass spectral scan events.

#### Implements
ISpectrum, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Spectrum (int count=0) | Initializes the spectrum with a default data point array size of count.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| DataPoints           | SpecDataPoint[]   | The array of m/z and intensity pairs that comprise a spectrum.         |
| Count    | int   | The number of data points in the DataPoints array   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Deserialize (byte[] data)      | void   |Converts a byte array to the contents of a Spectrum.         |
| Dispose ()      | void   |Disposes of the Spectrum.         |
| GetMz (double mz, double ppm = 0)    | int   | Returns the index of the DataPoints array whose m/z value falls within the ppm tolerance of the requested mz value.  |
| Resize (int sz)| int    | Resizes, and reinitializes to zero, the DataPoints array of the Spectrum.   |
| Serialize ()  | byte[]    | Packages the Spectrum into a byte array for storage or transmission.   |

* * *
## Example

```csharp
// Example C# code
using Nova.Io.Read;
using Nova.Data;

int MS1Count=0;
int MS2Count=0;
FileReader reader = new FileReader("TheBestDataEver.mzML",MSFilter.MS1|MSFilter.MS2);
foreach(Spectrum spec in reader)
{
  if(spec.MsLevel==1)
  {
    MS1Count++;
  }
  else
  {
    MS2Count++;
  }
}
Console.WriteLine("There are " + MS1Count + " MS scans and " + MS2Count + " MS/MS scans.");
```
