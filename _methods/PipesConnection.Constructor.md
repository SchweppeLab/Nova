---
name: PipesConnection.Constructor
title: PipesConnection.Constructor
member: Constructor
description: Creates a connection from the server's stream.
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
PipesServer and PipesClient create and open their own connections. Code using them
receives a connection through their events rather than constructing one.

* * *
## Syntax

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesConnection(int id, string name, PipeStream serverStream) | Constructor that passes the stream from the server, plus any identifiers. |

#### Parameters

| Name   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| id | int | An iterative connection ID. |
| name | string | The name of the client. |
| serverStream | PipeStream | The PipeStream from the server. |
