---
name: ISpecDataPoint
title: ISpecDataPoint
description: The common contract for a single data point in a mass spectrum.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: Data
type: Interface
siblings: [SpecDataPoint, SpecDataPointEx]
---

<br/>
## Remarks
The common contract for a single data point in a mass spectrum, minimally an m/z and
intensity pair. TSpectrum&lt;T&gt; is written against this interface so one spectrum
implementation serves both SpecDataPoint and SpecDataPointEx.

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Mz           | double   | The mass to charge ratio (m/z) value for a mass spectrum data point.   |
| Intensity    | double   | The intensity value for a mass spectrum data point.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [Read(BinaryReader reader)]({{ '/methods/ISpecDataPoint.Read.html' | relative_url }})      | void   | Populates the data point from its binary representation, as written by Write.   |
| [Write(BinaryWriter writer)]({{ '/methods/ISpecDataPoint.Write.html' | relative_url }})     | void   | Serializes the data point to its binary representation, as read back by Read.   |
