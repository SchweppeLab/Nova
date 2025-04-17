---
name: SpectrumEx
title: SpectrumEx
description: A collection of extended data points acquired during ion analysis.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: [favicon]
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [TSpectrum]
siblings: [Spectrum,TSpectrum]
---

<br/>
## Remarks
This class is a convenient wrapper around TSpectrum<SpecDataPointEx> to simplify the coding process
and make code more easily readable. It is intended for use to manage mass spectral scan events containing
what are likely Thermo Fisher Scientific's extended centroided peak information (Noise, Charge, etc.).

#### Implements
ISpectrum, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumEx (int count=0) | Initializes the spectrum with a default data point array size of count.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| DataPoints           | SpecDataPointEx[]   | The array of m/z,intensity (and extended data) structures that comprise a spectrum.         |
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
