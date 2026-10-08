---
name: SpecDataPointEx
title: SpecDataPointEx
description: An extended mass spectrum data point, with per-peak attributes from a Thermo centroid stream.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Struct
interfaces: [ISpecDataPoint]
siblings: [SpecDataPoint]
---

<br/>
## Remarks
An extended mass spectrum data point: m/z and intensity plus the per-peak attributes a Thermo
centroid stream provides. This is the point type within SpectrumEx. Only the Thermo RAW
reader fills the extra fields; the mzML, mzXML and MGF readers set m/z and intensity and
leave the rest at zero.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpecDataPointEx(double mz = 0, double intensity = 0, double noise = 0, double baseline = 0, int charge = 0, double resolution = 0) | Creates a data point, with every value defaulting to zero.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Mz           | double   | The mass to charge ratio (m/z) value for this data point.   |
| Intensity    | double   | The intensity value for this data point.   |
| Noise        | double   | The noise level the instrument reported at this peak.   |
| Baseline     | double   | The baseline level the instrument reported at this peak.   |
| Charge       | int      | The charge state the instrument assigned to this peak, or zero if none was assigned.   |
| Resolution   | double   | The mass resolution the instrument measured at this peak.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| CompareTo(SpecDataPointEx x) | int    | Orders data points by m/z alone. Two points at the same m/z compare as equal.   |
| Read(BinaryReader reader)      | void   | Reads an extended mass spectrum data point.   |
| Write(BinaryWriter writer)     | void   | Writes an extended mass spectrum data point.   |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
SpectrumEx spectrum = reader.ReadSpectrumEx("", 1000);

for (int i = 0; i < spectrum.Count; i++)
{
  SpecDataPointEx peak = spectrum.DataPoints[i];
  Console.WriteLine(peak.Mz + "\t" + peak.Intensity + "\tz=" + peak.Charge
    + "\tS/N=" + peak.Intensity / peak.Noise);
}
```
