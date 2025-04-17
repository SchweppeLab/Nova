---
name: TSpectrum
title: TSpectrum&lt;T>
description: A generic class for creating spectrum objects with any type of data point.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: [favicon]
namespaces: Data
type: Class
interfaces: [ISpectrum]
classes: [SpectrumFoundation]
siblings: [Spectrum,SpectrumEx,SpectrumFoundation]
---

<br/>
## Remarks
The TSpectrum generic class is an implementation of the ISpectrum interface defining
the characteristics for managing mass spectra data structures. &lt;T> defines the type of data
point stored in the class, preferrably an m/z - intensity pair (e.g., SpecDataPoint) or similar
structure.

#### Implements
ISpectrum, IDisposable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| TSpectrum (int count=0) | Initializes the spectrum with a default data point array size of count.  |

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
