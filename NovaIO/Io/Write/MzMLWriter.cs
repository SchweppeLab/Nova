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
using System.Xml.Schema;
using System.Collections.Specialized;
using System.Xml.Xsl;
using System.Reflection;
using ThermoFisher.CommonCore.Data;

namespace Nova.Io.Write
{

  public class MzMLWriter
  {
    //I'm not sure if I've ever seen more than one run in an mzML file...
    private List<NovaXmlElement> Run=new List<NovaXmlElement>();
    NovaXmlElement MzML = new NovaXmlElement("mzML");
    NovaXmlElement IndexedmzML = new NovaXmlElement("indexedmzML");
    NovaXmlElement IndexListOffset = new NovaXmlElement("indexListOffset");
    NovaXmlElement FileDescription = new NovaXmlElement("fileDescription");
    NovaXmlElement FileContent = new NovaXmlElement("fileContent");
    NovaXmlElement SoftwareList = new NovaXmlElement("softwareList");
    NovaXmlElement InstrumentConfigurationList = new NovaXmlElement("instrumentConfigurationList");
    NovaXmlElement DataProcessingList = new NovaXmlElement("dataProcessingList");

    NovaXmlElement? SourceFileList = null;
    NovaXmlElement? SpectrumList = null;
    NovaXmlElement? IndexSpectrum = null;
    NovaXmlElement? IndexChromatogram = null;
    NovaXmlElement? InstrumentConfiguration = null;
    NovaXmlElement? DataProcessing = null;

    private int SpecCount = 0;
    List<Tuple<string,long>> IndexListSpectrum = new List<Tuple<string,long>>();
    List<Tuple<string, long>> IndexListChromatogram = new List<Tuple<string, long>>();

    private FileStream? XmlFS;
    private XmlWriter? writer;

    private bool HasSpectrum = false;
    private bool HasChromatogram = false;

    public MzMLWriter()
    {
      InitializeXML();
    }

    public void AddDataProcessing(string id)
    {
      DataProcessing = new NovaXmlElement("dataProcessing");
      DataProcessing.AddAttribute("id", id);
      DataProcessingList.AddElement(DataProcessing);
    }

    public void AddFileDescription(string fileName)
    {
      if (SourceFileList == null) SourceFileList = new NovaXmlElement("sourceFileList");
      NovaXmlElement soucreFile = new NovaXmlElement("sourceFile");
    }
    public void AddRun(string id,string instConf)
    {
      NovaXmlElement element = new NovaXmlElement("run");
      element.AddAttribute("id", id);
      element.AddAttribute("defaultInstrumentConfigurationRef",instConf);

      //close out the old spectrum list
      if (SpectrumList != null)
      {
        SpectrumList.AddAttribute("count",SpectrumList.Elements.Count.ToString());
        SpectrumList.AddAttribute("defaultDataProcessingRef", "Nova_mzML");
      }

      SpectrumList = new NovaXmlElement("spectrumList");
      element.AddElement(SpectrumList);
      Run.Add(element);
    }

    public void AddInstrumentConfiguration(string id, string? refID)
    {
      InstrumentConfiguration = new NovaXmlElement("instrumentConfiguration");
      InstrumentConfiguration.AddAttribute("id", id);
      InstrumentConfigurationList.AddElement(InstrumentConfiguration);
    }

    public void AddProcessingMethod(string softwareRef)
    {
      if (DataProcessing == null) return;
      NovaXmlElement processingMethod = new NovaXmlElement("processingMethod");
      processingMethod.AddAttribute("order", DataProcessing.Elements.Count.ToString());
      processingMethod.AddAttribute("softwareRef", softwareRef);
      DataProcessing.AddElement(processingMethod);
    }

    public void AddSoftware(string id, string version)
    {
      NovaXmlElement software = new NovaXmlElement("software");
      software.AddAttribute("id", id);
      software.AddAttribute("version", version);
      if (id == "Xcalibur")
      {
        software.AddElement(MakeCvParam("MS", "MS:1000532", "Xcalibur"));
      } 
      else if (id == "pwiz")
      {
        software.AddElement(MakeCvParam("MS", "MS:1000615", "ProteoWizard software"));
      }
      SoftwareList.AddElement(software);
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

      NovaXmlElement element = new NovaXmlElement("spectrum");
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

    private void InitializeXML()
    {
      MzML.AddAttribute("version", "1.1.0");
      NovaXmlElement cvList = new NovaXmlElement("cvList");
      cvList.AddAttribute("count", "2");
      NovaXmlElement cv = new NovaXmlElement("cv");
      cv.AddAttribute("id", "MS");
      cv.AddAttribute("fullName", "Proteomics Standards Initiative Mass Spectrometry Ontology");
      cv.AddAttribute("version", "4.1.182");
      cv.AddAttribute("URI", "https://raw.githubusercontent.com/HUPO-PSI/psi-ms-CV/master/psi-ms.obo");
      cvList.AddElement(cv);
      cv = new NovaXmlElement("cv");
      cv.AddAttribute("id", "UO");
      cv.AddAttribute("fullName", "Unit Ontology");
      cv.AddAttribute("version", "09:04:2014");
      cv.AddAttribute("URI", "https://raw.githubusercontent.com/bio-ontology-research-group/unit-ontology/master/unit.obo");
      cvList.AddElement(cv);
      MzML.AddElement(cvList);
      MzML.AddElement(FileDescription);
      MzML.AddElement(SoftwareList);
      MzML.AddElement(InstrumentConfigurationList);

      MzML.AddElement(DataProcessingList);

      FileDescription.AddElement(FileContent);
    }

    private NovaXmlElement MakeBinaryDataArrayList(Spectrum spec)
    {
      NovaXmlElement binaryDataArrayList = new NovaXmlElement("binaryDataArrayList");
      binaryDataArrayList.AddAttribute("count", "2");
      double[] mz = new double[spec.Count];
      float[] abun = new float[spec.Count];
      for (int i = 0; i < spec.Count; i++)
      {
        mz[i] = spec.DataPoints[i].Mz;
        abun[i] = (float)spec.DataPoints[i].Intensity;
      }
      string mzData = EncodeBinaryDataArray(mz);
      NovaXmlElement binaryDataArrayMZ = new NovaXmlElement("binaryDataArray");
      binaryDataArrayMZ.AddAttribute("encodedLength", mzData.Length.ToString());
      binaryDataArrayMZ.AddElement(MakeCvParam("MS", "MS:1000523", "64-bit float"));
      binaryDataArrayMZ.AddElement(MakeCvParam("MS", "MS:1000574", "zlib compression"));
      binaryDataArrayMZ.AddElement(MakeCvParam("MS", "MS:1000514", "m/z array", "", "MS", "MS:1000040", "m/z"));
      NovaXmlElement binaryMZ = new NovaXmlElement("binary", mzData);
      binaryDataArrayMZ.AddElement(binaryMZ);
      binaryDataArrayList.AddElement(binaryDataArrayMZ);

      mzData = EncodeBinaryDataArray(abun);
      NovaXmlElement binaryDataArrayAbun = new NovaXmlElement("binaryDataArray");
      binaryDataArrayAbun.AddAttribute("encodedLength", mzData.Length.ToString());
      binaryDataArrayAbun.AddElement(MakeCvParam("MS", "MS:1000521", "32-bit float"));
      binaryDataArrayAbun.AddElement(MakeCvParam("MS", "MS:1000574", "zlib compression"));
      binaryDataArrayAbun.AddElement(MakeCvParam("MS", "MS:1000515", "intensity array", "", "MS", "MS:1000131", "number of detector counts"));
      NovaXmlElement binaryAbun = new NovaXmlElement("binary", mzData);
      binaryDataArrayAbun.AddElement(binaryAbun);
      binaryDataArrayList.AddElement(binaryDataArrayAbun);

      return binaryDataArrayList;
    }

    private NovaXmlElement MakeCvParam(string cvRef, string accession, string name, string value="", string? unitCvRef=null,string? unitAccession=null,string? unitName = null)
    {
      NovaXmlElement element = new NovaXmlElement("cvParam");
      element.AddAttribute("cvRef",cvRef);
      element.AddAttribute("accession",accession);
      element.AddAttribute("name",name);
      element.AddAttribute("value",value);
      if(unitCvRef != null) element.AddAttribute("unitCvRef",unitCvRef);
      if(unitAccession!=null) element.AddAttribute("unitAccession",unitAccession);
      if (unitName != null) element.AddAttribute("unitName", unitName);
      return element;
    }

    private NovaXmlElement MakePrecursorList(Spectrum spec)
    {
      NovaXmlElement precursorList = new NovaXmlElement("precursorList");
      precursorList.AddAttribute("count", spec.Precursors.Count.ToString());
      foreach (PrecursorIon precursor in spec.Precursors)
      {
        NovaXmlElement pre = new NovaXmlElement("precursor");

        NovaXmlElement isoWin = new NovaXmlElement("isolationWindow");
        isoWin.AddElement(MakeCvParam("MS", "MS:1000827", "isolation window target m/z", precursor.IsolationMz.ToString(), "MS", "MS:1000040", "m/z"));
        isoWin.AddElement(MakeCvParam("MS", "MS:1000828", "isolation window lower offset", (precursor.IsolationWidth / 2).ToString(), "MS", "MS:1000040", "m/z"));
        isoWin.AddElement(MakeCvParam("MS", "MS:1000829", "isolation window upper offset", (precursor.IsolationWidth / 2).ToString(), "MS", "MS:1000040", "m/z"));
        pre.AddElement(isoWin);

        NovaXmlElement selIonList = new NovaXmlElement("selectedIonList");
        selIonList.AddAttribute("count", "1");
        NovaXmlElement selIon = new NovaXmlElement("selectedIon");
        if (precursor.MonoisotopicMz != 0) selIon.AddElement(MakeCvParam("MS", "MS:1000744", "selected ion m/z", precursor.MonoisotopicMz.ToString(), "MS", "MS:1000040", "m/z"));
        else selIon.AddElement(MakeCvParam("MS", "MS:1000744", "selected ion m/z", precursor.IsolationMz.ToString(), "MS", "MS:1000040", "m/z"));
        selIonList.AddElement(selIon);
        pre.AddElement(selIonList);

        NovaXmlElement activation = new NovaXmlElement("activation");
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


    private NovaXmlElement MakeScanList(Spectrum spec)
    {
      NovaXmlElement scanList = new NovaXmlElement("scanList");
      scanList.AddAttribute("count", "1");

      NovaXmlElement scanElement = new NovaXmlElement("scan");
      scanElement.AddElement(MakeCvParam("MS", "MS:1000016", "scan start time", spec.RetentionTime.ToString(), "UO", "UO:0000031", "minute"));
      //scanElement.AddElement(MakeCvParam("MS", "MS:1000800", "mass resolving power", spec.Resolution.ToString()));
      if(!spec.ScanFilter.IsNullOrEmpty()) scanElement.AddElement(MakeCvParam("MS", "MS:1000512", "filter string", spec.ScanFilter.ToString()));
      if(spec.IonInjectionTime>0) scanElement.AddElement(MakeCvParam("MS", "MS:1000927", "ion injection time", spec.IonInjectionTime.ToString(), "UO", "UO:0000028", "millisecond"));
      
      NovaXmlElement scanWindowList = new NovaXmlElement("scanWindowList");
      scanWindowList.AddAttribute("count", "1");
      
      NovaXmlElement scanWindow = new NovaXmlElement("scanWindow");
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

      //Add Nova to the software list
      AddSoftware("Nova", Assembly.GetExecutingAssembly().GetName().Version.ToString());

      //Add Nova to the data processing list
      AddDataProcessing("Nova_mzML");
      AddProcessingMethod("Nova");

      //Set dynamic attributes
      SoftwareList.AddAttribute("count",SoftwareList.Elements.Count.ToString());
      InstrumentConfigurationList.AddAttribute("count", InstrumentConfigurationList.Elements.Count.ToString());
      DataProcessingList.AddAttribute("count", DataProcessingList.Elements.Count.ToString());
      SpectrumList.AddAttribute("count", SpectrumList.Elements.Count.ToString());
      SpectrumList.AddAttribute("defaultDataProcessingRef", "Nova_mzML");

      MzML.AddAttribute("id", filename);
      foreach (NovaXmlElement el in Run)
      {
        MzML.AddElement(el);
      }
      IndexedmzML.AddElement(MzML);

      NovaXmlElement IndexList = new NovaXmlElement("indexList");
      if (HasSpectrum && HasChromatogram) IndexList.AddAttribute("count", "2");
      else IndexList.AddAttribute("count", "1");

      if (HasSpectrum)
      {
        IndexSpectrum = new NovaXmlElement("index");
        IndexSpectrum.AddAttribute("name", "spectrum");
        IndexList.AddElement(IndexSpectrum);
      }
      if (HasChromatogram)
      {
        IndexChromatogram = new NovaXmlElement("index");
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
      XmlFS.Close();


      XmlReaderSettings checkSettings = new XmlReaderSettings();
      checkSettings.Schemas.Add("http://psi.hupo.org/ms/mzml", "D:\\Data\\mzML\\mzML1.1.0.utf8.xsd");
      checkSettings.ValidationType = ValidationType.Schema;
      //checkSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
      checkSettings.ValidationEventHandler += MzMLValidationEventHandler;

      Console.WriteLine("Reading: " + filename);

      FileStream reader = new FileStream(filename, FileMode.Open, FileAccess.Read);
      XmlReader mzml = XmlReader.Create(reader, checkSettings);
      while (mzml.Read()) { }

      Console.WriteLine("Everything checks out.");
      mzml.Close();
      reader.Close();

      return true;
    }

    /// <summary>
    /// Writes the elements to file, and recursively calls itself for child elements.
    /// </summary>
    /// <param name="element"></param>
    private void WriteElement(NovaXmlElement element)
    {
      //Special cases that are tracked for indexing purposes
      if (element.Name == "spectrum")
      {
        writer.Flush();
        NovaXmlElement offset = new NovaXmlElement("offset",(XmlFS.Position+1).ToString());
        offset.AddAttribute("idRef", element.Attributes[1].Item2);
        IndexSpectrum.AddElement(offset);
      } 
      else if(element.Name == "indexList")
      {
        writer.Flush();
        IndexListOffset.Contents=XmlFS.Position.ToString();
      }

      //Special cases for starting elements
      if (element.Name == "mzML")
      {
        writer.WriteStartElement(element.Name, "http://psi.hupo.org/ms/mzml");
        writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
        writer.WriteAttributeString("xsi", "schemaLocation", null, "http://psi.hupo.org/ms/mzml http://psidev.info/files/ms/mzML/xsd/mzML1.1.2_idx.xsd");
      }
      //Write this element and all children elements.
      else writer.WriteStartElement(element.Name);
      foreach (Tuple<string, string> attr in element.Attributes)
      {
        writer.WriteAttributeString(attr.Item1, attr.Item2);
      }
      foreach (NovaXmlElement el in element.Elements)
      {
        WriteElement(el);
      }
      if (element.Contents != null) writer.WriteString(element.Contents);
      writer.WriteEndElement();
    }

    static void MzMLValidationEventHandler(object? sender, ValidationEventArgs e)
    {
      if (e.Severity == XmlSeverityType.Warning)
      {
        Console.Write("WARNING: ");
        Console.WriteLine(e.Message);
      }
      else if (e.Severity == XmlSeverityType.Error)
      {
        Console.Write("ERROR: ");
        Console.WriteLine(e.Message);
      }
    }
  }
}
