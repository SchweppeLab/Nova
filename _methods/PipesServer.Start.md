---
name: PipesServer.Start
title: PipesServer.Start
member: Start
description: Begins listening for client connections.
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
Starts listening on a background task and returns immediately. The server accepts
connections until [PipesServer.Stop]({{ '/methods/PipesServer.Stop.html' | relative_url }}) is called.

Attach event handlers before calling this. A client that connects before
ClientConnected has a subscriber raises the event into nothing.

Because listening is handed to a background task, the server may not be up the instant
this returns. [PipesServer.IsRunning]({{ '/methods/PipesServer.IsRunning.html' | relative_url }}) reports the
actual state.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Start() | Starts the server.  |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");

// Subscribe before starting, or early connections are missed.
server.ClientConnected += connection =>
  Console.WriteLine("Connected: " + connection.Name);

server.ClientDisconnected += connection =>
  Console.WriteLine("Disconnected: " + connection.Name);

server.ClientMessage += (connection, message) =>
  Console.WriteLine(connection.Name + " said: " + message.DecodeString());

server.Start();
```
