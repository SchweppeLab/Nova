---
name: FileReader.Constructor
title: FileReader.Constructor
member: Constructor
description: Creates a FileReader, with or without a file open.
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
The filter sets the MS levels to read. By default it reads MS1, MS2 and MS3 spectra.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| FileReader(MSFilter filter = MSFilter.MS1 \| MSFilter.MS2 \| MSFilter.MS3) | Creates a reader with no file open. |
| FileReader(string filename, MSFilter filter = MSFilter.MS1 \| MSFilter.MS2 \| MSFilter.MS3) | Creates a reader and opens the file. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| filename | string | The file to open. |
| filter | MSFilter | The MS levels to read. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| FormatException | The file extension is not a recognized format. |
| FileNotFoundException | The file could not be opened. |

* * *
## Example

```csharp
using Nova.Io.Read;

// Open a file now, reading only MS2 spectra.
FileReader reader = new FileReader("DDA.mzML", MSFilter.MS2);

// Or create the reader first and name the file when reading.
FileReader later = new FileReader();
later.ReadSpectrum("DDA.mzML", 1000);
```
