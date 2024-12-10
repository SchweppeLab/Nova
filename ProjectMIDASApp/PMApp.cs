#define GLOBALMEM

using ProjectMIDAS.Data.Spectrum;

class PMApp
{

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
        PMApp app = new PMApp();

        app.MemoryTestSpectrumEx(); //Fixed #1
        app.Clear();
        if (Interrupt) app.Pause();

        app.MemoryTestSpectrum();  //Normal #1
        app.Clear();
        if (Interrupt) app.Pause();

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
                spec[i][j].Mz = j;
                spec[i][j].Intensity = j;
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
                specEx[i][j].Mz = j;
                specEx[i][j].Intensity = j;
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