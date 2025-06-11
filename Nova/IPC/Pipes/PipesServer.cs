// Copyright 2025 Michael Hoopmann
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO.Pipes;


/* The Pipes library is a simple server/client interface for interprocess communication
 * that wraps the System.IO.Pipes classes. It largely cribs from:
 * https://github.com/acdvorak/named-pipe-wrapper/tree/master
 * which is under the MIT license.
 * 
 * Several changes were made, including modernizing the code, simplifying the code,
 * removing unneeded features, and adding new features.
 */
namespace Nova.IPC.Pipes
{

  /// <summary>
  /// The PipesServer class wraps the NamedPipeServerStream class, and manages
  /// multiple connections from clients. Messages can be received from clients,
  /// or broadcast to individual or all clients. The server establishes client
  /// identities through a handshake, and names those clients for the duration
  /// of their connection.
  /// </summary>
  public class PipesServer
  {
    /// <summary>
    /// The identity of the server, which clients will use to locate it.
    /// </summary>
    private readonly string ServerID;

    /// <summary>
    /// An iterator used to name and identify each pipe as clients connect
    /// </summary>
    private int NextPipeID;

    /// <summary>
    /// Active connections to clients.
    /// </summary>
    private readonly List<PipesConnection> Connections = new List<PipesConnection>();

    /// <summary>
    /// Indicate when client has connected.
    /// </summary>
    public event PipeConnectionEvent ClientConnected;

    /// <summary>
    /// Indicate when client has disconnected.
    /// </summary>
    public event PipeConnectionEvent ClientDisconnected;

    /// <summary>
    /// Indicate reception of client message.
    /// </summary>
    public event PipeConnectionMessageEvent ClientMessage;

    /// <summary>
    /// Indicate error has occurred.
    /// </summary>
    public event PipeExceptionEventHandler Error;

    /// <summary>
    /// Indicates server is started and running.
    /// </summary>
    private volatile bool isRunning;

    /// <summary>
    /// Keeps server actively running until a stop is requested.
    /// </summary>
    private volatile bool keepRunning;

    /// <summary>
    /// Server constructor.
    /// </summary>
    /// <param name="sID">Identity of the server.</param>
    public PipesServer(string sID) 
    { 
      ServerID = sID;
    }

    /// <summary>
    /// Iterates a counter to provide a unqiue identifier to track client connections.
    /// </summary>
    /// <returns>String containing a unique client identifier.</returns>
    private string GetNextConnectionPipeName()
    {
      return string.Format("{0}_{1}", ServerID, ++NextPipeID);
    }

    /// <summary>
    /// Indicates if the server has been started and is currently running.
    /// </summary>
    /// <returns>True if started and running, false otherwise</returns>
    public bool IsRunning() {  return isRunning; }

    /// <summary>
    /// Raise that a client connected to the server.
    /// </summary>
    /// <param name="connection">Information about the connection</param>
    private void OnClientConnected(PipesConnection connection)
    {
      ClientConnected?.Invoke(connection);
    }

    /// <summary>
    /// Called when receiving client disconnection notificaiton. Removes client from the connection list.
    /// Raise that the client has disconnected.
    /// </summary>
    /// <param name="connection">Information about the connection.</param>
    private void OnClientDisconnected(PipesConnection connection)
    {
      if (connection == null) return;
      lock (Connections)
      {
        Connections.Remove(connection);
      }
      ClientDisconnected?.Invoke(connection);
    }

    /// <summary>
    /// Called when receiving a connection error.
    /// </summary>
    /// <param name="connection">Information about the connection.</param>
    /// <param name="ex">The error that occurred.</param>
    private void OnConnectionError(PipesConnection connection, Exception ex)
    {
      OnError(ex);
    }

    /// <summary>
    /// Called when receiving an error and raise that an error was received.
    /// </summary>
    /// <param name="ex"></param>
    private void OnError(Exception ex)
    {
      Error?.Invoke(ex);
    }

    /// <summary>
    /// Called when receiving a client message and raises that a message was received.
    /// </summary>
    /// <param name="connection">Information about the client connection.</param>
    /// <param name="msg">The message received in a PipeMessage structure.</param>
    private void OnReceiveClientMessage(PipesConnection connection, PipeMessage msg)
    {
      ClientMessage?.Invoke(connection, msg);
    }

    /// <summary>
    /// Continuously listens for new clients until the server has been told to Stop.
    /// </summary>
    private void Listen()
    {
      isRunning = true;
      while (keepRunning)
      {
        WaitForConnection();
      }
      isRunning = false;
    }

    /// <summary>
    /// Sends a message to all clients connected to the server.
    /// </summary>
    /// <param name="message">The message wrapped in a PipeMessage structure.</param>
    public void Send(PipeMessage message)
    {
      lock (Connections)
      {
        foreach (var client in Connections)
        {
          client.Send(message);
        }
      }
    }

    /// <summary>
    /// Sends a message to a specific client from the list of connections.
    /// </summary>
    /// <param name="message">The message wrapped in a PipeMessage structure.</param>
    /// <param name="clientID">The identifier of the client.</param>
    public void Send(PipeMessage message, string clientID)
    {
      lock (Connections)
      {
        foreach (var client in Connections)
        {
          if (client.Name == clientID) client.Send(message);
        }
      }
    }

    /// <summary>
    /// Starts the server.
    /// </summary>
    public void Start()
    {
      keepRunning = true;
      var task = new TaskQueue();
      task.Error += OnError;
      task.DoTask(Listen);
    }

    /// <summary>
    /// Stop the server, sending disconnect events to each client, and shutting down the current
    /// active listener.
    /// </summary>
    public void Stop()
    {
      keepRunning = false;

      lock (Connections)
      {
        foreach (var client in Connections.ToArray())
        {
          client.Close();
        }
      }

      // If background thread is still listening for a client to connect,
      // initiate a dummy connection that will allow the thread to exit.
      //dummy connection will use the local server name.
      PipesClient dummyClient = new PipesClient(ServerID);
      dummyClient.Start();
      dummyClient.WaitForConnection(10);
      dummyClient.Stop();
      dummyClient.WaitForDisconnection(10);
    }

    /// <summary>
    /// Waits for a client to connect to the server. When this occurs, perform a handshake,
    /// and add the client connection to the list of active pipes.
    /// </summary>
    private void WaitForConnection()
    {
      NamedPipeServerStream server = null;
      PipesConnection Connection = null;

      string connectionPipeName = GetNextConnectionPipeName();

      try
      {
        // Send the client the name of the data pipe to use
        server = new NamedPipeServerStream(ServerID, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0);
        server.WaitForConnection();
        PipeMessage pm = new PipeMessage();
        pm.EncodeString(connectionPipeName);
        PipeIO.Write(server, pm);
        server.WaitForPipeDrain();
        server.Close();

        // Wait for the client to connect to the data pipe
        server = new NamedPipeServerStream(connectionPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0);
        server.WaitForConnection();

        // Add the client's connection to the list of connections
        Connection = PipeConnectionFactory.CreatePipeConnection(server);
        Connection.ReceiveMessage += OnReceiveClientMessage;
        Connection.Disconnected += OnClientDisconnected;
        Connection.Error += OnConnectionError;
        Connection.Open();

        lock (Connections)
        {
          Connections.Add(Connection);
        }

        OnClientConnected(Connection);
      }
      // Catch the IOException that is raised if the pipe is broken or disconnected.
      catch (Exception e)
      {
        Console.Error.WriteLine("Named pipe is broken or disconnected: {0}", e);
        server?.Close();
        OnClientDisconnected(Connection);
      }
    }
  }
}
