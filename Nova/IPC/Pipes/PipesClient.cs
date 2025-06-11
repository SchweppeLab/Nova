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
using System.IO.Pipes;
using System.Threading;

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
  /// The PipesClient class wraps around the NamedPipesClientStream class. Each PipesClient
  /// establishes a single pipe between itself and a PipesSever, allowing messages to be both
  /// received and sent. If the server goes down or is not available, the client reverts to a
  /// listening state, and reconnects when the server comes back online.
  /// </summary>
  public class PipesClient
  {

    /// <summary>
    /// Indicates if the client should attempt to reconnect upon a broken pipe.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// The identifier for the client, matched to the server's list of connections.
    /// </summary>
    private readonly string PipeID;

    /// <summary>
    /// The identifier of the server
    /// </summary>
    private readonly string ServerID;

    /// <summary>
    /// The pipe connection details when connected to the server.
    /// </summary>
    private PipesConnection Connection;

    /// <summary>
    /// Indicate disconnection event
    /// </summary>
    public event PipeConnectionEvent Disconnected;

    /// <summary>
    /// Indicate message received from server
    /// </summary>
    public event PipeConnectionMessageEvent ServerMessage;

    /// <summary>
    /// Indicate error occurred.
    /// </summary>
    public event PipeExceptionEventHandler Error;


    private readonly AutoResetEvent connected = new AutoResetEvent(false);
    private readonly AutoResetEvent disconnected = new AutoResetEvent(false);

    /// <summary>
    /// Indicate client should not reconnect after a broken pipe.
    /// </summary>
    private volatile bool stayClosed;

    /// <summary>
    /// PipesClient constructor.
    /// </summary>
    /// <param name="pID">Should match the ID of the server.</param>
    /// <param name="sID">Default value is "." indicating local server. Otherwise provide network ID.</param>
    public PipesClient(string pID, string sID = ".")
    {
      PipeID = pID;
      ServerID = sID;
    }

    /// <summary>
    /// Listens and then connects to PipesServer.
    /// </summary>
    private void Listen()
    {
      // Perform a handshake and receive a pipe identifier from the server
      NamedPipeClientStream client = new NamedPipeClientStream(ServerID, PipeID, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
      client.Connect();
      string pID = PipeIO.Read(client).DecodeString();
      client.Close();

      // Reconnect using the pipe identifier
      client = new NamedPipeClientStream(ServerID, pID, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
      client.Connect();

      // Create a Connection object for the data pipe
      Connection = PipeConnectionFactory.CreatePipeConnection(client);
      Connection.Disconnected += OnDisconnected;
      Connection.ReceiveMessage += OnReceiveMessage;
      Connection.Error += OnConnectionError;
      Connection.Open();

      connected.Set();
    }

    /// <summary>
    /// Called when receiving a connection error.
    /// </summary>
    /// <param name="pc">Information about the connection.</param>
    /// <param name="ex">The error that occurred.</param>
    private void OnConnectionError(PipesConnection pc, Exception ex)
    {
      OnError(ex);
    }

    /// <summary>
    /// Called and raised when receiving a disconnect event. Will attempt reconnect
    /// unless otherwise instructed not to do so (e.g., when explicitly closed).
    /// </summary>
    /// <param name="pc"></param>
    private void OnDisconnected(PipesConnection pc)
    {
      Disconnected?.Invoke(pc);
      disconnected.Set();

      // Reconnect
      if (AutoReconnect && !stayClosed) Start();
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
    /// Called when receiving a server message and raises that a message was received.
    /// </summary>
    /// <param name="pc">Information about the client connection.</param>
    /// <param name="msg">The message received in a PipeMessage structure.</param>
    private void OnReceiveMessage(PipesConnection pc, PipeMessage msg)
    {
      ServerMessage?.Invoke(pc, msg);
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The message wrapped in a PipeMessage structure.</param>
    public void Send(PipeMessage message)
    {
      Connection?.Send(message);
    }

    /// <summary>
    /// Starts the client.
    /// </summary>
    public void Start()
    {
      stayClosed = false;
      var task = new TaskQueue();
      task.Error += OnError;
      task.DoTask(Listen);
    }

    /// <summary>
    /// Stop the client, and do not attempt to reconnect.
    /// </summary>
    public void Stop()
    {
      stayClosed = true;
      Connection?.Close();
    }

    public void WaitForConnection(int ms)
    {
      connected.WaitOne(ms);
    }

    public void WaitForDisconnection(int ms)
    {
      disconnected.WaitOne(ms);
    }
  }

  public delegate void PipeExceptionEventHandler(Exception exception);
}
