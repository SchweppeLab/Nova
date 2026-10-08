---
name: PipesConnection
title: PipesConnection
description: Manages a connection between a server and a client.
date: 2026-10-08 09:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
type: Class
interfaces: []
classes: []
siblings: [PipesServer, PipesClient, PipeMessage]
---

<br/>
## Remarks
Manages a connection between a server and a client. PipesServer creates one for each
client and passes it to its ClientConnected, ClientDisconnected and ClientMessage events.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesConnection(int id, string name, PipeStream serverStream) | Creates a connection from the server's stream. id is an iterative connection ID and name is the client's name.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ID  | int   | Numeric identifier. A read-only field.      |
| IsConnected  | bool   | Indicates if the stream is connected.      |
| Name  | string   | String identifier for the client on this stream.      |
| TheStream  | PipeStream   | The stream between server and client.      |

* * *
## Events

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Disconnected  | PipeConnectionEvent   | Raised when the stream has been disconnected.        |
| Error  | PipeConnectionExceptionEvent   | Raised on an error.        |
| ReceiveMessage  | PipeConnectionMessageEvent   | Raised when the stream receives a message.        |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Close()      | void   | Closes the stream.    |
| Open()      | void   | Opens the connection for reading and writing.    |
| Send(PipeMessage message)      | void   | Puts a PipeMessage in the queue to be sent.    |

* * *
## Delegates

| Delegate   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| PipeConnectionEvent(PipesConnection pc)  | void   | Handles a connection state event, such as disconnection.         |
| PipeConnectionMessageEvent(PipesConnection pc, PipeMessage message)  | void   | Handles a message received on a connection.         |
| PipeConnectionExceptionEvent(PipesConnection pc, Exception ex)  | void   | Handles an error on a connection.         |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesServer server = new PipesServer("NovaDemoServer");

// The server passes each client's PipesConnection to its events.
server.ClientConnected += connection =>
{
  Console.WriteLine(connection.Name + " connected: " + connection.IsConnected);

  PipeMessage welcome = new PipeMessage();
  welcome.EncodeString("Welcome.");
  connection.Send(welcome);
};

server.Start();
```
