---
name: FileReader
title: FileReader
description: Reads spectra and chromatograms from an MS data file, choosing the reader by file extension.
date: 2025-04-23 14:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Class
interfaces: []
classes: []
siblings: [SpectrumFileReaderFactory,FileFormat,MSFilter]
---

<br/>
## Remarks
Reads spectra and chromatograms from an MS data file, choosing the reader by file extension.
Enumerating it yields each spectrum in file order.

#### Implements
IEnumerable

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| [FileReader(MSFilter filter = MSFilter.MS1 \| MSFilter.MS2 \| MSFilter.MS3)]({{ '/methods/FileReader.Constructor.html' | relative_url }}) | Creates a reader with no file open.  |
| [FileReader(string filename, MSFilter filter = MSFilter.MS1 \| MSFilter.MS2 \| MSFilter.MS3)]({{ '/methods/FileReader.Constructor.html' | relative_url }}) | Creates a reader and opens the file. Throws FormatException if the file extension is not a recognized format, and FileNotFoundException if the file could not be opened.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ChromatCount  | int   | Number of chromatograms in the open file, or 0 for formats that carry none.   |
| FileName  | string   | The name of the file currently being read.   |
| FirstScan  | int   | The scan number of the first spectrum in the file.   |
| Format   | FileFormat   | Identifies the format of the most recently opened file.   |
| LastScan   | int   | The scan number of the last spectrum in the file.   |
| MaxRetentionTime    | double   | The retention time at the end of the run, in minutes.   |
| ScanCount   | int   | The number of spectra in the open file.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [CheckFile(string fileName)]({{ '/methods/FileReader.CheckFile.html' | relative_url }})     | bool   | Checks to see if we requested, or already have, a valid file from which to read a spectrum. Pass an empty string to keep reading the current file. Returns true if the file is already open. Throws ArgumentNullException if no file name was given and no file is open, and FileNotFoundException if the file is not found.  |
| static [CheckFileFormat(string fileName)]({{ '/methods/FileReader.CheckFileFormat.html' | relative_url }})     | FileFormat   | Reads a file name string and returns the FileFormat value based on the file extension characters. FormatException thrown if file doesn't have an extension or the extension isn't recognized.  |
| [GetEnumerator()]({{ '/methods/FileReader.GetEnumerator.html' | relative_url }})     | IEnumerator   | Yields each spectrum in the open file in file order, subject to the current filter, then resets the reader.   |
| [OpenSpectrumFile(string fileName)]({{ '/methods/FileReader.OpenSpectrumFile.html' | relative_url }})      | bool   | Opens a file for reading, closing any file already open. The reader is chosen by file extension. Throws FormatException if the file extension is not a recognized format.   |
| [ReadChromatogram(string fileName = "", int chromatIndex = -1)]({{ '/methods/FileReader.ReadChromatogram.html' | relative_url }})      | Chromatogram   | Reads a chromatogram, opening the file first if it is not the current one. Throws SpectrumFileOpenException if a new file could not be opened.   |
| [ReadSpectrum(string fileName = "", int scanNumber = -1, bool centroid = true)]({{ '/methods/FileReader.ReadSpectrum.html' | relative_url }})      | Spectrum   | Reads a spectrum, opening the file first if it is not the current one. Throws SpectrumFileOpenException if a new file could not be opened.   |
| [ReadSpectrumEx(string fileName = "", int scanNumber = -1, bool centroid = true)]({{ '/methods/FileReader.ReadSpectrumEx.html' | relative_url }})      | SpectrumEx   | Reads a spectrum with the extended per-peak data where the format provides it, opening the file first if it is not the current one. Throws SpectrumFileOpenException if a new file could not be opened.   |
| [Reset()]({{ '/methods/FileReader.Reset.html' | relative_url }})      | void   | Returns to the start of the open file for sequential reading.   |
| [SetFilter(MSFilter filter)]({{ '/methods/FileReader.SetFilter.html' | relative_url }})      | void   | Sets the MS levels to read, for this reader and any file it has open.   |

* * *
## Example

```csharp
using Nova.Io.Read;
using Nova.Data;

// Reads all MS1 scans from a Thermo Fisher Scientific data file
FileReader reader = new FileReader("DDA.raw", MSFilter.MS1);
foreach (Spectrum spec in reader)
{
  // Report each scan number and number of data points
  Console.WriteLine(spec.ScanNumber + " has " + spec.Count + " data points.");
}
```
