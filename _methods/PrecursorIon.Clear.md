---
name: PrecursorIon.Clear
title: PrecursorIon.Clear
member: Clear
description: Resets all variables to 0.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: PrecursorIon
type: Method
siblings: [PrecursorIon, FragmentationType]
---

<br/>
## Remarks
FragmentationMethod is reset to FragmentationType.None.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Clear() | Resets all variables to 0. |

* * *
## Example

```csharp
using Nova.Data;

PrecursorIon precursor = new PrecursorIon(652.3412, 1.2e6, 2);
precursor.Clear();
Console.WriteLine(precursor.MonoisotopicMz);   // 0
```
