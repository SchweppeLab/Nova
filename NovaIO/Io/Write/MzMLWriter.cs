using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using Nova.Data;
using ThermoFisher.CommonCore.Data.Business;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Nova.Io.Write
{

  internal class XMLElement
  {
    public readonly string Name;
    public string Contents;
    public List<Tuple<string, string>> Attributes = new List<Tuple<string,string>>();
    public List<XMLElement> Elements = new List<XMLElement>();

    public XMLElement(string name, string? contents=null) {
      Name = name;
      Contents = contents;
    }

    public void AddAttribute(string name, string value)
    {
      Attributes.Add(new Tuple<string, string>(name, value));
    }

    public void AddElement(XMLElement el)
    {
      Elements.Add(el);
    }

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
    //  foreach (XMLElement element in Elements)
    //  {
    //    element.Write(writer,fs);
    //  }
    //  if(Contents != null) writer.WriteString(Contents);
    //  writer.WriteEndElement();
    //}
  }


  public class MzMLWriter
  {
    private List<XMLElement> Run=new List<XMLElement>();
    XMLElement IndexedmzML = new XMLElement("indexedmzML");
    XMLElement IndexListOffset = new XMLElement("indexListOffset");
    XMLElement ? SpectrumList = null;
    XMLElement? IndexSpectrum = null;
    XMLElement? IndexChromatogram = null;

    private int SpecCount = 0;
    List<Tuple<string,long>> IndexListSpectrum = new List<Tuple<string,long>>();
    List<Tuple<string, long>> IndexListChromatogram = new List<Tuple<string, long>>();

    private FileStream? XmlFS;
    private XmlWriter? writer;

    private bool HasSpectrum = false;
    private bool HasChromatogram = false;

    public void AddRun(string id)
    {
      XMLElement element = new XMLElement("run");
      element.AddAttribute("id", id);

      SpectrumList = new XMLElement("spectrumList");
      element.AddElement(SpectrumList);
      Run.Add(element);
    }

    /// <summary>
    /// Converts a spectrum object into a spectrum element
    /// </summary>
    /// <param name="spec"></param>
    public void AddSpectrum(Spectrum spec)
    {
      //Check if a run has been established
      if (Run.Count == 0)
      {
        //throw some error
      }

      XMLElement element = new XMLElement("spectrum");
      element.AddAttribute("index",SpecCount.ToString());
      element.AddAttribute("id","scan="+spec.ScanNumber.ToString());
      element.AddAttribute("defaultArrayLength",spec.DataPoints.Length.ToString());

      if(spec.MsLevel == 1) element.AddElement(MakeCvParam("MS", "MS:1000579", "MS1 spectrum"));
      else element.AddElement(MakeCvParam("MS", "MS:1000580", "MSn spectrum"));
      element.AddElement(MakeCvParam("MS", "MS:1000511", "ms level", spec.MsLevel.ToString()));
      if(spec.Polarity) element.AddElement(MakeCvParam("MS","MS:1000130","positive scan"));
      else element.AddElement(MakeCvParam("MS", "MS:1000129", "negative scan"));
      if(spec.Centroid) element.AddElement(MakeCvParam("MS", "MS:1000127", "centroid spectrum"));
      element.AddElement(MakeCvParam("MS", "MS:1000504", "base peak m/z", spec.BasePeakMz.ToString(), "MS", "MS:1000040", "m/z"));
      element.AddElement(MakeCvParam("MS", "MS:1000505", "base peak intensity", spec.BasePeakIntensity.ToString(), "MS", "MS:1000131", "number of detector counts"));
      element.AddElement(MakeCvParam("MS", "MS:1000285", "total ion current", spec.TotalIonCurrent.ToString()));
      element.AddElement(MakeCvParam("MS", "MS:1000528", "lowest observed m/z", spec.LowestMz.ToString(), "MS", "MS:1000040", "m/z"));
      element.AddElement(MakeCvParam("MS", "MS:1000527", "highest observed m/z", spec.HighestMz.ToString(), "MS", "MS:1000040", "m/z"));

      element.AddElement(MakeScanList(spec));

      if (spec.Precursors.Count > 0)
      {
        element.AddElement(MakePrecursorList(spec));
      }

      element.AddElement(MakeBinaryDataArrayList(spec));

      SpectrumList.AddElement(element);
      SpecCount++;
      HasSpectrum = true;

    }

    private byte[] Compress(byte[] data)
    {
      using (MemoryStream stream = new MemoryStream())
      using (DeflaterOutputStream dos = new DeflaterOutputStream(stream))
      {
        dos.Write(data, 0, data.Length);
        dos.Finish();
        return stream.ToArray();
      }
    }

    private string EncodeBinaryDataArray(double[] arr)
    {
      byte[] bytes = new byte[arr.Length * 8];
      for (int i = 0; i < arr.Length; i++)
      {
        var mzBytes = BitConverter.GetBytes(arr[i]);
        mzBytes.CopyTo(bytes, i * 8);
      }
      byte[] zip = Compress(bytes);
      return Convert.ToBase64String(zip);
    }

    private string EncodeBinaryDataArray(float[] arr)
    {
      byte[] bytes = new byte[arr.Length * 4];
      for (int i = 0; i < arr.Length; i++)
      {
        var mzBytes = BitConverter.GetBytes(arr[i]);
        mzBytes.CopyTo(bytes, i * 4);
      }
      byte[] zip = Compress(bytes);
      return Convert.ToBase64String(zip);
    }

    private XMLElement MakeBinaryDataArrayList(Spectrum spec)
    {
      XMLElement binaryDataArrayList = new XMLElement("binaryDataArrayList");
      binaryDataArrayList.AddAttribute("count", "2");
      double[] mz = new double[spec.Count];
      float[] abun = new float[spec.Count];
      for (int i = 0; i < spec.Count; i++)
      {
        mz[i] = spec.DataPoints[i].Mz;
        abun[i] = (float)spec.DataPoints[i].Intensity;
      }
      string mzData = EncodeBinaryDataArray(mz);
      XMLElement binaryDataArrayMZ = new XMLElement("binaryDataArray");
      binaryDataArrayMZ.AddAttribute("encodedLength", mzData.Length.ToString());
      binaryDataArrayMZ.AddElement(MakeCvParam("MS", "MS:1000523", "64-bit float"));
      binaryDataArrayMZ.AddElement(MakeCvParam("MS", "MS:1000574", "zlib compression"));
      binaryDataArrayMZ.AddElement(MakeCvParam("MS", "MS:1000514", "m/z array", "", "MS", "MS:1000040", "m/z"));
      XMLElement binaryMZ = new XMLElement("binary", mzData);
      binaryDataArrayMZ.AddElement(binaryMZ);
      binaryDataArrayList.AddElement(binaryDataArrayMZ);

      mzData = EncodeBinaryDataArray(abun);
      XMLElement binaryDataArrayAbun = new XMLElement("binaryDataArray");
      binaryDataArrayAbun.AddAttribute("encodedLength", mzData.Length.ToString());
      binaryDataArrayAbun.AddElement(MakeCvParam("MS", "MS:1000521", "32-bit float"));
      binaryDataArrayAbun.AddElement(MakeCvParam("MS", "MS:1000574", "zlib compression"));
      binaryDataArrayAbun.AddElement(MakeCvParam("MS", "MS:1000515", "intensity array", "", "MS", "MS:1000131", "number of detector counts"));
      XMLElement binaryAbun = new XMLElement("binary", mzData);
      binaryDataArrayAbun.AddElement(binaryAbun);
      binaryDataArrayList.AddElement(binaryDataArrayAbun);

      return binaryDataArrayList;
    }

    private XMLElement MakeCvParam(string cvRef, string accession, string name, string value="", string? unitCvRef=null,string? unitAccession=null,string? unitName = null)
    {
      XMLElement element = new XMLElement("cvParam");
      element.AddAttribute("cvRef",cvRef);
      element.AddAttribute("accession",accession);
      element.AddAttribute("name",name);
      element.AddAttribute("value",value);
      if(unitCvRef != null) element.AddAttribute("unitCvRef",unitCvRef);
      if(unitAccession!=null) element.AddAttribute("unitAccession",unitAccession);
      if (unitName != null) element.AddAttribute("unitName", unitName);
      return element;
    }

    private XMLElement MakePrecursorList(Spectrum spec)
    {
      XMLElement precursorList = new XMLElement("precursorList");
      precursorList.AddAttribute("count", spec.Precursors.Count.ToString());
      foreach (PrecursorIon precursor in spec.Precursors)
      {
        XMLElement pre = new XMLElement("precursor");

        XMLElement isoWin = new XMLElement("isolationWindow");
        isoWin.AddElement(MakeCvParam("MS", "MS:1000827", "isolation window target m/z", precursor.IsolationMz.ToString(), "MS", "MS:1000040", "m/z"));
        isoWin.AddElement(MakeCvParam("MS", "MS:1000828", "isolation window lower offset", (precursor.IsolationWidth / 2).ToString(), "MS", "MS:1000040", "m/z"));
        isoWin.AddElement(MakeCvParam("MS", "MS:1000829", "isolation window upper offset", (precursor.IsolationWidth / 2).ToString(), "MS", "MS:1000040", "m/z"));
        pre.AddElement(isoWin);

        XMLElement selIonList = new XMLElement("selectedIonList");
        selIonList.AddAttribute("count", "1");
        XMLElement selIon = new XMLElement("selectedIon");
        if (precursor.MonoisotopicMz != 0) selIon.AddElement(MakeCvParam("MS", "MS:1000744", "selected ion m/z", precursor.MonoisotopicMz.ToString(), "MS", "MS:1000040", "m/z"));
        else selIon.AddElement(MakeCvParam("MS", "MS:1000744", "selected ion m/z", precursor.IsolationMz.ToString(), "MS", "MS:1000040", "m/z"));
        selIonList.AddElement(selIon);
        pre.AddElement(selIonList);

        XMLElement activation = new XMLElement("activation");
        switch (precursor.FramentationMethod)
        {
          case FramentationType.HCD:
            activation.AddElement(MakeCvParam("MS", "MS:1000422", "beam-type collision-induced dissociation"));
            activation.AddElement(MakeCvParam("MS", "MS:1000045", "collision energy", precursor.CollisionEnergy.ToString(), "UO", "UO:0000266", "electronvolt"));
            break;
          default:
            break;
        }
        pre.AddElement(activation);

        precursorList.AddElement(pre);
      }

      return precursorList;
    }


    private XMLElement MakeScanList(Spectrum spec)
    {
      XMLElement scanList = new XMLElement("scanList");
      scanList.AddAttribute("count", "1");

      XMLElement scanElement = new XMLElement("scan");
      scanElement.AddElement(MakeCvParam("MS", "MS:1000016", "scan start time", spec.RetentionTime.ToString(), "UO", "UO:0000031", "minute"));
      //scanElement.AddElement(MakeCvParam("MS", "MS:1000800", "mass resolving power", spec.Resolution.ToString()));
      scanElement.AddElement(MakeCvParam("MS", "MS:1000512", "filter string", spec.ScanFilter.ToString()));
      scanElement.AddElement(MakeCvParam("MS", "MS:1000927", "ion injection time", spec.IonInjectionTime.ToString(), "UO", "UO:0000028", "millisecond"));
      
      XMLElement scanWindowList = new XMLElement("scanWindowList");
      scanWindowList.AddAttribute("count", "1");
      
      XMLElement scanWindow = new XMLElement("scanWindow");
      scanWindow.AddElement(MakeCvParam("MS", "MS:1000501", "scan window lower limit", spec.StartMz.ToString(), "MS", "MS:1000040", "m/z"));
      scanWindow.AddElement(MakeCvParam("MS", "MS:1000500", "scan window upper limit", spec.EndMz.ToString(), "MS", "MS:1000040", "m/z"));
      
      scanWindowList.AddElement(scanWindow);
      scanElement.AddElement(scanWindowList);
      scanList.AddElement(scanElement);
      
      return scanList;
    }

    public bool Write(string filename)
    {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.NewLineChars = "\n";
      //settings.IndentChars = " ";  //for smaller files?
      //settings.IndentChars = "";   //or even smaller files?

      XMLElement MzML = new XMLElement("mzML");
      MzML.AddAttribute("id", filename);
      foreach (XMLElement el in Run)
      {
        MzML.AddElement(el);
      }
      IndexedmzML.AddElement(MzML);

      XMLElement IndexList = new XMLElement("indexList");
      if (HasSpectrum && HasChromatogram) IndexList.AddAttribute("count", "2");
      else IndexList.AddAttribute("count", "1");

      if (HasSpectrum)
      {
        IndexSpectrum = new XMLElement("index");
        IndexSpectrum.AddAttribute("name", "spectrum");
        IndexList.AddElement(IndexSpectrum);
      }
      if (HasChromatogram)
      {
        IndexChromatogram = new XMLElement("index");
        IndexChromatogram.AddAttribute("name", "chromatogram");
        IndexList.AddElement(IndexChromatogram);
      }
      IndexedmzML.AddElement(IndexList);
      IndexedmzML.AddElement(IndexListOffset);

      XmlFS = new FileStream(filename, FileMode.Create, FileAccess.Write);
      writer = XmlWriter.Create(XmlFS,settings);
      writer.WriteStartDocument();
      WriteElement(IndexedmzML);

      writer.WriteEndDocument();
      writer.Flush();
      writer.Close();
      return true;
    }

    private void WriteElement(XMLElement element)
    {
      //Special cases that are tracked for indexing purposes
      if (element.Name == "spectrum")
      {
        writer.Flush();
        XMLElement offset = new XMLElement("offset",(XmlFS.Position+1).ToString());
        offset.AddAttribute("idRef", element.Attributes[1].Item2);
        IndexSpectrum.AddElement(offset);
      } 
      else if(element.Name == "indexList")
      {
        writer.Flush();
        IndexListOffset.Contents=XmlFS.Position.ToString();
      }


      writer.WriteStartElement(element.Name);
      foreach (Tuple<string, string> attr in element.Attributes)
      {
        writer.WriteAttributeString(attr.Item1, attr.Item2);
      }
      foreach (XMLElement el in element.Elements)
      {
        WriteElement(el);
      }
      if (element.Contents != null) writer.WriteString(element.Contents);
      writer.WriteEndElement();
    }
  }
}
