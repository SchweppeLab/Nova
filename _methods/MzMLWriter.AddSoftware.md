---
name: MzMLWriter.AddSoftware
title: MzMLWriter.AddSoftware
member: AddSoftware
description: Adds a software entry.
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
The id is how processing methods refer to the software; see AddProcessingMethod.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| AddSoftware(string id, string version) | Adds a software entry. The ids "Xcalibur" and "pwiz" also get their standard CV terms. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| id | string | The software id. |
| version | string | The software version. |

* * *
## Example

```csharp
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
writer.AddSoftware("Xcalibur", "4.5");   // also gets its standard CV term
writer.AddSoftware("Nova", "1.1.0");
```
