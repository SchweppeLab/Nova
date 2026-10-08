---
name: FileReader.CheckFile
title: FileReader.CheckFile
member: CheckFile
description: Checks whether a file needs to be opened before reading.
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
ReadSpectrum, ReadSpectrumEx and ReadChromatogram call this before reading. It returns
false when the file exists but is not the one open, meaning it must be opened first.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| CheckFile(string fileName) | Checks to see if we requested, or already have, a valid file from which to read a spectrum. Pass an empty string to keep reading the current file. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | The file to read from, or empty to keep reading the current file. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| bool | true if the file is already open, false if not open. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| ArgumentNullException | No file name was given and no file is open. |
| FileNotFoundException | File not found. |

* * *
## Example

```csharp
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.mzML");

bool open = reader.CheckFile("");            // true: keep reading DDA.mzML
bool other = reader.CheckFile("DIA.mzML");   // false: DIA.mzML exists but is not open
```
