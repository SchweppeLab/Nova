---
name: PipesClient.Constructor
title: PipesClient.Constructor
member: Constructor
description: Creates a client for a named PipesServer.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipesClient
type: Method
siblings: [PipesClient, PipesServer, PipeMessage]
---

<br/>
## Remarks
The client does not connect until Start is called.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesClient(string pID, string sID = ".") | PipesClient constructor. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| pID | string | Should match the ID of the server. |
| sID | string | Default value is "." indicating local server. Otherwise provide network ID. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

// A server on this computer.
PipesClient client = new PipesClient("NovaDemoServer");

// A server on another computer on the network.
PipesClient remote = new PipesClient("NovaDemoServer", "LabPC");
```
