---
name: ISpectrumFileReader
title: ISpectrumFileReader
description: A reader for one MS data file, obtained from SpectrumFileReaderFactory.GetReader.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Io.Read
type: Interface
siblings: [SpectrumFileReaderFactory,MSFilter,Spectrum,SpectrumEx]
---

<br/>
## Remarks
A reader for one MS data file, obtained from SpectrumFileReaderFactory.GetReader.
Enumerating it yields each spectrum in file order.

#### Implements
IEnumerable

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ChromatCount  | int   | Number of chromatograms retrievable from the open file via GetChromatogram, or 0 for formats that carry none.   |
| Filter   | MSFilter   | The MS levels to read. Spectra at other levels are skipped.   |
| FirstScan  | int   | The scan number of the first spectrum in the file.   |
| LastScan   | int   | The scan number of the last spectrum in the file.   |
| MaxRetentionTime    | double   | The retention time at the end of the run, in minutes.   |
| ScanCount   | int   | The number of spectra in the open file.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Close()     | void   | Performs any cleanup.   |
| GetChromatogram(int chromatIndex = -1)     | Chromatogram   | Reads a chromatogram from the currently open MS data file.   |
| GetSpectrum(int scanNumber = -1, bool centroid = true)      | Spectrum   | Reads a spectrum from the currently open MS data file.   |
| GetSpectrumEx(int scanNumber = -1, bool centroid = true)     | SpectrumEx   | Reads a spectrum from the currently open MS data file, with the extended per-peak data where the format provides it.   |
| Open(string fileName)     | bool   | Open data file and hold new scan information.   |
| Reset()     | void   | Resets the file reader to the beginning of the file when iteratively reading.   |
