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
using Nova.IPC.Pipes;

namespace ScanReceiver
{
  public partial class ScanReceiver : Form
  {
    PipesClient? pipeClient = null;
    Spectrum? spec;
    bool connected = false;

    int ms1 = 0;
    int ms2 = 0;
    int ms3 = 0;
    int peaks = 0;

    public ScanReceiver()
    {
      InitializeComponent();
      UpdateCount();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      pipeClient = new PipesClient("NovaBroadcaster");
      pipeClient.ServerMessage += OnServerMessage;
      pipeClient.Error += OnError;
      pipeClient.Start();
      button1.Enabled = false;
    }

    private void OnError(Exception exception)
    {
      Console.Error.WriteLine("ERROR: {0}", exception);
    }

    private void OnServerMessage(PipesConnection connection, PipeMessage message)
    {
      switch (message.MsgCode)
      {
        case '0':
          connected = true;
          break;
        case '1':
          spec = new Spectrum();
          spec.Deserialize(message.MsgData);
          if (spec.MsLevel == 1) ms1++;
          else if(spec.MsLevel == 2) ms2++;
          else if(spec.MsLevel == 3) ms3++;
          peaks += spec.Count;
          UpdateCount();
          break;
        default:
          Console.WriteLine("Server sent unrecognized message code: {0}", message.MsgCode);
          break;
      }
    }

    private void UpdateCount()
    {
      label1.Text="MS1 scans received: " + ms1 + Environment.NewLine;
      label1.Text += "MS2 scans received: " + ms2 + Environment.NewLine;
      label1.Text += "MS3 scans received: " + ms3 + Environment.NewLine;
      label1.Text += "Data points received: " + peaks + Environment.NewLine;
    }
  }
}
