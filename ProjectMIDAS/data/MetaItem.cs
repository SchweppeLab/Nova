using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMIDAS.data
{
  public class MetaItem
  {
    public MetaItem(string name, object value, MetaClass @class)
    {
      Name = name;
      Value = value;
      Class = @class;
    }

    public MetaItem(string name, object value, List<string> aliases, MetaClass @class)
    {
      Name = name;
      Value = value;
      Aliases = aliases;
      Class = @class;
    }

    public MetaItem(string name, object value, string[] aliases, MetaClass @class)
    {
      Name = name;
      Value = value;
      Aliases = aliases.ToList();
      Class = @class;
    }

    public string Name { get; set; } = "";

    public object Value { get; set; } = null;

    public List<string> Aliases { get; set; } = new List<string>();

    public MetaClass Class { get; set; } = MetaClass.None;

    public bool TryGetValue(string name, out object value)
    {
      if (name.Trim() == Name)
      {
        value = Value;
        return true;
      }
      else if (Aliases.Contains(name))
      {
        value = Value;
        return true;
      }
      value = null;
      return false;
    }

    public bool TrySetValue(string name, object value)
    {
      if (name.Trim() == Name.Trim())
      {
        Value = value;
        return true;
      }
      else if (Aliases.Contains(name.Trim()))
      {
        Value = value;
        return true;
      }
      return false;
    }
  }
}
