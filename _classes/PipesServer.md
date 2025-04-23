---
name: PipesServer
title: PipesServer
description: A class for sending data to and receiving data from client processes.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: IPC.Pipes
type: Class
interfaces: []
classes: []
siblings: [PipesClient,PipeMessage]
---

<br/>
## Remarks
This class implements a lightweight server based on the named pipes from System.IO.Pipes.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesServer (string sID) | Creates a PipesServer with the sID identifier.  |

* * *
## Events

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ClientConnected  | PipeConnectionEvent   | Raised when client has connected.        |
| ClientDisconnected  | PipeConnectionEvent   | Raised when client has disconnected.        |
| ClientMessage  | PipeConnectionMessageEvent   | Raised upon reception of client message.        |
| Error  | PipeExceptionEventHandler   | Raised when an error has occurred.       |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| IsRunning()      | bool   |Returns true if the server has been started and is actively listening.         |
| Send(PipeMessage message)     | void   |Sends a message to all clients connected to the server.         |
| Send(PipeMessage message, string clientID)    | void   | Sends a message to a specific client from the list of connections.  |
| Start() | void    | Starts the server.   |
| Stop()  | void    | Stops the server, sending disconnect events to each client, and shuts down the current active listener.   |

* * *
## Example

```csharp
// Example C# code
using Nova.IPC.Pipes;

public class PipeServer
{

  private PipesServer server;

  public PipeServer(string pipeName)
  {
    server = new PipesServer(pipeName);
    server.ClientConnected += OnClientConnected;
    server.ClientDisconnected += OnClientDisconnected;
    server.ClientMessage += OnClientMessage;
    server.Error += OnError;
    server.Start();
    while (KeepRunning)
    {
      // Do nothing - wait for user to press 'q' key
    }
    server.Stop();
  }

  // Receives console key input from user.
  private bool KeepRunning
  {
    get
    {
      var key = Console.ReadKey();
      if (key.Key == ConsoleKey.Q) return false;
      else if (key.Key == ConsoleKey.S)
      {
        //PipeMessage sent by this server have a MsgCode of '1'.
        //The client should know what that code means, and how to interpret the associate MsgData
        PipeMessage pm = new PipeMessage();
        pm.MsgCode='1';
        pm.MsgData=new SomeObject().Serialize();
        server.Send(pm);
      }
      return true;
    }
  }

  private void OnClientConnected(PipesConnection connection)
  {
    Console.WriteLine("Client Connected: "+connection.ID);
    PipeMessage pm = new PipeMessage();
    pm.EncodeString("Welcome!");
    connection.Send(pm);
  }

  private void OnClientDisconnected(PipesConnection connection)
  {
    Console.WriteLine("Client {0} disconnected", connection.ID);
  }

  private void OnClientMessage(PipesConnection connection, PipeMessage message)
  {
    //Note that the server will only process string messages from the client. All other messages
    //remain unprocessed (other than to notify the user that they were received).
    switch (message.MsgCode)
    {
      case '0':
        Console.WriteLine("Client {0} says: {1}", connection.ID,message.DecodeString());
        break;
      default:
        Console.WriteLine("Server received unrecognized message code from {0}: {1}", connection.ID, message.MsgCode);
        break;
    }
  }

  private void OnError(Exception exception)
  {
    Console.Error.WriteLine("ERROR: {0}", exception);
  }

  public static void Main()
  {
    Console.WriteLine("Running in SERVER mode");
    Console.WriteLine("Press 's' to send an object message to the client");
    Console.WriteLine("Press 'q' to exit");
    new PipeServer("TestServer");
  }

}
```
