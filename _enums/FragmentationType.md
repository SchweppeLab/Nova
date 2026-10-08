---
name: FragmentationType
title: FragmentationType
description: How a precursor was fragmented to produce a dependent scan.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Enums
interfaces: []
siblings: [PrecursorIon]
---

<br/>
## Remarks
How a precursor was fragmented to produce a dependent scan. Carried by
PrecursorIon.FragmentationMethod.

* * *
## Fields

| Name   | Value     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| None    | 0   | Not fragmented, or not recorded.   |
| CID     | 1   | Collision-induced dissociation.   |
| ECD     | 2   | Electron capture dissociation.   |
| ETD     | 3   | Electron transfer dissociation.   |
| EThcD   | 4   | Electron transfer dissociation with supplemental higher-energy collisional dissociation.   |
| ETDSA   | 5   | Electron transfer dissociation with supplemental activation.   |
| HCD     | 6   | Higher-energy collisional dissociation.   |
| IRMPD   | 7   | Infrared multiphoton dissociation.   |
| PQD     | 8   | Pulsed Q dissociation.   |
| SID     | 9   | Surface-induced dissociation.   |

* * *
## Example

```csharp
using Nova.Data;

if (precursor.FragmentationMethod == FragmentationType.HCD)
{
  Console.WriteLine("HCD at " + precursor.CollisionEnergy);
}
```
