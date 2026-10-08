---
name: SpectrumFileOpenException.Constructor
title: SpectrumFileOpenException.Constructor
member: Constructor
description: Creates the exception for a file that could not be opened.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
class: SpectrumFileOpenException
type: Method
siblings: [SpectrumFileOpenException, FileReader, SpectrumFileReaderFactory]
---

<br/>
## Remarks
Nova's readers throw this exception themselves; code using them catches it rather than
constructing one. The message reads "Failed to open '&lt;fileName&gt;': &lt;detail&gt;".

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpectrumFileOpenException(string fileName, string detail) | Creates the exception for a file that could not be opened. |
| SpectrumFileOpenException(string fileName, string detail, Exception innerException) | Creates the exception for a file that could not be opened, wrapping the exception that caused it. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| fileName | string | The file that could not be opened. |
| detail | string | Why, as reported by the reader. |
| innerException | Exception | The exception that caused the failure. |

* * *
## Example

```csharp
using Nova.Io.Read;

try
{
  FileReader reader = new FileReader();
  reader.ReadSpectrum("Damaged.mzML");
}
catch (SpectrumFileOpenException ex)
{
  Console.WriteLine(ex.FileName + ": " + ex.Message);
}
```
