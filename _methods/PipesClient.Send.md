---
name: PipesClient.Send
title: PipesClient.Send
member: Send
description: Sends a message to the server.
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
A message sent before the client has connected is dropped. Use WaitForConnection first.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| Send(PipeMessage message) | Sends a message to the server. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| message | PipeMessage | The message wrapped in a PipeMessage structure. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesClient client = new PipesClient("NovaDemoServer");
client.Start();
client.WaitForConnection(5000);

PipeMessage message = new PipeMessage();
message.EncodeString("Hello!");
client.Send(message);
```
