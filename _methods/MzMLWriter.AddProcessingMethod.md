---
name: MzMLWriter.AddProcessingMethod
title: MzMLWriter.AddProcessingMethod
member: AddProcessingMethod
description: Adds a processing method to the current data processing entry.
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
Call AddDataProcessing first. Without it, AddProcessingMethod does nothing.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| AddProcessingMethod(string softwareRef) | Adds a processing method to the current data processing entry. Does nothing if AddDataProcessing has not been called. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| softwareRef | string | The id of the software that performed it. |

* * *
## Example

```csharp
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
writer.AddSoftware("Xcalibur", "4.5");
writer.AddSoftware("Nova", "1.1.0");

// Methods are numbered in the order they are added.
writer.AddDataProcessing("DP1");
writer.AddProcessingMethod("Xcalibur");
writer.AddProcessingMethod("Nova");
```
