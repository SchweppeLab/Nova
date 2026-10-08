---
name: TSpectrum.Dispose
title: TSpectrum&lt;T>.Dispose
member: Dispose
description: A no-op beyond marking the instance disposed.
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
A spectrum holds no unmanaged resources, so disposing it is optional. It is IDisposable so it can be used in a using block.

Inherited by [Spectrum]({{ '/classes/Spectrum.html' | relative_url }}) and [SpectrumEx]({{ '/classes/SpectrumEx.html' | relative_url }}).

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Dispose() | A no-op beyond marking the instance disposed; a spectrum holds no unmanaged resources.  |

* * *
## Example

```csharp
using Nova.Data;

using (Spectrum spectrum = new Spectrum(100))
{
  Console.WriteLine(spectrum.Count);
}
```
