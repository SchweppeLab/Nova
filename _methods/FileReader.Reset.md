---
name: FileReader.Reset
title: FileReader.Reset
member: Reset
description: Returns to the start of the open file for sequential reading.
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
After Reset, ReadSpectrum with no scan number starts again from the first spectrum that
passes the filter.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Reset() | Returns to the start of the open file for sequential reading. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw");
Spectrum first = reader.ReadSpectrum();
Spectrum second = reader.ReadSpectrum();

reader.Reset();
Spectrum again = reader.ReadSpectrum();   // the same scan as first
```
