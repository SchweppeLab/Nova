---
name: PipesConnection.Send
title: PipesConnection.Send
member: Send
description: Puts a PipeMessage in the queue to be sent.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipesConnection
type: Method
siblings: [PipesConnection, PipesServer, PipeMessage]
---

<br/>
## Remarks
Send returns at once. Messages are sent in the order they were queued.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Send(PipeMessage message) | Puts a PipeMessage in the queue to be sent. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| message | PipeMessage | The PipeMessage. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");

// Reply to the client that sent the message.
server.ClientMessage += (connection, message) =>
{
  PipeMessage reply = new PipeMessage();
  reply.EncodeString("Received.");
  connection.Send(reply);
};

server.Start();
```
