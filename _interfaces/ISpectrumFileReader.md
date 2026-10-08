---
name: ISpectrumFileReader
title: ISpectrumFileReader
description: Interface for MS data file readers.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Io.Read
type: Interface
siblings: [MSFilter,Spectrum,SpectrumEx]
---

<br/>
## Remarks
none

#### Implements
IEnumerable

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ChromatCount  | int   | Number of chromatograms retrievable from the open file, or 0 for formats that carry none.      |
| FirstScan  | int   | Number of the first scan event in the file.      |
| LastScan   | int   | Number of the last scan event in the file.   |
| MaxRetentionTime    | double   | Retention time (in minutes) of the last scan event in the file.   |
| ScanCount   | int   | Total number of scan events in the file.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Close()     | void   | Closes an open MS data file.  |
| GetSpectrum(int scanNumber = -1, bool centroid = true)      | Spectrum   | Reads a spectrum from the currently open MS data file.         |
| GetSpectrumEx(int scanNumber = -1, bool centroid = true)     | SpectrumEx   | Reads a spectrum and any extended data from the currently open MS data file.  |
| Open(string fileName)     | bool   | Open an MS data file and return true upon success.  |
| Reset()     | void   |  Resets the file reader to the beginning of the file when iteratively reading.  |

* * *
## Example
