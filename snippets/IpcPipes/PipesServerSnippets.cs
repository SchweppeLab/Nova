// Snippets published on the PipesServer method pages.
// One method here per documented method. Keep the names in step with the pages.

using System;
using Nova.IPC.Pipes;

namespace NovaSnippets.IpcPipes
{
  internal static class PipesServerSnippets
  {
    // PipesServer(string sID)
    internal static void Constructor()
    {
      // The identifier is how clients find this server. A PipesClient
      // constructed with the same string connects to it.
      PipesServer server = new PipesServer("NovaDemoServer");

      Console.WriteLine(server.IsRunning());
    }

    // Start()
    internal static void Start()
    {
      PipesServer server = new PipesServer("NovaDemoServer");

      // Subscribe before starting, or early connections are missed.
      server.ClientConnected += connection =>
        Console.WriteLine("Connected: " + connection.Name);

      server.ClientDisconnected += connection =>
        Console.WriteLine("Disconnected: " + connection.Name);

      server.ClientMessage += (connection, message) =>
        Console.WriteLine(connection.Name + " said: " + message.DecodeString());

      server.Start();
    }

    // IsRunning()
    internal static void IsRunning()
    {
      PipesServer server = new PipesServer("NovaDemoServer");
      server.Start();

      // Start hands listening to a background task, so the server is not
      // necessarily running the instant Start returns.
      while (!server.IsRunning())
      {
        System.Threading.Thread.Sleep(10);
      }

      Console.WriteLine("Listening.");
    }

    // Send(PipeMessage message)
    internal static void SendToEveryClient()
    {
      PipesServer server = new PipesServer("NovaDemoServer");
      server.Start();

      PipeMessage message = new PipeMessage();
      message.MsgCode = 'T';
      message.EncodeString("Acquisition started.");

      // Goes to every connected client. No-op when none are connected.
      server.Send(message);
    }

    // Send(PipeMessage message, string clientID)
    internal static void SendToOneClient()
    {
      PipesServer server = new PipesServer("NovaDemoServer");

      server.ClientConnected += connection =>
      {
        PipeMessage greeting = new PipeMessage();
        greeting.MsgCode = 'T';
        greeting.EncodeString("Welcome.");

        // connection.Name is the identifier the server assigned during the
        // handshake, and is what the clientID argument matches against.
        server.Send(greeting, connection.Name);
      };

      server.Start();
    }

    // Stop()
    internal static void Stop()
    {
      PipesServer server = new PipesServer("NovaDemoServer");
      server.Start();

      // Closes every client connection, then releases the listener. Stop
      // connects a throwaway client of its own to unblock the listening
      // thread, so it does not return instantly.
      server.Stop();

      Console.WriteLine(server.IsRunning());
    }
  }
}
