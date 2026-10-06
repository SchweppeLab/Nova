---
name: PipesServer.Stop
title: PipesServer.Stop
member: Stop
description: Closes every client connection and shuts the listener down.
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
Closes each active client connection, which raises ClientDisconnected for each, then
releases the listener.

Stop unblocks its own listening task by connecting a throwaway client, so it takes a
moment rather than returning instantly.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Stop() | Stop the server, sending disconnect events to each client, and shutting down the current active listener.  |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");
server.Start();

// Closes every client connection, then releases the listener. Stop
// connects a throwaway client of its own to unblock the listening
// thread, so it does not return instantly.
server.Stop();
```
