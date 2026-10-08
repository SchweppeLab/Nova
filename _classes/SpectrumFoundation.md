---
name: SpectrumFoundation
title: SpectrumFoundation
description: The scan-level fields of a mass spectrum, independent of its data point type.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: []
classes: []
siblings: [TSpectrum, Spectrum, SpectrumEx, PrecursorIon]
---

<br/>
## Remarks
The scan-level fields of a mass spectrum, independent of its data point type. It is the
base of TSpectrum&lt;T&gt;, so every field here is available on Spectrum and SpectrumEx.

This class is abstract and is not constructed directly.

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Analyzer   | string   | Mass analyzer used.   |
| BasePeakIntensity   | double   | The abundance of the most intense peak in the scan.   |
| BasePeakMz   | double   | The m/z of the most intense peak in the scan.   |
| Centroid   | bool   | Identifies the peak data type as centroid (true) or profile (false).   |
| CollisionEnergy   | double   | The activation energy for a fragmentation event.   |
| Description   | string   | The "Scan Description" field from the scan header.   |
| DetectorType   | string   | Detector or mass analyzer used for the scan, such as ITMS or FTMS.   |
| ElapsedScanTime   | double   | Total time, including injection time, to acquire the scan, in milliseconds.   |
| EndMz   | double   | m/z that the scan ends at.   |
| FaimsCV   | double   | FAIMS compensation voltage, if used, in volts.   |
| FaimsState   | bool   | FAIMS state, true when on. FaimsState can be on while FaimsCV is 0.   |
| HighestMz   | double   | Highest m/z observed in the scan.   |
| IonInjectionTime   | double   | Injection time used to acquire the scan ions, in milliseconds, maximum 5000.   |
| LowestMz   | double   | Lowest m/z observed in the scan.   |
| MasterIndex   | int   | Thermo variable for master scan number.   |
| MetaData   | Dictionary&lt;string, string&gt;   | Key/value dictionary for information without a dedicated field.   |
| MsLevel   | int   | The MS level: 0 unknown, 1 MS1, 2 MS2, 3 MS3, and so on.   |
| Polarity   | bool   | Polarity, true when positive.   |
| PrecursorActivationMethod   | string   | If a dependent scan, the activation method used to generate the scan fragments.   |
| PrecursorMasterScanNumber   | int   | If a dependent scan, the parent scan number.   |
| Precursors   | List&lt;PrecursorIon&gt;   | Zero or more precursor ions associated with this spectrum.   |
| RetentionTime   | double   | Scan retention time, in minutes.   |
| ScanDescription   | string   | The scan description.   |
| ScanEvent   | int   | Scan event order.   |
| ScanFilter   | string   | Scan filter line from the raw file.   |
| ScanNumber   | int   | Scan number.   |
| ScanType   | string   | String description of the scan type.   |
| StartMz   | double   | m/z that the scan starts at.   |
| TotalIonCurrent   | double   | Total ion current for the scan.   |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  Console.WriteLine(spectrum.ScanNumber + "\t" + spectrum.RetentionTime
    + "\tMS" + spectrum.MsLevel + "\t" + spectrum.TotalIonCurrent);
}
```
