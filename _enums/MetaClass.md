---
name: MetaClass
title: MetaClass
description: The kinds of scan metadata recognized in a Thermo trailer, by label.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Io.Meta
type: Enums
interfaces: []
siblings: [MetaDictionary]
---

<br/>
## Remarks
The kinds of scan metadata recognized in a Thermo trailer, by label. Use
MetaDictionary.FindMeta to turn a label from a file into one of these values.

* * *
## Fields

| Name   | Value     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| None               | 0    | Not a recognized label.   |
| Analyzer           | 1    | Mass analyzer.   |
| ChargeState        | 2    | Precursor charge state.   |
| FaimsState         | 3    | Whether FAIMS was on.   |
| FaimsCV            | 4    | FAIMS compensation voltage.   |
| IIT                | 5    | Ion injection time.   |
| MasterIndex        | 6    | The Thermo master index.   |
| MasterScanNumber   | 7    | Scan number of the master (parent) scan.   |
| MonoisotopicMZ     | 8    | Precursor monoisotopic m/z.   |
| ScanDescription    | 9    | Scan description.   |
| ScanNumber         | 10   | Scan number.   |
| TIC                | 11   | Total ion current.   |

* * *
## Example

```csharp
using Nova.Io.Meta;

MetaClass kind = MetaDictionary.FindMeta("FAIMS CV");

if (kind == MetaClass.FaimsCV)
{
  // The label was recognized as the FAIMS compensation voltage.
}
```
