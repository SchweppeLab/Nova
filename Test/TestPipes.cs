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

using Nova.IPC.Pipes;

namespace TestNova
{
  // TEST-1: same-process connect/send/receive/disconnect coverage for the named-pipes IPC
  // layer (PipesServer/PipesClient/PipesConnection). Each test uses a GUID-derived server ID
  // since MSTestSettings.cs parallelizes at method level and named pipes are a shared,
  // process-wide namespace - a fixed ID would let parallel test runs collide.
  [TestClass]
  public sealed class TestPipes
  {
    [TestMethod]
    public void ConnectSendReceiveDisconnect()
    {
      string id = "NovaTestPipe_" + Guid.NewGuid().ToString("N");
      PipesServer server = new PipesServer(id);
      PipesClient client = new PipesClient(id);

      ManualResetEventSlim clientConnected = new ManualResetEventSlim(false);
      PipesConnection? serverSideConnection = null;
      server.ClientConnected += pc => { serverSideConnection = pc; clientConnected.Set(); };

      ManualResetEventSlim serverReceived = new ManualResetEventSlim(false);
      PipeMessage serverReceivedMessage = default;
      server.ClientMessage += (pc, msg) => { serverReceivedMessage = msg; serverReceived.Set(); };

      ManualResetEventSlim clientReceived = new ManualResetEventSlim(false);
      PipeMessage clientReceivedMessage = default;
      client.ServerMessage += (pc, msg) => { clientReceivedMessage = msg; clientReceived.Set(); };

      ManualResetEventSlim clientDisconnected = new ManualResetEventSlim(false);
      client.Disconnected += pc => clientDisconnected.Set();

      try
      {
        server.Start();
        client.Start();

        Assert.IsTrue(clientConnected.Wait(5000), "server never observed a client connection");
        client.WaitForConnection(5000);
        Assert.IsNotNull(serverSideConnection);

        PipeMessage toServer = new PipeMessage();
        toServer.EncodeString("hello server");
        client.Send(toServer);
        Assert.IsTrue(serverReceived.Wait(5000), "server never received the client's message");
        Assert.AreEqual("hello server", serverReceivedMessage.DecodeString());

        PipeMessage toClient = new PipeMessage();
        toClient.EncodeString("hello client");
        server.Send(toClient, serverSideConnection!.Name);
        Assert.IsTrue(clientReceived.Wait(5000), "client never received the server's message");
        Assert.AreEqual("hello client", clientReceivedMessage.DecodeString());

        client.AutoReconnect = false;
        client.Stop();
        Assert.IsTrue(clientDisconnected.Wait(5000), "client never raised its Disconnected event");
      }
      finally
      {
        client.AutoReconnect = false;
        client.Stop();
        server.Stop();
      }
    }
  }
}
