---
name: PipesClient.WaitForConnection
title: PipesClient.WaitForConnection
member: WaitForConnection
description: Blocks until the client has connected to the server, or the timeout elapses.
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
It does not report which of the two happened.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| WaitForConnection(int ms) | Blocks until the client has connected to the server, or the timeout elapses. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ms | int | Milliseconds to wait. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesClient client = new PipesClient("NovaDemoServer");
client.Start();

// Wait up to five seconds before sending.
client.WaitForConnection(5000);
```
