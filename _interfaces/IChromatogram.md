---
name: IChromatogram
title: IChromatogram
description: The contract for a chromatogram.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: Data
type: Interface
siblings: [Chromatogram, IChromatDataPoint]
---

<br/>
## Remarks
The contract for a chromatogram: a series of retention time and intensity data points.
Implemented by Chromatogram.

#### Implements
IDisposable

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Count        | int      | The number of data points.   |
| DataPoints   | ChromatDataPoint[]   | The data points.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Deserialize(byte[] data)      | void   | Replaces the contents with the data points from a Serialize payload.   |
| Resize(int sz)      | void   | Replaces the data points with a new, empty array of the given size. Existing points are discarded.   |
| Serialize()      | byte[]   | Serializes the chromatogram to a byte array, suitable as a pipe message payload. Read back by Deserialize.   |
