---
name: TSpectrum.Serialize
title: TSpectrum&lt;T>.Serialize
member: Serialize
description: Writes spectrum object data members into a byte array.
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
Converts the spectrum to a byte array, for storage or for sending to another process.
[Deserialize]({{ '/methods/TSpectrum.Deserialize.html' | relative_url }}) turns the array back into a spectrum.

The current version does not serialize some of the more obscure data members.

Inherited by [Spectrum]({{ '/classes/Spectrum.html' | relative_url }}) and [SpectrumEx]({{ '/classes/SpectrumEx.html' | relative_url }}).

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Serialize() | Writes spectrum object data members into a byte array. Not every member is carried.  |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| byte[] | The serialized spectrum.  |

* * *
## Example

```csharp
using Nova.Data;
using Nova.Io.Read;

FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
Spectrum spectrum = reader.ReadSpectrum();

byte[] payload = spectrum.Serialize();      // store it, or send it in a PipeMessage
```
