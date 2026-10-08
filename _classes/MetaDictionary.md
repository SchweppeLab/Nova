---
name: MetaDictionary
title: MetaDictionary
description: Maps a Thermo trailer label to the MetaClass it represents.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Io.Meta
type: Class
interfaces: []
classes: []
siblings: [MetaClass]
---

<br/>
## Remarks
Maps a Thermo trailer label to the MetaClass it represents.

Trailer labels vary between instruments and file versions, so several spellings map
to the same value. "Charge State", "Charge State:", "Charge" and "Z" all resolve to
MetaClass.ChargeState.

This is a static class. There is nothing to construct.

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| static FindMeta(string label)     | MetaClass   | Looks up a trailer label. Returns the matching MetaClass, or MetaClass.None if the label is not recognized.  |

* * *
## Example

```csharp
using Nova.Io.Meta;

// Several spellings map to the same MetaClass, because the label
// varies between instruments and file versions.
MetaClass a = MetaDictionary.FindMeta("Charge State");
MetaClass b = MetaDictionary.FindMeta("Z");

// An unrecognized label returns None rather than throwing.
MetaClass unknown = MetaDictionary.FindMeta("Not A Real Label");
```

```csharp
switch (MetaDictionary.FindMeta(label))
{
  case MetaClass.ChargeState:
    Console.WriteLine("charge " + value);
    break;

  case MetaClass.FaimsCV:
    Console.WriteLine("FAIMS CV " + value);
    break;

  case MetaClass.None:
    // Nothing recognized this label, so leave it alone.
    break;
}
```
