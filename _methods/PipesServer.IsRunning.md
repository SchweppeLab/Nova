---
name: PipesServer.IsRunning
title: PipesServer.IsRunning
member: IsRunning
description: Reports whether the server is currently listening.
date: 2026-10-06 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipesServer
type: Method
siblings: [PipesServer]
---

<br/>
## Remarks
Start hands listening to a background task, so the server is not necessarily running
the moment Start returns. Poll this if the next step depends on the server being up.

The value becomes false once [PipesServer.Stop](/methods/PipesServer.Stop.html) has
finished unwinding the listener.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| IsRunning() | Indicates if the server has been started and is currently running.  |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| bool | True if started and running, false otherwise.  |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");
server.Start();

// Start hands listening to a background task, so the server is not
// necessarily running the instant Start returns.
while (!server.IsRunning())
{
  System.Threading.Thread.Sleep(10);
}
```
