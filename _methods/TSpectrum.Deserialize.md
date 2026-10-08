---
name: TSpectrum.Deserialize
title: TSpectrum&lt;T>.Deserialize
member: Deserialize
description: Reads a byte array into spectrum object data members.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: Data
class: TSpectrum
type: Method
siblings: [TSpectrum, Spectrum, SpectrumEx]
---

<br/>
## Remarks
Turns a byte array made by [Serialize]({{ '/methods/TSpectrum.Serialize.html' | relative_url }}) back into a spectrum.

The current version of Serialize does not carry some of the more obscure data members,
so those are not restored.

Inherited by [Spectrum]({{ '/classes/Spectrum.html' | relative_url }}) and [SpectrumEx]({{ '/classes/SpectrumEx.html' | relative_url }}).

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Deserialize(byte[] data) | Reads a byte array into spectrum object data members.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| data  | byte[]   | A byte array produced by Serialize.   |

* * *
## Example

```csharp
using Nova.Data;

Spectrum copy = new Spectrum();
copy.Deserialize(payload);                  // payload from Serialize
Console.WriteLine(copy.ScanNumber + ": " + copy.Count + " points");
```
