---
name: SpectrumFileReaderFactory
title: SpectrumFileReaderFactory
description: Factory class supporting multiple MS data file types.
date: 2025-04-23 14:00:00 -0700
layout: post
tags: []
namespaces: Io.Read
type: Class
interfaces: []
classes: []
siblings: [ISpectrumFileReader,FileReader]
---

<br/>
## Remarks
Factory class for dynamic file support when reading different mass spectrometry data file types.


* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| GetReader(string file, MSFilter filter)     | ISpectrumFileReader   | Opens a file and returns a file reader object based on the extension provided in the file name. A filter is provided to restrict the types of scans that are read.  |


* * *
## Example

```csharp
```
