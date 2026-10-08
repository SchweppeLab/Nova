---
name: MzMLWriter.AddInstrumentConfiguration
title: MzMLWriter.AddInstrumentConfiguration
member: AddInstrumentConfiguration
description: Adds an instrument configuration entry.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Write
class: MzMLWriter
type: Method
siblings: [MzMLWriter, InstrumentComponents]
---

<br/>
## Remarks
A run refers to a configuration by its id; see AddRun.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| AddInstrumentConfiguration(string id, string? refID) | Adds an instrument configuration entry. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| id | string | The configuration id. |
| refID | string? | Unused. |

* * *
## Example

```csharp
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
writer.AddInstrumentConfiguration("IC1", null);
writer.AddRun("run1", "IC1");
```
