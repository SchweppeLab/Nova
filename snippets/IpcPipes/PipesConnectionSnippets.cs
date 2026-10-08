// Snippets published on the PipesConnection page.

using System;
using Nova.IPC.Pipes;

namespace NovaSnippets.IpcPipes
{
  internal static class PipesConnectionSnippets
  {
    internal static void UseTheConnectionAServerHandsYou()
    {
      PipesServer server = new PipesServer("NovaDemoServer");

      // The server passes each client's PipesConnection to its events.
      server.ClientConnected += connection =>
      {
        Console.WriteLine(connection.Name + " connected: " + connection.IsConnected);

        PipeMessage welcome = new PipeMessage();
        welcome.EncodeString("Welcome.");
        connection.Send(welcome);
      };

      server.ClientDisconnected += connection =>
        Console.WriteLine(connection.Name + " left.");

      server.Start();
    }
  }
}
