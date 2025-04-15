---
name: SpecDataPoint
title: SpecDataPoint
description: The basic unit of information in a spectrum (i.e., m/z and intensity).
class: struct
layout: namespace
menu:
  - item1:
    name: Data
    title: Data Namespace
    collection: namespaces
  - item2:
    name: SpecDataPointEx
    title: SpecDataPointEx
    collection: ns_data
---

<br/>
## Remarks
Some remarks beyond the short description.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| SpecDataPoint(double mz=0, double intensity = 0) | Initializes the Mz and Intensity properties to the values provided.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Mz           | double   | Gets/sets the m/z value of a spectral data point.         |
| Intensity    | double   | Gets/sets the intensity value of a spectral data point.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Read (BinaryReader)      | void   | Reads the Mz and Intensity value from a BinaryReader.         |
| Write (BinaryWriter)     | void   | Writes the Mz and Intensity value to a BinaryWriter.  |
| CompareTo (SpecDataPoint)| int    | Performs the CompareTo function on the Mz of two SpecDataPoints to identify the lower value.   |

* * *
## Example

* * *
## See also...
