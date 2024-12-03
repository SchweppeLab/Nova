
using System.Collections.Generic;
using System;

/// <summary>
/// Class based peak information. May make sense if a lot of functionality needs to be added to each peak, but may
/// not make sense if performance is preferred.
/// </summary>
namespace ProjectMIDAS.Data.Peak
{
    /// <summary>
    /// For pulling spectral information from a scan
    /// </summary>
    public struct Centroid //: ICentroid, IEquatable<Centroid>
    {
        /// <summary>
        /// Basic peak constructor. Additional Thermo features disabled while speed testing. Note that double intensity is probably
        /// more precision than necessary and adds 33% space requirements.
        /// </summary>
        /// <param name="mz"></param>
        /// <param name="intensity"></param>
        public Centroid(double mz, double intensity) //, double baseline = 0, double noise = 0, double resolution = 0)
        {
            Mz = mz;
            Intensity = intensity;
            //Baseline = baseline;
            //Noise = noise;
            //Resolution = resolution;
            //Z = 0;
        }

        //public Centroid(double mz, double intensity) //, int z, double baseline = 0, double noise = 0, double resolution = 0)
        //{
        //    Mz = mz;
        //    Intensity = intensity;
        //    //Z = z;
        //    //Noise = noise;
        //    //Baseline = baseline;
        //    //Resolution = resolution;
        //}

        /// <summary>
        /// Centroid m/z
        /// </summary>
        public double Mz { get; set; }
        //public int Z { get; set; }
        /// <summary>
        /// Baseline
        /// </summary>
        //public double Baseline { get; set; }

        /// <summary>
        /// Centroid intensity
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// Source noise level
        /// </summary>
        //public double Noise { get; set; }

        /// <summary>
        /// Centroid resolution
        /// </summary>
        //public double Resolution { get; set; }

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

    //public class IntensityComparer : IComparer<Centroid>
    //{
    //    public int Compare(Centroid x, Centroid y)
    //    {
    //        return y.Intensity.CompareTo(x.Intensity);
    //    }
    //}
}
