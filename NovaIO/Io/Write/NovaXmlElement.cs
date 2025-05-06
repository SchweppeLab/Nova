using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Io.Write
{
  /// <summary>
  /// Similar to the XmlElement, I suppose. I didn't look closely. I just went for as
  /// simple as possible.
  /// </summary>
  internal class NovaXmlElement
  {
    public readonly string Name;
    public string? Contents;
    public List<Tuple<string, string>> Attributes = new List<Tuple<string, string>>();
    public List<NovaXmlElement> Elements = new List<NovaXmlElement>();

    public NovaXmlElement(string name, string? contents = null)
    {
      Name = name;
      Contents = contents;
    }

    public void AddAttribute(string name, string value)
    {
      Attributes.Add(new Tuple<string, string>(name, value));
    }

    public void AddElement(NovaXmlElement el)
    {
      Elements.Add(el);
    }

    //This was a cool way to have XML written out from inside the elements,
    //but ultimately I'm not going to use it in favor of managing element export elsewhere.
    //public void Write(XmlWriter writer, FileStream fs)
    //{
    //  if (Name == "spectrum")
    //  {
    //    writer.Flush();
    //    Console.WriteLine("Position: " + fs.Position.ToString());
    //  }
    //  writer.WriteStartElement(Name);
    //  foreach (Tuple<string, string> attr in Attributes)
    //  {
    //    writer.WriteAttributeString(attr.Item1, attr.Item2);
    //  }
    //  foreach (NovaXmlElement element in Elements)
    //  {
    //    element.Write(writer,fs);
    //  }
    //  if(Contents != null) writer.WriteString(Contents);
    //  writer.WriteEndElement();
    //}
  }
}
