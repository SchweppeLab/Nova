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

using Nova.Data;
using Nova.Io.Read;
using Nova.IPC.Pipes;

namespace ScanBroadcaster
{
  public partial class ScanBroadcaster : Form
  {
    private PipesServer? server;
    private int connections = 0;


    public ScanBroadcaster()
    {
      InitializeComponent();
      server = new PipesServer("NovaBroadcaster");
      server.ClientConnected += OnClientConnected;
      server.ClientDisconnected += OnClientDisconnected;
      server.Start();
    }

    private void OnClientConnected(PipesConnection connection)
    {
      richTextBox1.Text += ("Client Connected: " + connection.ID + Environment.NewLine);
      connections++;
      label2.Text = "Connections: " + connections.ToString();
    }

    private void OnClientDisconnected(PipesConnection connection)
    {
      richTextBox1.Text += "Client " + connection.ID + " disconnected" + Environment.NewLine;
      connections--;
      label2.Text = "Connections: " + connections.ToString();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        label1.Text = openFileDialog1.FileName;
      }
      if (label1.Text != "(no file selected)")
      {
        button2.Enabled = true;
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      button1.Enabled = false;
      button2.Enabled = false;
      richTextBox1.Text += "Broadcasting...";
      FileReader reader = new FileReader();
      Spectrum spec=reader.ReadSpectrum(label1.Text);
      while (spec.ScanNumber > 0)
      {
        PipeMessage pm = new PipeMessage();
        pm.MsgCode = '1';
        pm.MsgData = spec.Serialize();
        server?.Send(pm);
        spec = reader.ReadSpectrum();
      }
      richTextBox1.Text += "Done!" + Environment.NewLine;
      button1.Enabled = true;
      button2.Enabled=true;
    }
  }
}
