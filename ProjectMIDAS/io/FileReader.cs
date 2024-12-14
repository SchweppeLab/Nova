using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectMIDAS.Data.Spectrum;

namespace ProjectMIDAS.IO
{
  internal class FileReader
  {
    public string FileName { get; }

    /// <summary>
    /// Checks to see if we requested, or already have, a valid file from which to read a spectrum. Use null to specify reading
    /// another spectrum from the same file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>true if the file is already open, false if not open</returns>
    /// <exception cref="ArgumentNullException">No file name was given and no file is open.</exception>
    public bool CheckFile(string fileName)
    {
      //first, check if we're requesting a new spectrum from the same file
      if (fileName == null)
      {
        if (FileName == null)
        {
          throw new ArgumentNullException(fileName, "null file name invalid unless file has already been opened.");
        }
        return true;
      }
      
      //next, check if we're reading the same file
      if (fileName == FileName) return true;

      //finally, check if this file exists
      if(!File.Exists(fileName)) 
      {
        throw new FileNotFoundException(fileName);
      }
      return false;
    }
    public Spectrum ReadSpectrum(string fileName, bool centroid = true)
    {
      Spectrum spectrum = null;
      try
      {
        //If CheckFile returns false, that means we need to open a new file
        if (!CheckFile(fileName))
        {
          //TODO: the function below
          OpenSpectrumFile();
        }
      }
      catch (Exception ex) 
      {
        //TODO: Handle file checking exceptions
      }
      return spectrum;
    }

  }
}
