#define GLOBALMEM

using System.Collections.Specialized;
using ProjectMIDAS.Data.Spectrum;
using ProjectMIDAS.IO;

class PMApp
{

  public FileReader Reader;


    public const int ScanCount= 100000;
    public const int ScanSize = 5000;
    public const bool Interrupt = false;
    public const bool Echo = false;

#if(GLOBALMEM)
    public List<Spectrum> spec;
    public List<SpectrumEx> specEx;
#endif

  static void Main(string[] args)
  {
    

    //Test code accepts a Thermo raw file as a parameter, then reads all MS2 & MS3 scans from the file.
    FileReader Reader = new FileReader();
    Reader.Filter = MSFilter.MS2 | MSFilter.MS3;
    Spectrum spec = new Spectrum(); //alternatively make this a global variable...

    //Reading the first spectrum from a file is easy, just give it the file name.
    //If filters are used, the reader automatically advances to the first spectrum
    //that satisfies the filter. This is in a try block because exceptions regarding
    //the existence of the file can be thrown. But I'm not convinced of the utility.
    try
    {
      spec = Reader.ReadSpectrum(args[0]);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }

    //A ScanNumber of 0 indicates failure to read a scan. This could be for various
    //reasons: end of file, no scan data, no scans fit filter parameters, failure to
    //open the file, etc.
    int count = 0;
    while (spec.ScanNumber > 0)
    {
      //TODO: whatever you want to do with a spectrum...
      count++;

      //Reading the next spectrum is just another call to ReadSpectrum() without any parameters.
      spec = Reader.ReadSpectrum();
    }

    //Give some output to indicate success.
    Console.WriteLine(count.ToString() + " MS2 and MS3 scans in " + args[0]);


    /*
    PMApp app = new PMApp();

    app.MemoryTestSpectrumEx(); //Fixed #1
    app.Clear();
    if (Interrupt) app.Pause();

    app.MemoryTestSpectrum();  //Normal #1
    app.Clear();
    if (Interrupt) app.Pause();

    app.MemoryTestSpectrum();  //Normal #2
    app.Clear();
    if (Interrupt) app.Pause();

    app.MemoryTestSpectrumEx(); //Fixed #2
    app.Clear();
    if (Interrupt) app.Pause();
    */

  }

    PMApp()
    {
#if GLOBALMEM
        spec = new List<Spectrum>();
        specEx = new List<SpectrumEx>();
#endif
    }

    public void Clear()
    {
#if GLOBALMEM
        spec.Clear();
        specEx.Clear();
#endif
    }

    public void MemoryTestSpectrum()
    {
        //Create a set of 1000 spectra of 1000 datapoints
        if(Echo) Console.WriteLine("MemoryTestSpectrum");
#if (!GLOBALMEM)
        List<Spectrum> spec = new List<Spectrum>();
#endif
        for (int i = 0; i < ScanCount; i++)
        {
            spec.Add(new Spectrum(ScanSize));
            for(int j = 0; j < ScanSize; j++)
            {
                spec[i].DataPoints[j].Mz = j;
                spec[i].DataPoints[j].Intensity = j;
            }
        }
        if (Echo) Console.WriteLine("MemoryTestSpectrum Finished");
    }

    public void MemoryTestSpectrumEx()
    {
        //Create a set of 1000 spectra of 1000 datapoints
        if (Echo) Console.WriteLine("MemoryTestSpectrumEx");
#if (!GLOBALMEM)
        List<SpectrumEx> specEx = new List<SpectrumEx>();
#endif
        for (int i = 0; i < ScanCount; i++)
        {
            specEx.Add(new SpectrumEx(ScanSize));
            for (int j = 0; j < ScanSize; j++)
            {
                specEx[i].DataPoints[j].Mz = j;
                specEx[i].DataPoints[j].Intensity = j;
            }
        }
        if (Echo) Console.WriteLine("MemoryTestSpectrumEx Finished");
    }

    public void Pause()
    {
        Console.WriteLine("Press SPACE to continue");
        do
        {

        } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);
    }
}