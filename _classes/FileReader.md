---
name: FileReader
title: FileReader
description: Class capable of reading spectra from multiple formats.
date: 2025-04-23 14:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Class
interfaces: []
classes: []
siblings: [SpectrumFileReaderFactory,FileFormat]
---

<br/>
## Remarks
A versatile class for reading multiple formats of mass spectrometry data. Internally, FileReader implements
the SpectrumFileReaderFactory to support all Nova compatible spectra files. Additional functionality has been
added beyond the individual file reader classes in Nova.

#### Implements
IEnumerable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| FileReader(MSFilter filter = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3) | Initializes the FileReader class and defaults to parsing all MS, MS/MS, and MS3 scans.  |
| FileReader(string filename,MSFilter filter = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3) | Initializes the FileReader class and opens the requeested file, defaults to parsing all MS, MS/MS, and MS3 scans.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| FileName  | string   | The names of the file currently being read, or empty if no file has been opened.      |
| FirstScanNumber  | int   | Number of the first scan event in the file.      |
| LastScanNumber   | int   | Number of the last scan event in the file.   |
| MaxRetentionTime    | double   | Retention time (in minutes) of the last scan event in the file.   |
| ScanCount   | int   | Total number of scan events in the file.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| CheckFileFormat(string fileName)     | FileFormat   | Reads a file name string and returns the FileFormat value based on the file extension characters. FormatException thrown if file doesn't have an exception or the extension isn't recognized.  |
| Close()     | void   | Closes an open MS data file.  |
| OpenSpectrumFile(string fileName)      | bool   |Opens an mzML file and parses the index and meta information.         |
| ReadSpectrum(string filename="", int scanNumber = -1, bool centroid = true)      | Spectrum   |Opens and/or reads the requested spectrum or the next spectrum if a valid scanNumber is not given. Data are returned in centroid, if possible, unless otherwise requested. Providing an empty string for the file name reads from the previously opened data file, or throws an exception if a file has not been previously opened.   |
| ReadSpectrumEx(string filename="", int scanNumber = -1, bool centroid = true)      | SpectrumEx   |Opens and/or reads the requested spectrum or the next spectrum if a valid scanNumber is not given, and returns the data in the extended spectrum format. Data are returned in centroid, if possible, unless otherwise requested. Providing an empty string for the file name reads from the previously opened data file, or throws an exception if a file has not been previously opened.     |
| Reset()      | void   |Resets the reader to the beginning of the file if sequentially reading the spectra.    |
| SetFilter(MSFilter filter)      | void   |Sets the spectrum type filter to the values requested.    |


* * *
## Example

```csharp
using Nova.Io.Read;
using Nova.Data;

//Reads all MS1 scans from a Thermo Fisher Scientific data file
FileReader reader = new FileReader("DDA.raw",MSFilter.MS1);
foreach(Spectrum spec in reader)
{
  //Report each scan number and number of data points
  Console.WriteLine(spec.ScanNumber + " has " + spec.Count + " data points.");
}
```
