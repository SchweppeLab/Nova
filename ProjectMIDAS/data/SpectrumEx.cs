using System.Runtime.InteropServices;

namespace ProjectMIDAS.Data.Spectrum
{
    /// <summary>
    /// An expanded version of the Spectrum object that uses the sCentroid structure for the data array.
    /// </summary>
    public class SpectrumEx : SpectrumFoundation, IDisposable
    {

        #region CONSTRUCTORS
        //=== CONSTRUCTORS ===//
        /// <summary>
        /// Constructor for Spectrum class.
        /// </summary>
        public SpectrumEx(int sz)
        {
            DataPointsCount = sz;
            DataPoints = new sCentroid[DataPointsCount];
        }
        #endregion

        #region INDEXERS & OPERATORS
        //=== INDEXERS & OPERATORS ==//
        /// <summary>
        /// Indexer to any data point in the spectrum. WARNING: no bounds check.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public sCentroid this[int i]
        {
            get => DataPoints[i];
            set => DataPoints[i] = value;
        }
        #endregion

        #region FUNCTIONS
        //=== FUNCTIONS ===//
        /// <summary>
        /// Total number of peaks in the current scan
        /// </summary>
        /// 
        public int Count() { return DataPointsCount; }
        #endregion

        #region DATA MEMBERS
        //=== DATA MEMBERS ===//
        private sCentroid[] DataPoints;
        private int DataPointsCount;
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
