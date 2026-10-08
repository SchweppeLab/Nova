---
name: PipeMessage.DecodeString
title: PipeMessage.DecodeString
member: DecodeString
description: Decodes MsgData to a string.
date: 2026-10-08 12:00:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
class: PipeMessage
type: Method
siblings: [PipeMessage, PipesServer, PipesClient]
---

<br/>
## Remarks
Check that MsgCode is '0' before decoding.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| DecodeString() | Decoder method to convert the byte[] MsgData array to a string. Should only be called if the array originated from a string. |

#### Returns

| Type   | Description                                               |
|:-------------|:----------------------------------------------------------|
| string | The MsgData decoded to a string. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipesClient client = new PipesClient("NovaDemoServer");
client.ServerMessage += (connection, message) =>
{
  if (message.MsgCode == '0')
  {
    Console.WriteLine("Server says: " + message.DecodeString());
  }
};
client.Start();
```
