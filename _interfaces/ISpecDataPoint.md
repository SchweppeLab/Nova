---
name: ISpecDataPoint
title: ISpecDataPoint
description: Abstract design for the data points stored in a spectrum.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: [favicon]
namespaces: Data
type: Interface
---

<br/>
## Remarks
Defines the minimum contents that must exist for any spectrum data point.

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

* * *
## Example
