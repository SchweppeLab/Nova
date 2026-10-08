---
name: PipeMessage
title: PipeMessage
description: A message passed through a pipe between a server and its clients.
date: 2025-04-22 13:15:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
type: Struct
interfaces: []
siblings: [PipesServer, PipesClient, PipesConnection]
---

<br/>
## Remarks
A basic structure for packaging messages that are passed through a pipe stream.
Data should be packaged into a byte[] array, presumably by a serialize function.
Each PipeMessage contains a single character message code (MsgCode), used to
indicate the type of content in the byte[] array. Only a char value of '0' is
reserved for encoding and decoding strings. Developers can use the other 254
characters for their own purposes.

* * *
## Fields

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| MsgCode    | char   | Identifies the content of MsgData; '0' is reserved for strings.   |
| MsgData    | byte[]   | The message payload.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| DecodeString()     | string   | Decoder method to convert the byte[] MsgData array to a string. Should only be called if the array originated from a string.  |
| EncodeString(string str)      | void   | Encoder method to convert any string into the byte[] MsgData. Note that the MsgCode is automatically set to '0', which is reserved for string data.   |

* * *
## Example

```csharp
using Nova.IPC.Pipes;

PipeMessage text = new PipeMessage();
text.EncodeString("Hello!");     // MsgCode is now '0'

PipeMessage data = new PipeMessage();
data.MsgCode = '1';              // a code the receiver knows how to interpret
data.MsgData = new byte[] { 1, 2, 3 };

foreach (PipeMessage message in new[] { text, data })
{
  if (message.MsgCode == '0')
  {
    Console.WriteLine(message.DecodeString());
  }
  else
  {
    Console.WriteLine("Code " + message.MsgCode + ": " + message.MsgData.Length + " bytes");
  }
}
```
