using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  public class PrecursorIon
  {
    /// <summary>
    /// PrecursorIon object constructor
    /// </summary>
    public PrecursorIon()
    {
    }

    /// <summary>
    /// PrecursorIon object copy constructor
    /// </summary>
    /// <param name="pi"></param>
    public PrecursorIon(PrecursorIon pi)
    {
      MonoisotopicMz=pi.MonoisotopicMz;
      Intensity=pi.Intensity;
      Charge=pi.Charge;
      IsolationMz=pi.IsolationMz;
      IsolationWidth=pi.IsolationWidth;
    }

    public PrecursorIon(double mz, double intensity = 0, int charge = 0)
    {
      MonoisotopicMz = mz;
      Intensity = intensity;
      Charge = charge;
    }

    /// <summary>
    /// Resets all variables to 0.
    /// </summary>
    public void Clear()
    {
      MonoisotopicMz = 0;
      Intensity = 0;
      Charge = 0;
      IsolationMz = 0;
      IsolationWidth = 0;
    }

    /// <summary>
    /// The putative monoisotopic m/z peak of the isotope envelope that contains the isolation m/z.
    /// </summary>
    public double MonoisotopicMz { get; set; } = 0;

    /// <summary>
    /// Intensity of the IsolationMz precursor peak.
    /// </summary>
    public double Intensity { get; set; } = 0;

    /// <summary>
    /// Charge state of the precursor.
    /// </summary>
    public int Charge { get; set; } = 0;


    /// <summary>
    /// The m/z that the instrument targeted for isolation.
    /// </summary>
    public double IsolationMz { get; set; } = 0;

    /// <summary>
    /// The size of the window that the instrument targeted for isolation.
    /// </summary>
    public double IsolationWidth { get; set; } = 0;

    //TODO: Determine if this is relevant; might be Monocle-specific...
    /// <summary>
    /// Proportion of the intensity in the isolation window
    /// that belongs to the precursor.
    /// 
    /// This should be a value from zero to one.
    /// </summary>
    public double IsolationSpecificity { get; set; }
  }
}
