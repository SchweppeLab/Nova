---
name: TSpectrum.Constructor
title: TSpectrum&lt;T>.Constructor
member: Constructor
description: Creates a spectrum sized for the given number of data points.
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
Spectrum and SpectrumEx call this from their own constructors, which are the ones to use.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| TSpectrum(int count = 0) | Creates a spectrum sized for the given number of data points. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| count | int | The number of data points. |

* * *
## Example

```csharp
using Nova.Data;

// Through Spectrum, which is TSpectrum<SpecDataPoint>.
Spectrum spectrum = new Spectrum(100);
Console.WriteLine(spectrum.Count);   // 100
```
