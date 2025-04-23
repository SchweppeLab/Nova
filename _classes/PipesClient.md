---
name: PipesClient
title: PipesClient
description: A class for sending data to and receiving data from a server process.
date: 2025-04-23 11:45:00 -0700
layout: post
tags: [favicon]
namespaces: IPC.Pipes
type: Class
interfaces: []
classes: []
siblings: [PipesServer, PipeMessage]
---

<br/>
## Remarks
This class implements a lightweight inter process communication client based on the named pipes from System.IO.Pipes.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesClient(string pID, string sID = ".") | Creates a PipesClient where pID should match an existing PipesServer identifier. An sID vale of "." indicates a local server. Otherwise provide network ID.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| AutoReconnect   | bool   | Indicates if the client should attempt to reconnect upon a broken pipe.        |

* * *
## Events

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Disconnected  | PipeConnectionEvent   | Raised when client has disconnected.        |
| ServerMessage  | PipeConnectionMessageEvent   | Raised upon reception of a server message.        |
| Error  | PipeExceptionEventHandler   | Raised when an error has occurred.       |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|  |
| Send(PipeMessage message)     | void   |Sends a message to the server.         |
| Start() | void    | Starts the client.   |
| Stop()  | void    | Stops the client, and does not attempt to reconnect.   |
| WaitForConnection(int ms)  | void    |   |
| WaitForDisconnection(int ms)  | void    |   |

* * *
## Delegates

| Delegate   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|  |
| PipeExceptionEventHandler  | void   | Represents the method that will handle the Error event         |


* * *
## Example

```csharp
// Example C# code
using Nova.IPC.Pipes;

public class PipeClient
{

  private PipesClient client;

  public PipeClient(string pipeName)
  {
    client = new PipesClient(pipeName);
    client.ServerMessage += OnServerMessage;
    client.Error += OnError;
    client.Start();
    while (KeepRunning)
    {
      // Do nothing - wait for user to press 'q' key
    }
    client.Stop();
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
        PipeMessage pm = new PipeMessage();
        pm.EncodeString("Hello!");
        client.Send(pm);
      }
      return true;
    }
  }

  public static void Main()
  {
    Console.WriteLine("Running in CLIENT mode");
    Console.WriteLine("Press 's' to send a string message to the server");
    Console.WriteLine("Press 'q' to exit");
    new PipeClient("TestServer");
  }

  private void OnServerMessage(PipesConnection connection, PipeMessage message)
  {
    switch (message.MsgCode)
    {
      case '0': 
        Console.WriteLine("Server says: {0}", message.DecodeString());
        break;
      case '1':
        SomeObject obj = new SomeObject();
        obj.Deserialize(message.MsgData);
        PrintMessage(obj);
        break;
      default:
        Console.WriteLine("Server sent unrecognized message code: {0}",message.MsgCode);
        break;
    }
    
  }

  private void OnError(Exception exception)
  {
    Console.Error.WriteLine("ERROR: {0}", exception);
  }

  private void PrintMessage(SomeObject obj)
  {
    Console.WriteLine("SomeObject int:    " + obj.intData.ToString());
    Console.WriteLine("SomeObject double: " + obj.doubleData.ToString());
    Console.WriteLine("SomeObject string: " + obj.strData);
  }
  
}
```
