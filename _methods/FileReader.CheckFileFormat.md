---
name: FileReader.CheckFileFormat
title: FileReader.CheckFileFormat
member: CheckFileFormat
description: Returns the FileFormat that matches a file name's extension.
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
A static method, so no reader is needed. Only the extension is examined, without regard to
case: .raw, .mzML, .mzXML and .mgf are recognized. The file does not have to exist.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| static CheckFileFormat(string fileName) | Reads a file name string and returns the FileFormat value based on the file extension characters. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | The file name; only its extension is examined. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| FileFormat | The format matching the extension. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| FormatException | The file doesn't have an extension or the extension isn't recognized. |

* * *
## Example

```csharp
using Nova.Io.Read;

FileFormat format = FileReader.CheckFileFormat("DDA.RAW");   // FileFormat.ThermoRaw
```
