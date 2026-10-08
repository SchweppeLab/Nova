---
name: MzMLWriter.Write
title: MzMLWriter.Write
member: Write
description: Writes the accumulated mzML content to a file.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Write
class: MzMLWriter
type: Method
siblings: [MzMLWriter]
---

<br/>
## Remarks
Validation is off by default, since it requires a local copy of the schema file and
validation is not needed to produce a usable file.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Write(string filename, bool validateSchema = false, string? schemaPath = null) | Writes the accumulated mzML content to filename. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| filename | string | Output file path. |
| validateSchema | bool | If true, re-opens the written file and validates it against the mzML XSD at schemaPath. |
| schemaPath | string? | Path to a local copy of the mzML 1.1.0 XSD. Required if validateSchema is true. |

#### Exceptions

| Exception   | Condition                                               |
|:-------------|:----------------------------------------------------------|
| ArgumentException | validateSchema is true but schemaPath is empty. |

* * *
## Example

```csharp
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
// ... add software, instrument configuration, a run and spectra ...

writer.Write("converted.mzML");

// Or write and validate against a local copy of the schema.
writer.Write("converted.mzML", true, @"C:\schemas\mzML1.1.0.xsd");
```
