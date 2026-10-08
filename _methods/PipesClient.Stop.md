---
name: PipesClient.Stop
title: PipesClient.Stop
member: Stop
description: Stops the client, and does not attempt to reconnect.
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
The connection is closed and the Disconnected event is raised. The client stays
disconnected even when AutoReconnect is true.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Stop() | Stop the client, and do not attempt to reconnect. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesClient client = new PipesClient("NovaDemoServer");
client.Start();
client.WaitForConnection(5000);

// ... exchange messages ...

client.Stop();
client.WaitForDisconnection(5000);
```
