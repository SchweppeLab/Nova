---
name: FileReader.OpenSpectrumFile
title: FileReader.OpenSpectrumFile
member: OpenSpectrumFile
description: Opens a file for reading, closing any file already open.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
class: FileReader
type: Method
siblings: [FileReader, FileFormat, MSFilter]
---

<br/>
## Remarks
Opening a file sets Format, FileName, ScanCount, ChromatCount, FirstScan, LastScan and
MaxRetentionTime.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| OpenSpectrumFile(string fileName) | Opens a file for reading, closing any file already open. The reader is chosen by file extension. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | The file to open. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| bool | true if the file opened and is ready to read. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| FormatException | The file extension is not a recognized format. |

* * *
## Example

```csharp
using Nova.Io.Read;

FileReader reader = new FileReader();
if (reader.OpenSpectrumFile("DDA.mzML"))
{
  Console.WriteLine(reader.ScanCount + " spectra, scans " + reader.FirstScan + " to " + reader.LastScan);
}
```
