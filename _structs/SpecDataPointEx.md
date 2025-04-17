---
name: SpecDataPointEx
title: SpecDataPointEx
description: An extended unit of information in a spectrum (i.e., m/z, intensity, and more).
date: 2025-04-15 11:18:14 -0700
layout: post
tags: [favicon]
namespaces: Data
type: Struct
interfaces: [ISpecDataPoint]
siblings: [SpecDataPoint]
menu:
  - item1:
    name: Data
    title: Data Namespace
    collection: namespaces
  - item2:
    name: SpecDataPoint
    title: SpecDataPoint
    collection: ns_data
---

<br/>
## Remarks
Provides a means for storing extended spectral data point values. There are limited sources
of these additional data. Notably, such extended information can be obtained when reading
mass spectra from the Thermo Fisher Scientific native vendor format (raw). The extended information
is only available if the spectra was acquired in a suitable analyzer (e.g., Orbitrap) and has
been processed to centroid format. Use of SpecDataPointEx is not relevant for profile data or
spectra where the extended information is not stored. In those cases, the extended properties
will contain zero values.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpecDataPointEx(double mz = 0, double intensity = 0, double noise=0, double baseline=0, int charge=0, double resolution=0) | Initializes the properties to the values, if provided.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Mz           | double   | Gets/sets the m/z value of a spectral data point.         |
| Intensity    | double   | Gets/sets the intensity value of a spectral data point.   |
| Noise           | double   | Gets/sets the noise value of a spectral data point.         |
| Baseline    | double   | Gets/sets the baseline value of a spectral data point.   |
| Charge           | int   | Gets/sets the charge value of a spectral data point.         |
| Resolution    | double   | Gets/sets the resolution value of a spectral data point.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Read (BinaryReader)      | void   | Reads the six property values from a BinaryReader.         |
| Write (BinaryWriter)     | void   | Writes the six property values to a BinaryWriter.  |
| CompareTo (SpecDataPoint)| int    | Performs the CompareTo function on the Mz of two SpecDataPointsEx to identify the lower value.   |

* * *
## Example
