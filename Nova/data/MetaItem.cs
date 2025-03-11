using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  public class MetaItem
  {
    public string Name { get; set; } = string.Empty;
    public MetaClass Class { get; set; } = MetaClass.None;

  }
}
