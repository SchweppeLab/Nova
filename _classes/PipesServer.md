---
name: PipesServer
title: PipesServer
description: A named pipe server that manages connections from multiple PipesClient instances.
date: 2025-04-15 11:18:14 -0700
layout: post
tags: []
namespaces: IPC.Pipes
type: Class
interfaces: []
classes: []
siblings: [PipesClient, PipeMessage, PipesConnection]
---

<br/>
## Remarks
The PipesServer class wraps the NamedPipeServerStream class, and manages multiple
connections from clients. Messages can be received from clients, or broadcast to individual
or all clients. The server establishes client identities through a handshake, and names
those clients for the duration of their connection.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| [PipesServer(string sID)]({{ '/methods/PipesServer.Constructor.html' | relative_url }}) | Server constructor. sID is the identity of the server.  |

* * *
## Events

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| ClientConnected  | PipeConnectionEvent   | Indicate when client has connected.   |
| ClientDisconnected  | PipeConnectionEvent   | Indicate when client has disconnected.   |
| ClientMessage  | PipeConnectionMessageEvent   | Indicate reception of client message.   |
| Error  | PipeExceptionEventHandler   | Indicate error has occurred.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| [IsRunning()]({{ '/methods/PipesServer.IsRunning.html' | relative_url }})      | bool   | Indicates if the server has been started and is currently running.   |
| [Send(PipeMessage message)]({{ '/methods/PipesServer.Send.html' | relative_url }})     | void   | Sends a message to all clients connected to the server.   |
| [Send(PipeMessage message, string clientID)]({{ '/methods/PipesServer.Send.html' | relative_url }})    | void   | Sends a message to a specific client from the list of connections.   |
| [Start()]({{ '/methods/PipesServer.Start.html' | relative_url }}) | void    | Starts the server.   |
| [Stop()]({{ '/methods/PipesServer.Stop.html' | relative_url }})  | void    | Stop the server, sending disconnect events to each client, and shutting down the current active listener.   |

* * *
## Example

```csharp
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
