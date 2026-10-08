---
name: PipesClient.Start
title: PipesClient.Start
member: Start
description: Starts the client.
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
Start returns at once and connects in the background, waiting for the server if it is not
yet running. Subscribe to the events before calling Start, and use WaitForConnection when
the next step needs the connection.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Start() | Starts the client. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesClient client = new PipesClient("NovaDemoServer");
client.ServerMessage += (connection, message) =>
  Console.WriteLine("Server says: " + message.DecodeString());

client.Start();
client.WaitForConnection(5000);
```
