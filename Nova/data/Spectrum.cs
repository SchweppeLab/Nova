using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using Nova.Data;

namespace Nova.Data.Spectrum
{
    /// <summary>
    /// The struct-based contents of a single spectrum/scan event
    /// </summary>
  public class Spectrum : SpectrumFoundation, IMIDASSpectrum
  {

    #region CONSTRUCTORS
    //=== CONSTRUCTORS ===//
    /// <summary>
    /// Constructor for Spectrum class.
    /// </summary>
    public Spectrum(int sz=0) : base()
    {
      Count = sz;
      DataPoints = new sSpecDP[Count];
    }

    /// <summary>
    /// Construct a Spectrum from a serialized data stream.
    /// </summary>
    /// <param name="str">The UTF data stream</param>
    public Spectrum(byte[] arr)
    {
      Deserialize(arr);
    }
    #endregion

    #region INDEXERS & OPERATORS
    //=== INDEXERS & OPERATORS ==//
    #endregion

    #region FUNCTIONS
    //=== FUNCTIONS ===//

    public void Deserialize(byte[] data)
    {
      using (MemoryStream m = new MemoryStream(data))
      using (BinaryReader reader = new BinaryReader(m,System.Text.Encoding.Unicode))
      {
        ScanNumber = reader.ReadInt32();
        MsLevel = reader.ReadInt32();
        Centroid = reader.ReadBoolean();
        RetentionTime = reader.ReadDouble();
        StartMz = reader.ReadDouble();
        EndMz = reader.ReadDouble();
        TotalIonCurrent = reader.ReadDouble();
        BasePeakIntensity = reader.ReadDouble();
        FaimsState = reader.ReadBoolean();
        FaimsCV = reader.ReadDouble();
        Analyzer = reader.ReadString();
        IonInjectionTime = reader.ReadDouble();
        ScanType = reader.ReadString();

        ScanFilter = reader.ReadString();

        int pre= reader.ReadInt32();
        for (int a = 0; a < pre; a++)
        {
          PrecursorIon pi = new PrecursorIon();
          pi.IsolationMz=reader.ReadDouble();
          pi.IsolationWidth= reader.ReadDouble();
          pi.MonoisotopicMz=reader.ReadDouble();
          pi.Charge = reader.ReadInt32();
          Precursors.Add(pi);
        }

        Count = reader.ReadInt32();
        Resize(Count);
        for (int a = 0; a < Count; a++)
        {
          DataPoints[a].Mz = reader.ReadDouble();
          DataPoints[a].Intensity = reader.ReadDouble();
        }
        
      }
    }
 

    /// <summary>
    /// Resizes the spectrum to a new number of peaks. Does not preserve exisiting data.
    /// </summary>
    /// <param name="sz"></param>
    public void Resize(int sz = 0)
    {
      Count = sz;
      DataPoints = new sSpecDP[Count];
    }

    public byte[] Serialize()
    {
      using (MemoryStream m = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(m,System.Text.Encoding.Unicode))
      {
        writer.Write(ScanNumber);
        writer.Write(MsLevel);
        writer.Write(Centroid);
        writer.Write(RetentionTime);
        writer.Write(StartMz);
        writer.Write(EndMz);
        writer.Write(TotalIonCurrent);
        writer.Write(BasePeakIntensity);
        writer.Write(FaimsState);
        writer.Write(FaimsCV);
        writer.Write(Analyzer);
        writer.Write(IonInjectionTime);
        writer.Write(ScanType);

        writer.Write(ScanFilter);

        writer.Write(Precursors.Count);
        for(int a = 0; a < Precursors.Count; a++)
        {
          writer.Write(Precursors[a].IsolationMz);
          writer.Write(Precursors[a].IsolationWidth);
          writer.Write(Precursors[a].MonoisotopicMz);
          writer.Write(Precursors[a].Charge);
        }
        
        writer.Write(Count);
        for (int a = 0; a < Count; a++)
        {
          writer.Write(DataPoints[a].Mz);
          writer.Write(DataPoints[a].Intensity);
        }
        
        return m.ToArray();
      }
    }
    #endregion

    #region DATA MEMBERS
    //=== DATA MEMBERS ===//
    /// <summary>
    /// Total number of peaks in the current scan
    /// </summary>
    /// 
    public int Count { get; private set; } = 0;

    public sSpecDP[] DataPoints;
    //private int DataPointsCount;
    #endregion

        /* START TODO: Fill out the rest of this information after speed testing the above code

        /// <summary>
        /// Holds precursor information for dependent scans.
        /// There may be multiple precursors for a single scan.
        /// </summary>
        /// <typeparam name="Precursor"></typeparam>
        /// <returns></returns>
        public List<Precursor> Precursors { get; set; } = new List<Precursor>();

        /// <summary>
        /// Returns the m/z of the first precursor. 
        /// </summary>
        /// <value></value>
        public double PrecursorMz
        {
            get { return Precursors[0].Mz; }
            set { Precursors[0].Mz = value; }
        }

        /// <summary>
        /// Monoisotopic Mz for the precursor, if exist, otherwise should equal PrecursorMz
        /// </summary>
        public double MonoisotopicMz { get; set; }
        /// <summary>
        /// Returns the charge of the first precursor.
        /// </summary>
        /// <value></value>
        public int PrecursorCharge
        {
            get { return Precursors[0].Charge; }
            set { Precursors[0].Charge = value; }
        }

        public double PrecursorIntensity
        {
            get { return Precursors[0].Intensity; }
            set { Precursors[0].Intensity = value; }
        }

        public double IsolationMz
        {
            get { return Precursors[0].IsolationMz; }
            set { Precursors[0].IsolationMz = value; }
        }

        END TODO */

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Centroids.Clear();
                    //Centroids = null; //Not nullable?
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
