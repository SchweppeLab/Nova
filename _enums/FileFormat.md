---
name: FileFormat
title: FileFormat
description: The file formats FileReader can open.
date: 2025-04-22 13:15:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Enums
interfaces: []
siblings: [FileReader]
---

<br/>
## Remarks
The file formats FileReader can open.

* * *
## Fields

| Name   | Value     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Unknown     | 0   | Not yet determined.   |
| MGF         | 1   | Mascot generic format.   |
| MzML        | 2   | mzML.   |
| MzXML       | 3   | mzXML.   |
| ThermoRaw   | 4   | Thermo RAW.   |

* * *
## Example

```csharp
using Nova.Io.Read;

FileFormat format = FileReader.CheckFileFormat("DDA.mzML");
if (format == FileFormat.MzML)
{
  Console.WriteLine("An mzML file.");
}
```
