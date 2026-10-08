---
name: ISpectrumFileReader.Close
title: ISpectrumFileReader.Close
member: Close
description: Performs any cleanup when finished with a file.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
class: ISpectrumFileReader
type: Method
siblings: [ISpectrumFileReader, SpectrumFileReaderFactory]
---

<br/>
## Remarks
Call Close when finished reading, to release the file.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Close() | Performs any cleanup. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
Spectrum spectrum = reader.GetSpectrum();
reader.Close();
```
