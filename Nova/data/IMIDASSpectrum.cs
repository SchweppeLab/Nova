using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  public interface IMIDASSpectrum : IDisposable
  {
    Dictionary<string,string> MetaData { get; }

    void Deserialize(byte[] data);
    byte[] Serialize();
  }
}
