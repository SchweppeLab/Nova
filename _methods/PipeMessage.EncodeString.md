---
name: PipeMessage.EncodeString
title: PipeMessage.EncodeString
member: EncodeString
description: Encodes a string into MsgData and sets MsgCode to '0'.
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
Any MsgData already in the message is replaced.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| EncodeString(string str) | Encoder method to convert any string into the byte[] MsgData. Note that the MsgCode is automatically set to '0', which is reserved for string data. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| str | string | The string to be encoded. |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipeMessage message = new PipeMessage();
message.EncodeString("Hello!");
Console.WriteLine(message.MsgCode);   // 0
```
