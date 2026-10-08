---
name: IChromatDataPoint
title: IChromatDataPoint
description: The contract for a single data point in a chromatogram.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Interface
siblings: [ChromatDataPoint, IChromatogram]
---

<br/>
## Remarks
The contract for a single data point in a chromatogram: a retention time and intensity
pair. Implemented by ChromatDataPoint.

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Intensity    | double   | The intensity value for a chromatogram data point.   |
| RT           | double   | The retention time for a chromatogram data point. The unit is defined by the source, not here.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Read(BinaryReader reader)      | void   | Populates the data point from its binary representation, as written by Write.   |
| Write(BinaryWriter writer)     | void   | Serializes the data point to its binary representation, as read back by Read.   |
