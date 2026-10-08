// Generated from the Example sections of the method pages. Do not hand-edit;
// change the page and regenerate.

using Nova.IPC.Pipes;
using System;

namespace NovaSnippets.IpcPipes
{
  internal static class PipesMethodSnippets
  {
    internal static void PipesClient_Constructor()
    {
      // A server on this computer.
      PipesClient client = new PipesClient("NovaDemoServer");

      // A server on another computer on the network.
      PipesClient remote = new PipesClient("NovaDemoServer", "LabPC");
    }

    internal static void PipesClient_Send()
    {
      PipesClient client = new PipesClient("NovaDemoServer");
      client.Start();
      client.WaitForConnection(5000);

      PipeMessage message = new PipeMessage();
      message.EncodeString("Hello!");
      client.Send(message);
    }

    internal static void PipesClient_Start()
    {
      PipesClient client = new PipesClient("NovaDemoServer");
      client.ServerMessage += (connection, message) =>
        Console.WriteLine("Server says: " + message.DecodeString());

      client.Start();
      client.WaitForConnection(5000);
    }

    internal static void PipesClient_Stop()
    {
      PipesClient client = new PipesClient("NovaDemoServer");
      client.Start();
      client.WaitForConnection(5000);

      // ... exchange messages ...

      client.Stop();
      client.WaitForDisconnection(5000);
    }

    internal static void PipesClient_WaitForConnection()
    {
      PipesClient client = new PipesClient("NovaDemoServer");
      client.Start();

      // Wait up to five seconds before sending.
      client.WaitForConnection(5000);
    }

    internal static void PipesClient_WaitForDisconnection()
    {
      PipesClient client = new PipesClient("NovaDemoServer");
      client.Start();
      client.WaitForConnection(5000);

      client.Stop();
      client.WaitForDisconnection(5000);
    }

    internal static void PipesConnection_Close()
    {
      PipesServer server = new PipesServer("NovaDemoServer");

      // Drop a client that says goodbye.
      server.ClientMessage += (connection, message) =>
      {
        if (message.MsgCode == '0' && message.DecodeString() == "bye")
        {
          connection.Close();
        }
      };

      server.Start();
    }

    internal static void PipesConnection_Send()
    {
      PipesServer server = new PipesServer("NovaDemoServer");

      // Reply to the client that sent the message.
      server.ClientMessage += (connection, message) =>
      {
        PipeMessage reply = new PipeMessage();
        reply.EncodeString("Received.");
        connection.Send(reply);
      };

      server.Start();
    }

    internal static void PipeMessage_DecodeString()
    {
      PipesClient client = new PipesClient("NovaDemoServer");
      client.ServerMessage += (connection, message) =>
      {
        if (message.MsgCode == '0')
        {
          Console.WriteLine("Server says: " + message.DecodeString());
        }
      };
      client.Start();
    }

    internal static void PipeMessage_EncodeString()
    {
      PipeMessage message = new PipeMessage();
      message.EncodeString("Hello!");
      Console.WriteLine(message.MsgCode);   // 0
    }
  }
}
