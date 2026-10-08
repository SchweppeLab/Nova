---
name: FileReader.GetEnumerator
title: FileReader.GetEnumerator
member: GetEnumerator
description: Yields each spectrum in the open file in file order.
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
This is what foreach uses. The reader is reset before and after, so each foreach starts at
the first spectrum that passes the filter.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| GetEnumerator() | Yields each spectrum in the open file in file order, subject to the current filter, then resets the reader. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| IEnumerator | An enumerator over Spectrum objects. |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  Console.WriteLine(spectrum.ScanNumber + "\t" + spectrum.RetentionTime);
}
```
