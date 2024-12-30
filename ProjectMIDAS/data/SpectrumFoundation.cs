using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMIDAS.Data.Spectrum
{
  /// <summary>
  /// The spectrum data point structure defines the basic contents of a mass spectrum.
  /// </summary>
  public struct sSpecDP
  {
    /// <summary>
    /// The m/z value of a spectrum data point.
    /// </summary>
    public double Mz;
    /// <summary>
    /// The intensity (aka abundance) of a spectrum data point. This may in the future be set
    /// to foating point precision to save memory. Currently at double precision to maximize
    /// compatibility.
    /// </summary>
    public double Intensity;

    /// <summary>
    /// Spectrum data point constructor.
    /// </summary>
    /// <param name="mz"></param>
    /// <param name="intensity"></param>
    public sSpecDP(double mz, double intensity)
    {
        Mz = mz;
        Intensity = intensity;
    }
  }

  /// <summary>
  /// An advanced spectrum data structure where data points are represented by centroid peaks.
  /// This format replicates Thermo centroid structures. If only mz and intensity are used, it
  /// is more economical to use the sSpecDP structure.
  /// </summary>
  public struct sCentroid //: ICentroid, IEquatable<Centroid>
  {
    /// <summary>
    /// Basic centroid peak constructor.
    /// </summary>
    /// <param name="mz"></param>
    /// <param name="intensity"></param>
    public sCentroid(double mz, double intensity, int z=0, double baseline = 0, double noise = 0, double resolution = 0)
    {
        Mz = mz;
        Intensity = intensity;
        Baseline = baseline;
        Noise = noise;
        Resolution = resolution;
        Z = z;
    }

    /// <summary>
    /// Centroid m/z
    /// </summary>
    public double Mz;

    /// <summary>
    /// charge state
    /// </summary>
    public int Z;

    /// <summary>
    /// Baseline
    /// </summary>
    public double Baseline;

    /// <summary>
    /// Centroid intensity
    /// </summary>
    public double Intensity;

    /// <summary>
    /// Source noise level
    /// </summary>
    public double Noise;

    /// <summary>
    /// Centroid resolution
    /// </summary>
    public double Resolution;

            //TODO: possibly reimplement this here, but with more flexibility, such as PPM tolerance.
            //Will need to be speed tested.
            //public int CompareTo(object obj)
            //{
            //    Centroid objCent = (Centroid)obj;
            //    return this.Mz.CompareTo(objCent.Mz);
            //}

            /// <summary>
            /// Bin m/z values based on the Comet binning approach
            /// </summary>
            //public static int AssignBin(double dMass, double tolerance, double offset)
            //{
            //    double dInverseBinWidth = 1 / tolerance;
            //    double dOneMinusBinOffset = 1.0 - offset;

            //    return (int)(dMass * dInverseBinWidth + dOneMinusBinOffset);
            //}

            //public override bool Equals(object obj)
            //{
            //    return (obj is Centroid cent) && Equals(cent);
            //}

            //public override int GetHashCode()
            //{
            //    return Mz.GetHashCode();
            //}

            //public bool Equals(Centroid other)
            //{
            //    return Mz == other.Mz;
            //}
    }


  public abstract class SpectrumFoundation
  {

    /// <summary>
    /// SpectrumFoundation constructor.
    /// </summary>
    protected SpectrumFoundation() 
    {
      Precursors = new List<PrecursorIon>();
    }

    /// <summary>
    /// Identifies the peak data type as centroid (true) or profile (false)
    /// </summary>
    public bool Centroid { get; set; } = true;

    /// <summary>
    /// Total time, including injection time, to acquire the current scan (milliseconds)
    /// </summary>
    public double ElapsedScanTime { get; set; } = 0;

    /// <summary>
    /// Injection time used to acquire the scan ions (milliseconds, max = 5000)
    /// </summary>
    public double IonInjectionTime { get; set; } = 0;

    /// <summary>
    /// The MS level (e.g. 0 = Unknown, 1 = MS1, 2 = MS2, 3 = MS3, MSn)
    /// </summary>
    public int MsLevel { get; set; } = 0;

    /// <summary>
    /// Bool representation of polarity (true = positive)
    /// </summary>
    public bool Polarity { get; set; } = true;

    /// <summary>
    /// The current scan description
    /// </summary>
    public string ScanDescription { get; set; } = string.Empty;

    /// <summary>
    /// Scan event order
    /// </summary>
    public int ScanEvent { get; set; } = 0;

    /// <summary>
    /// Current scan number
    /// </summary>
    public int ScanNumber { get; set; } = 0;

        
        /// <summary>
        /// Thermo variable for master scan number
        /// </summary>
        //public int MasterIndex { get; set; }

        /// <summary>
        /// Mass analyzer used
        /// </summary>
        //public Analyzer Analyzer { get; set; } = Analyzer.IonTrap;

        /// <summary>
        /// String description of scan type
        /// </summary>
        //public string ScanType { get; set; }

    /// <summary>
    /// Scan filter line from RAW file
    /// </summary>
    public string ScanFilter { get; set; } = string.Empty;

    /// <summary>
    /// Scan retention time (minutes)
    /// </summary>
    public double RetentionTime { get; set; } = 0;

        /// <summary>
        /// Mz that scan starts at
        /// </summary>
        public double StartMz { get; set; } = 0;

        /// <summary>
        /// Mz that scan ends at
        /// </summary>
        public double EndMz { get; set; } = 0;

        /// <summary>
        /// Lowest Mz observed in scan
        /// </summary>
        public double LowestMz { get; set; }

        /// <summary>
        /// Highest Mz observed in scan
        /// </summary>
        public double HighestMz { get; set; }
        /// <summary>
        /// The most intense Mz peak in the scan
        /// </summary>
        public double BasePeakMz { get; set; }

        public double BasePeakIntensity { get; set; }

    /// <summary>
    /// FAIMS compensation voltage, if used (in volts)
    /// </summary>
    public int FaimsCV { get; set; } = 0;

        /// <summary>
        /// FAIMS state, it's possible to have a CV of 0 which is the int default.
        /// </summary>
        //public TriState FaimsState { get; set; } = TriState.Off;

        /// <summary>
        /// Total ion current for the current scan
        /// </summary>
        public double TotalIonCurrent { get; set; }

        /// <summary>
        /// If a dependent scan, the fragmentation energy used
        /// </summary>
        public double CollisionEnergy { get; set; }

    /// <summary>
    /// Zero or more precursor ions associated with this spectrum.
    /// </summary>
    public List<PrecursorIon> Precursors { get; set; }


    /// <summary>
    /// If a dependent scan, the parent scan number
    /// </summary>
    public int PrecursorMasterScanNumber { get; set; }

        /// <summary>
        /// If a dependent scan, the activation method used to generate the scan fragments
        /// </summary>
        //public string PrecursorActivationMethod { get; set; }
  }
}
