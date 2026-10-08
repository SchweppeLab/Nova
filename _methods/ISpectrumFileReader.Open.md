---
name: ISpectrumFileReader.Open
title: ISpectrumFileReader.Open
member: Open
description: Opens an MS data file and reads its scan information.
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
A reader from SpectrumFileReaderFactory.GetReader is already open. Open switches it to
another file of the same format.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Open(string fileName) | Open data file and hold new scan information. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | Path to the data file. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| bool | true if the file opened and is ready to read. |

* * *
## Example

```csharp
using Nova.Io.Read;

ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("Run1.mzML", MSFilter.MS2);
Console.WriteLine("Run1: " + reader.ScanCount + " spectra");

if (reader.Open("Run2.mzML"))
{
  Console.WriteLine("Run2: " + reader.ScanCount + " spectra");
}
reader.Close();
```
