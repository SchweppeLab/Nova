---
name: TSpectrum.Serialize
title: TSpectrum&lt;T>.Serialize
member: Serialize
description: Writes spectrum object data members into a byte array.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: TSpectrum
type: Method
siblings: [TSpectrum, Spectrum, SpectrumEx]
---

<br/>
## Remarks
Not every member is carried. The array holds ScanNumber, MsLevel, Centroid, RetentionTime, StartMz, EndMz, TotalIonCurrent, BasePeakIntensity, FaimsState, FaimsCV, Analyzer, IonInjectionTime, ScanType, PrecursorMasterScanNumber, MasterIndex, ScanFilter, ScanDescription, each precursor's IsolationMz, IsolationWidth, MonoisotopicMz and Charge, and the data points.

Read it back with [Deserialize]({{ '/methods/TSpectrum.Deserialize.html' | relative_url }}).

Inherited by [Spectrum]({{ '/classes/Spectrum.html' | relative_url }}) and [SpectrumEx]({{ '/classes/SpectrumEx.html' | relative_url }}).

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Serialize() | Writes spectrum object data members into a byte array. Not every member is carried.  |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| byte[] | The serialized spectrum.  |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
Spectrum spectrum = reader.ReadSpectrum();

byte[] payload = spectrum.Serialize();      // store it, or send it in a PipeMessage
```
