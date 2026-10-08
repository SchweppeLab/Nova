---
name: SpectrumFileOpenException
title: SpectrumFileOpenException
description: Thrown when an MS data file cannot be opened or indexed.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Class
interfaces: []
classes: []
siblings: [FileReader, SpectrumFileReaderFactory]
---

<br/>
## Remarks
Thrown when an MS data file cannot be opened or indexed. Derives from IOException, so
existing file-error handling still catches it.

Thrown by SpectrumFileReaderFactory.GetReader, and by FileReader.ReadSpectrum,
ReadSpectrumEx and ReadChromatogram when given a new file name.

#### Inherits
IOException

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumFileOpenException(string fileName, string detail) | Creates the exception for a file that could not be opened. detail is why, as reported by the reader.  |
| SpectrumFileOpenException(string fileName, string detail, Exception innerException) | Creates the exception for a file that could not be opened, wrapping the exception that caused it.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| FileName  | string   | The file that could not be opened.      |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader();

try
{
  Spectrum spectrum = reader.ReadSpectrum("missing.mzML");
}
catch (SpectrumFileOpenException ex)
{
  Console.WriteLine(ex.FileName + ": " + ex.Message);
}
```
