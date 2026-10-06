---
name: PipesServer.Send
title: PipesServer.Send
member: Send
description: Sends a message to every connected client, or to one named client.
date: 2026-10-06 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipesServer
type: Method
siblings: [PipesServer, PipesClient, PipeMessage]
---

<br/>
## Remarks
Sends a message over the named pipe to clients that have completed the handshake and
are currently connected. Clients that have disconnected are not in the connection
list and are skipped.

Sending to no clients is not an error. If nothing is connected, the call returns
having done nothing.

The server must be started before a message can reach anyone. See
[PipesServer.Start](/methods/PipesServer.Start.html).

* * *
## Overloads

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Send(PipeMessage message) | Sends the message to every connected client.  |
| Send(PipeMessage message, string clientID) | Sends the message to the one client with the given identifier.  |

* * *
## Send(PipeMessage message)

Sends a message to all clients connected to the server.

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| message  | PipeMessage   | The message wrapped in a PipeMessage structure.      |

#### Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");
server.Start();

PipeMessage message = new PipeMessage();
message.MsgCode = 'T';
message.EncodeString("Acquisition started.");

// Goes to every connected client. No-op when none are connected.
server.Send(message);
```

* * *
## Send(PipeMessage message, string clientID)

Sends a message to a specific client from the list of connections.

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| message  | PipeMessage   | The message wrapped in a PipeMessage structure.      |
| clientID  | string   | The identifier of the client. This is the name the server assigned during the handshake, available as the Name property of the PipesConnection passed to the ClientConnected event.      |

A clientID that matches no connection is not an error. Nothing is sent.

#### Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");

server.ClientConnected += connection =>
{
  PipeMessage greeting = new PipeMessage();
  greeting.MsgCode = 'T';
  greeting.EncodeString("Welcome.");

  // connection.Name is the identifier the server assigned during the
  // handshake, and is what the clientID argument matches against.
  server.Send(greeting, connection.Name);
};

server.Start();
```
