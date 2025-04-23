---
name: PipeMessage
title: PipeMessage
description: Structure containing the contents of a message passed between server and clients.
date: 2025-04-22 13:15:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
type: Struct
interfaces: []
siblings: []
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
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipeMessage | Default constructor. |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| MsgCode    | char   | A single character to indicate the type of message.        |
| MsgData    | byte[]   | The contents of the message encoded to a byte array.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| DecodeString()     | string   | Decoder method to convert the byte[] MsgData array to a string. Should only be called if the array originated from a string.  |
| EncodeString(string str)      | void   | Encoder method to convert any string into the byte[] MsgData. Note that the MsgCode is automatically set to '0', which is reserved for string data.         |


* * *
## Example

None yet.