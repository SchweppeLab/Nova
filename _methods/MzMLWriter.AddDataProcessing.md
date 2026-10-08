---
name: MzMLWriter.AddDataProcessing
title: MzMLWriter.AddDataProcessing
member: AddDataProcessing
description: Starts a data processing entry.
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
Later AddProcessingMethod calls attach to the entry most recently started.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| AddDataProcessing(string id) | Starts a data processing entry; later AddProcessingMethod calls attach to it. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| id | string | The entry's id. |

* * *
## Example

```csharp
using Nova.Io.Write;

MzMLWriter writer = new MzMLWriter();
writer.AddSoftware("Nova", "1.1.0");

writer.AddDataProcessing("DP1");
writer.AddProcessingMethod("Nova");
```
