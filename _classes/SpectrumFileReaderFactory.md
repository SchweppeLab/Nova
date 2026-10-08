---
name: SpectrumFileReaderFactory
title: SpectrumFileReaderFactory
description: Creates an opened ISpectrumFileReader for an MS data file.
date: 2025-04-23 14:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Class
interfaces: []
classes: []
siblings: [ISpectrumFileReader,FileReader]
---

<br/>
## Remarks
Creates an opened ISpectrumFileReader for an MS data file.

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| static [GetReader(string file, MSFilter filter)]({{ '/methods/SpectrumFileReaderFactory.GetReader.html' | relative_url }})     | ISpectrumFileReader   | Creates a reader for the file, chosen by its extension, and opens it. Throws ArgumentException if the file extension is not a recognized format, and SpectrumFileOpenException if the file could not be opened or indexed.  |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
Console.WriteLine(reader.ScanCount + " spectra, scans " + reader.FirstScan + " to " + reader.LastScan);

Spectrum spectrum = reader.GetSpectrum();   // the first MS2 spectrum
Console.WriteLine(spectrum.Count + " peaks");

reader.Close();
```
