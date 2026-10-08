---
name: PipesConnection.Close
title: PipesConnection.Close
member: Close
description: Closes the stream.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipesConnection
type: Method
siblings: [PipesConnection, PipesServer, PipesClient]
---

<br/>
## Remarks
Closing raises the connection's Disconnected event.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Close() | Closes the stream. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");

// Drop a client that says goodbye.
server.ClientMessage += (connection, message) =>
{
  if (message.MsgCode == '0' && message.DecodeString() == "bye")
  {
    connection.Close();
  }
};

server.Start();
```
