---
name: PipesServer.Constructor
title: PipesServer Constructor
member: Constructor
description: Creates a server under an identifier that clients use to find it.
date: 2026-10-06 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipesServer
type: Method
siblings: [PipesServer, PipesClient]
---

<br/>
## Remarks
Creates the server but does not start it. Nothing listens until
[PipesServer.Start]({{ '/methods/PipesServer.Start.html' | relative_url }}) is called, so event handlers can
be attached in between.

The identifier is the rendezvous point. A PipesClient constructed with the same string
connects to this server, so the two must agree on it.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesServer(string sID) | Server constructor.  |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| sID  | string   | Identity of the server.      |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

// The identifier is how clients find this server. A PipesClient
// constructed with the same string connects to it.
PipesServer server = new PipesServer("NovaDemoServer");
```
