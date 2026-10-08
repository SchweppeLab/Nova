---
name: MetaDictionary.FindMeta
title: MetaDictionary.FindMeta
member: FindMeta
description: Looks up a Thermo trailer label.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Io.Meta
class: MetaDictionary
type: Method
siblings: [MetaDictionary, MetaClass]
---

<br/>
## Remarks
A static method. The label must match one of the known spellings exactly, including case.
Several spellings map to the same MetaClass, because labels vary between instruments and
file versions.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| static FindMeta(string label) | Looks up a trailer label. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| label | string | The label as it appears in the file. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| MetaClass | The matching MetaClass, or MetaClass.None if the label is not recognized. |

* * *
## Example

```csharp
using Nova.Io.Meta;

MetaClass a = MetaDictionary.FindMeta("Charge State");       // MetaClass.ChargeState
MetaClass b = MetaDictionary.FindMeta("Z");                  // MetaClass.ChargeState
MetaClass c = MetaDictionary.FindMeta("Not A Real Label");   // MetaClass.None
```
