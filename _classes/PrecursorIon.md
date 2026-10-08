---
name: PrecursorIon
title: PrecursorIon
description: The precursor ion of a dependent scan and how it was isolated and fragmented.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Class
interfaces: []
classes: []
siblings: [SpectrumFoundation, FragmentationType]
---

<br/>
## Remarks
The precursor ion of a dependent scan and how it was isolated and fragmented. A spectrum
holds zero or more in its Precursors list.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| [PrecursorIon()]({{ '/methods/PrecursorIon.Constructor.html' | relative_url }}) | Creates an empty precursor.  |
| [PrecursorIon(PrecursorIon pi)]({{ '/methods/PrecursorIon.Constructor.html' | relative_url }}) | Copy constructor.  |
| [PrecursorIon(double mz, double intensity = 0, int charge = 0, double isoMz = 0, double isoWidth = 0)]({{ '/methods/PrecursorIon.Constructor.html' | relative_url }}) | Creates a precursor from its monoisotopic m/z, with optional intensity, charge, isolation m/z and isolation width.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Charge        | int      | Charge state of the precursor.   |
| CollisionEnergy        | double      | If a dependent scan, the fragmentation energy used.   |
| FragmentationMethod        | FragmentationType      | The fragmentation method used, or FragmentationType.None if not known.   |
| Intensity        | double      | Intensity of the IsolationMz precursor peak.   |
| IsolationMz        | double      | The m/z that the instrument targeted for isolation.   |
| IsolationSpecificity        | double      | Proportion of the intensity in the isolation window that belongs to the precursor, from zero to one.   |
| IsolationWidth        | double      | The size of the window that the instrument targeted for isolation.   |
| MonoisotopicMz        | double      | The putative monoisotopic m/z peak of the isotope envelope that contains the isolation m/z.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [Clear()]({{ '/methods/PrecursorIon.Clear.html' | relative_url }})      | void   | Resets all values to 0.   |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
foreach (Spectrum spectrum in reader)
{
  foreach (PrecursorIon precursor in spectrum.Precursors)
  {
    Console.WriteLine(precursor.MonoisotopicMz + "\t" + precursor.Charge
      + "\t" + precursor.FragmentationMethod);
  }
}
```

```csharp
PrecursorIon precursor = new PrecursorIon(652.3412, 1.2e6, 2, 652.84, 1.6);
precursor.FragmentationMethod = FragmentationType.HCD;

// The copy constructor carries every field.
PrecursorIon copy = new PrecursorIon(precursor);
```
