---
name: PrecursorIon.Constructor
title: PrecursorIon.Constructor
member: Constructor
description: Creates a precursor, empty, copied from another, or from its m/z.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: PrecursorIon
type: Method
siblings: [PrecursorIon, FragmentationType, SpectrumFoundation]
---

<br/>
## Remarks
The copy constructor copies every field, including CollisionEnergy, FragmentationMethod
and IsolationSpecificity. The m/z constructor leaves those three at their defaults.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PrecursorIon() | PrecursorIon object constructor. |
| PrecursorIon(PrecursorIon pi) | PrecursorIon object copy constructor. |
| PrecursorIon(double mz, double intensity = 0, int charge = 0, double isoMz = 0, double isoWidth = 0) | Creates a precursor from its monoisotopic m/z, with optional intensity, charge, isolation m/z, and isolation width. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| pi | PrecursorIon | The precursor to copy. |
| mz | double | The monoisotopic m/z. |
| intensity | double | The intensity of the precursor peak. |
| charge | int | The charge state. |
| isoMz | double | The isolation m/z of the precursor selection window. |
| isoWidth | double | The isolation width of the precursor selection window. |

* * *
## Example

```csharp
using Nova.Data;

PrecursorIon precursor = new PrecursorIon(652.3412, 1.2e6, 2, 652.84, 1.6);
precursor.FragmentationMethod = FragmentationType.HCD;

// The copy carries every field.
PrecursorIon copy = new PrecursorIon(precursor);
```
