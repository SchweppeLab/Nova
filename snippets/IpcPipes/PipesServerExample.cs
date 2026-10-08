// Published verbatim as the Example on PipesServer. Regenerate from the page, don't hand-edit.
using System;
using Nova.IPC.Pipes;
using System.IO;

namespace NovaSnippets.IpcPipes.ServerExample
{

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
}
