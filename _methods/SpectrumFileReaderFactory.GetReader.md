---
name: SpectrumFileReaderFactory.GetReader
title: SpectrumFileReaderFactory.GetReader
member: GetReader
description: Creates a reader for an MS data file, chosen by its extension, and opens it.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
class: SpectrumFileReaderFactory
type: Method
siblings: [SpectrumFileReaderFactory, ISpectrumFileReader, FileReader]
---

<br/>
## Remarks
A static method. The reader returned is already open; call its Close method when finished.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| static GetReader(string file, MSFilter filter) | Creates a reader for the file, chosen by its extension, and opens it. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| file | string | The file to open. |
| filter | MSFilter | The MS levels to read. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| ISpectrumFileReader | A reader with the file open. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| ArgumentException | The file extension is not a recognized format. |
| SpectrumFileOpenException | The file could not be opened or indexed. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

try
{
  ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
  Spectrum spectrum = reader.GetSpectrum();
  reader.Close();
}
catch (SpectrumFileOpenException ex)
{
  Console.WriteLine(ex.Message);
}
```
