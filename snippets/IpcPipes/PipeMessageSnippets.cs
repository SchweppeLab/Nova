// Snippets published on the PipeMessage page.

using System;
using Nova.IPC.Pipes;

namespace NovaSnippets.IpcPipes
{
  internal static class PipeMessageSnippets
  {
    internal static void EncodeAndDecode()
    {
      PipeMessage text = new PipeMessage();
      text.EncodeString("Hello!");     // MsgCode is now '0'

      PipeMessage data = new PipeMessage();
      data.MsgCode = '1';              // a code the receiver knows how to interpret
      data.MsgData = new byte[] { 1, 2, 3 };

      foreach (PipeMessage message in new[] { text, data })
      {
        if (message.MsgCode == '0')
        {
          Console.WriteLine(message.DecodeString());
        }
        else
        {
          Console.WriteLine("Code " + message.MsgCode + ": " + message.MsgData.Length + " bytes");
        }
      }
    }
  }
}
