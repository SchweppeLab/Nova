using System;
using System.Collections.Concurrent;
using System.IO;
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
  /// A basic structure for packaging messages that are passed through a pipe stream.
  /// Data should be packaged into a byte[] array, presumably by a serialize function.
  /// Each PipeMessage contains a single character message code (MsgCode), used to
  /// indicate the type of content in the byte[] array. Only a char value of '0' is
  /// reserved for encoding and decoding strings. Developers can use the other 254
  /// characters for their own purposes.
  /// </summary>
  public struct PipeMessage
  {
    public char MsgCode;
    public byte[] MsgData;

    /// <summary>
    /// Decoder method to convert the byte[] MsgData array to a string. Should only be called
    /// if the array originated from a string.
    /// </summary>
    /// <returns>The MsgData decoded to a string.</returns>
    public string DecodeString()
    {
      using (MemoryStream m = new MemoryStream(MsgData))
      using (BinaryReader reader = new BinaryReader(m, System.Text.Encoding.Unicode))
      {
        return reader.ReadString();
      }
    }

    /// <summary>
    /// Encoder method to convert any string into the byte[] MsgData. Note that
    /// the MsgCode is automatically set to '0', which is reserved for string data.
    /// </summary>
    /// <param name="str">The string to be encoded</param>
    public void EncodeString(string str)
    {
      using (MemoryStream m = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(m, System.Text.Encoding.Unicode))
      {
        writer.Write(str);
        MsgData = m.ToArray();
      }
      MsgCode = '0';
    }
  }
  
  /// <summary>
  /// Class for managing connections between Server and Client
  /// </summary>
  public class PipesConnection
  {
    /// <summary>
    /// Numeric identifier
    /// </summary>
    public readonly int ID;

    /// <summary>
    /// Indicates if stream is connected
    /// </summary>
    public bool IsConnected { get { return TheStream.IsConnected; } }

    /// <summary>
    /// String identifier for the client in this connected stream.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The stream between server and client
    /// </summary>
    public PipeStream TheStream { get; private set; }

    /// <summary>
    /// A queue for messages to be sent to the server.
    /// </summary>
    private readonly BlockingCollection<PipeMessage> MsgQueue = new BlockingCollection<PipeMessage>();

    private readonly AutoResetEvent writeSignal = new AutoResetEvent(false);

    /// <summary>
    /// Used internally when closing a connection. See OnSuccess()
    /// </summary>
    private bool bSuccess;

    /// <summary>
    /// Indicates the stream has been disconnected.
    /// </summary>
    public event PipeConnectionEvent Disconnected;

    /// <summary>
    /// Indicates that the stream received a message from the server.
    /// </summary>
    public event PipeConnectionMessageEvent ReceiveMessage;

    /// <summary>
    /// Indicates an error.
    /// </summary>
    public event PipeConnectionExceptionEvent Error;

    /// <summary>
    /// Constructor that passes the stream from the server, plus any identifiers
    /// </summary>
    /// <param name="id">An iterative connection ID</param>
    /// <param name="name">The name of the client</param>
    /// <param name="serverStream">The PipeStream from the server</param>
    public PipesConnection(int id, string name, PipeStream serverStream) 
    {
      ID = id;
      Name = name;
      TheStream = serverStream;
    }

    /// <summary>
    /// Closes the stream.
    /// </summary>
    public void Close()
    {
      TheStream.Close();
      writeSignal.Set();
    }

    /// <summary>
    /// Raised when there is an error.
    /// </summary>
    /// <param name="ex">The error.</param>
    private void OnError(Exception ex)
    {
      Error?.Invoke(this, ex);
    }

    /// <summary>
    /// Called when the connection task completes. This will then raise the disconnect event.
    /// Because a connection has two tasks, this method will be called twice. Therefore use a
    /// flag to indicate if it was already called and prevent raising the disconnect event twice.
    /// </summary>
    private void OnSuccess()
    {
      // Only notify observers once
      if (bSuccess)  return;
      bSuccess = true;

      Disconnected?.Invoke(this);
    }

    /// <summary>
    /// Opens the connection for reading and writing.
    /// </summary>
    public void Open()
    {
      var readTask= new TaskQueue();
      readTask.Succeeded += OnSuccess;
      readTask.Error += OnError;
      readTask.DoTask(Read);

      var writeTask = new TaskQueue();
      writeTask.Succeeded += OnSuccess;
      writeTask.Error += OnError;
      writeTask.DoTask(Write);
    }

    /// <summary>
    /// Continually reads messages while connected and raises the event.
    /// </summary>
    private void Read()
    {
      while (IsConnected && TheStream.CanRead)
      {
        try
        {
          PipeMessage pm = PipeIO.Read(TheStream);

          //Not sure if wanting to intercept null messages here
          //if (obj == null)
          //{
          //  Close();
          //  return;
          //}
          ReceiveMessage?.Invoke(this, pm);
        }
        catch
        {
          //we must igonre exception, otherwise, the namepipe wrapper will stop work.
        }
      }
    }
    
    /// <summary>
    /// Puts a PipeMessage in the queue to be sent.
    /// </summary>
    /// <param name="message">The PipeMessage</param>
    public void Send(PipeMessage message)
    {
      MsgQueue.Add(message);
      writeSignal.Set();
    }

    /// <summary>
    /// Sends messages from the message queue to the server.
    /// </summary>
    private void Write()
    {
      while (IsConnected && TheStream.CanWrite)
      {
        try
        {
          {
            PipeMessage pm = MsgQueue.Take();
            PipeIO.Write(TheStream, pm);
            TheStream.WaitForPipeDrain();
          }
        }
        catch
        {
          //we must ignore exception, otherwise, the namepipe wrapper will stop work.
        }
      }
    }

  }

  public delegate void PipeConnectionEvent(PipesConnection pc);
  public delegate void PipeConnectionMessageEvent(PipesConnection pc, PipeMessage message);
  public delegate void PipeConnectionExceptionEvent(PipesConnection pc, Exception ex);


  /// <summary>
  /// Ugh. A whole factory, simply to increment a global variable.
  /// </summary>
  static class PipeConnectionFactory
  {
    /// <summary>
    /// An iterating integer ID.
    /// </summary>
    private static int connectID;

    /// <summary>
    /// Creates a new pipe connection containing the stream (from the server) between the server and client.
    /// </summary>
    /// <param name="stream">The PipeStream</param>
    /// <returns>PipeConnection object</returns>
    public static PipesConnection CreatePipeConnection(PipeStream stream)
    {
      return new PipesConnection(++connectID,"Client"+connectID,stream);
    }
  }

  /// <summary>
  /// PipIO contains methods for transferring PipeMessage structures between servers and clients.
  /// </summary>
  static class PipeIO
  {

    /// <summary>
    /// Reads incoming data from a stream and converts it to a PipeMessage
    /// </summary>
    /// <param name="stream">The PipeStream</param>
    /// <returns>A PipeMessage object</returns>
    public static PipeMessage Read(PipeStream stream)
    {
      PipeMessage pm = new PipeMessage();
      pm.MsgCode = (char)stream.ReadByte();
      int len = stream.ReadByte() << 24;
      len += stream.ReadByte() << 16;
      len += stream.ReadByte() << 8;
      len += stream.ReadByte();
      pm.MsgData = new byte[len];
      stream.Read(pm.MsgData, 0, len);
      return pm;
    }

    /// <summary>
    /// Writes a PipeMessage to data that is sent across a stream.
    /// </summary>
    /// <param name="stream">The PipeStream</param>
    /// <param name="pm">The PipeMessage</param>
    public static void Write(PipeStream stream, PipeMessage pm)
    {
      stream.WriteByte((byte)pm.MsgCode);

      int len = pm.MsgData.Length;
      stream.WriteByte((byte)(len >> 24));
      stream.WriteByte((byte)(len >> 16));
      stream.WriteByte((byte)(len >> 8));
      stream.WriteByte((byte)len);
      stream.Write(pm.MsgData, 0, len);
      stream.Flush();
    }
  }
}
