---
name: MzMLWriter
title: MzMLWriter
description: Builds an indexed mzML file from spectra.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Io.Write
type: Class
interfaces: []
classes: []
siblings: [Spectrum, InstrumentComponents, FileReader]
---

<br/>
## Remarks
Builds an indexed mzML file from spectra. Describe the instrument, software and data
processing with the Add methods, start a run with AddRun, add spectra with AddSpectrum,
then call Write.

Order matters. A run must be started before any spectrum is added, and
AddProcessingMethod does nothing unless AddDataProcessing has been called first.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| MzMLWriter() | Creates a writer with the fixed mzML header in place.  |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| AddDataProcessing(string id)     | void   | Starts a data processing entry. Later AddProcessingMethod calls attach to it.  |
| AddFileDescription(string fileName)     | void   | Not yet implemented. Has no effect on the written file.  |
| AddInstrumentConfiguration(string id, string? refID)     | void   | Adds an instrument configuration entry. The refID parameter is unused.  |
| AddProcessingMethod(string softwareRef)     | void   | Adds a processing method to the current data processing entry. Does nothing if AddDataProcessing has not been called.  |
| AddRun(string id, string instConf)     | void   | Starts a run. Spectra added afterward belong to it.  |
| AddSoftware(string id, string version)     | void   | Adds a software entry. The ids "Xcalibur" and "pwiz" also get their standard CV terms.  |
| AddSpectrum(Spectrum spec)     | void   | Converts a spectrum object into a spectrum element.  |
| Write(string filename, bool validateSchema = false, string? schemaPath = null)     | void   | Writes the accumulated mzML content to the given path. With validateSchema true, re-opens the written file and validates it against the mzML XSD at schemaPath. Throws ArgumentException if validateSchema is true and schemaPath is empty.  |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();

// Describe how the file was produced. Ids given here are referred to
// by the entries that follow.
writer.AddSoftware("Nova", "1.1.0");
writer.AddInstrumentConfiguration("IC1", null);
writer.AddDataProcessing("DP1");
writer.AddProcessingMethod("Nova");

// A run must be started before any spectrum is added.
writer.AddRun("run1", "IC1");

FileReader reader = new FileReader("DDA.raw", MSFilter.MS1 | MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  writer.AddSpectrum(spectrum);
}

writer.Write("converted.mzML");
```

Validation is off by default, because it needs a local copy of the schema and the
output is usable either way.

```csharp
writer.Write("converted.mzML", true, @"C:\schemas\mzML1.1.0.xsd");
```
