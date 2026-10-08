// Published verbatim as the Example on PipesClient. Regenerate from the page, don't hand-edit.
using System;
using Nova.IPC.Pipes;
using System.IO;

namespace NovaSnippets.IpcPipes.ClientExample
{

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
}
