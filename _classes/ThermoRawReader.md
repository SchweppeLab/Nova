---
name: ThermoRawReader
title: ThermoRawReader
description: Class capable of reading spectra from Thermo Fisher Scientific raw formatted data files.
date: 2025-04-23 14:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Class
interfaces: [ISpectrumFileReader]
classes: []
siblings: [ISpectrumFileReader,Spectrum,SpectrumEx]
---

<br/>
## Remarks
Reads mass spectra stored in the proprietary Thermo Fisher Scientific raw format. Requires linking
Nova to the [MSFileReader](https://github.com/thermofisherlsms/MSFileReader).

#### Implements
ISpectrumFileReader

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| ThermoRawReader(MSFilter filter) | Initializes the ThermoRawReader class and sets a spectrum filter.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| FirstScanNumber  | int   | Number of the first scan event in the file.      |
| LastScanNumber   | int   | Number of the last scan event in the file.   |
| MaxRetentionTime    | double   | Retention time (in minutes) of the last scan event in the file.   |
| ScanCount   | int   | Total number of scan events in the file.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Close()     | void   | Closes an open MS data file.  |
| GetSpectrum(int scanNumber = -1, bool centroid = true)      | Spectrum   |Reads the requested spectrum or the next spectrum if a valid scanNumber is not given. Data are returned in centroid, if possible, unless otherwise requested.    |
| GetSpectrumEx(int scanNumber = -1, bool centroid = true)      | SpectrumEx   |Reads the requested spectrum or the next spectrum if a valid scanNumber is not given, and returns the data in the extended spectrum format. Data are returned in centroid, if possible, unless otherwise requested.    |
| Open(string fileName)      | bool   |Opens an mzML file and parses the index and meta information.         |
| GetSpectrum(int scanNumber = -1, bool centroid = true)      | Spectrum   |Reads the requested spectrum or the next spectrum if a valid scanNumber is not given. Data are returned in centroid, if possible, unless otherwise requested.    |


* * *
## Example

```csharp
```
