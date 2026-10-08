---
name: FileReader.SetFilter
title: FileReader.SetFilter
member: SetFilter
description: Sets the MS levels to read.
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
The new filter applies from the next read, to the file already open and to any file
opened later.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SetFilter(MSFilter filter) | Sets the MS levels to read, for this reader and any file it has open. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| filter | MSFilter | The MS levels to read. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS1);
int ms1 = 0;
foreach (Spectrum spectrum in reader) ms1++;

reader.SetFilter(MSFilter.MS2 | MSFilter.MS3);
int msn = 0;
foreach (Spectrum spectrum in reader) msn++;
```
