---
name: ISpectrumFileReader.Reset
title: ISpectrumFileReader.Reset
member: Reset
description: Resets the file reader to the beginning of the file.
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
After Reset, GetSpectrum with no scan number starts again from the first spectrum that
passes the filter.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Reset() | Resets the file reader to the beginning of the file when iteratively reading. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
Spectrum first = reader.GetSpectrum();

reader.Reset();
Spectrum again = reader.GetSpectrum();   // the same scan as first
reader.Close();
```
