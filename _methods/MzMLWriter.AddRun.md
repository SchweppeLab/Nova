---
name: MzMLWriter.AddRun
title: MzMLWriter.AddRun
member: AddRun
description: Starts a run; spectra added afterward belong to it.
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
Call AddRun before the first AddSpectrum. Without a run, AddSpectrum throws a
NullReferenceException.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| AddRun(string id, string instConf) | Starts a run; spectra added afterward belong to it. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| id | string | The run id. |
| instConf | string | The id of the instrument configuration the run used. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
writer.AddInstrumentConfiguration("IC1", null);
writer.AddRun("run1", "IC1");

FileReader reader = new FileReader("DDA.raw");
foreach (Spectrum spectrum in reader)
{
  writer.AddSpectrum(spectrum);
}
```
