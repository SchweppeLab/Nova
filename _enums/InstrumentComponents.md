---
name: InstrumentComponents
title: InstrumentComponents
description: Instrument components, for describing an instrument configuration.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Io.Write
type: Enums
interfaces: []
siblings: [MzMLWriter]
---

<br/>
## Remarks
Instrument components, for describing an instrument configuration when writing an
mzML file.

* * *
## Fields

| Name   | Value     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ElectronMultiplier   | 0   | Electron multiplier detector.   |
| Electrospray         | 1   | Electrospray ionization source.   |
| InductiveDetector    | 2   | Inductive detector.   |
| IonTrap              | 3   | Ion trap analyzer.   |
| Nanospray            | 4   | Nanospray ionization source.   |
| Orbitrap             | 5   | Orbitrap analyzer.   |
| Quadrupole           | 6   | Quadrupole analyzer.   |

* * *
## Example

```csharp
using Nova.Io.Write;

InstrumentComponents source = InstrumentComponents.Nanospray;
InstrumentComponents analyzer = InstrumentComponents.Orbitrap;
```
