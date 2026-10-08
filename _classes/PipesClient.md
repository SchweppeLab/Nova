---
name: PipesClient
title: PipesClient
description: A named pipe client that sends messages to and receives messages from a PipesServer.
date: 2025-04-23 11:45:00 -0700
layout: post
tags: []
namespaces: IPC.Pipes
type: Class
interfaces: []
classes: []
siblings: [PipesServer, PipeMessage, PipesConnection]
---

<br/>
## Remarks
The PipesClient class wraps around the NamedPipeClientStream class. Each PipesClient
establishes a single pipe between itself and a PipesServer, allowing messages to be both
received and sent. If the server goes down or is not available, the client reverts to a
listening state, and reconnects when the server comes back online.

* * *
## Constructors

| Syntax   | Description                                               |
|:-------------|:----------------------------------------------------------|
| PipesClient(string pID, string sID = ".") | PipesClient constructor. pID should match the ID of the server. sID defaults to ".", indicating a local server; otherwise provide the network ID.  |

* * *
## Properties

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| AutoReconnect   | bool   | Indicates if the client should attempt to reconnect upon a broken pipe.   |

* * *
## Events

| Identifier   | Type     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Disconnected  | PipeConnectionEvent   | Indicate disconnection event.   |
| Error  | PipeExceptionEventHandler   | Indicate error occurred.   |
| ServerMessage  | PipeConnectionMessageEvent   | Indicate message received from server.   |

* * *
## Methods

| Method   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| Send(PipeMessage message)     | void   | Sends a message to the server.   |
| Start() | void    | Starts the client.   |
| Stop()  | void    | Stop the client, and do not attempt to reconnect.   |
| WaitForConnection(int ms)  | void    | Blocks until the client has connected to the server, or the timeout elapses.   |
| WaitForDisconnection(int ms)  | void    | Blocks until the client has disconnected from the server, or the timeout elapses.   |

* * *
## Delegates

| Delegate   | Returns     | Description                                               |
|:-------------|:---------|:----------------------------------------------------------|
| PipeExceptionEventHandler(Exception exception)  | void   | Handles an error raised by a pipes client or server.   |

* * *
## Example

```csharp
using Nova.IPC.Pipes;
using System.IO;

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

// Stands in for your own message payload type.
public class SomeObject
{
  public int intData = 1;
  public double doubleData = 2.5;
  public string strData = "three";

  public byte[] Serialize()
  {
    using (MemoryStream stream = new MemoryStream())
    using (BinaryWriter writer = new BinaryWriter(stream))
    {
      writer.Write(intData);
      writer.Write(doubleData);
      writer.Write(strData);
      return stream.ToArray();
    }
  }

  public void Deserialize(byte[] data)
  {
    using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
    {
      intData = reader.ReadInt32();
      doubleData = reader.ReadDouble();
      strData = reader.ReadString();
    }
  }
}
```
