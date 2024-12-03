#define GLOBALMEM

using ProjectMIDAS.Data.Peak;
using ProjectMIDAS.Data.Spectrum;
using ProjectMIDAS.Data.SpectrumFixed;
using ProjectMIDAS.Data.SpectrumLight;

class PMApp
{

    public const int ScanCount= 100000;
    public const int ScanSize = 5000;
    public const bool Interrupt = false;
    public const bool Echo = false;

#if(GLOBALMEM)
    public List<Spectrum> spec;
    public List<SpectrumLight> specLight;
    public List<SpectrumFixed> specFixed;
#endif

    static void Main(string[] args)
    {
        PMApp app = new PMApp();

        app.MemoryTestSpectrumLight(); //Light #1
        app.Clear();
        if (Interrupt) app.Pause();

        app.MemoryTestSpectrumFixed(); //Fixed #1
        app.Clear();
        if (Interrupt) app.Pause();

        app.MemoryTestSpectrum();  //Normal #1
        app.Clear();
        if (Interrupt) app.Pause();

        

        app.MemoryTestSpectrumFixed(); //Fixed #2
        app.Clear();
        if (Interrupt) app.Pause();

        app.MemoryTestSpectrum();  //Normal #2
        app.Clear();
        if (Interrupt) app.Pause();

        app.MemoryTestSpectrumLight(); //Light #2
        app.Clear();
        if (Interrupt) app.Pause();

    }

    PMApp()
    {
#if GLOBALMEM
        spec = new List<Spectrum>();
        specLight = new List<SpectrumLight>();
        specFixed = new List<SpectrumFixed>();
#endif
    }

    public void Clear()
    {
#if GLOBALMEM
        spec.Clear();
        specLight.Clear();
        specFixed.Clear();
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
            Spectrum spectrum = new Spectrum();
            for(int j = 0; j < ScanSize; j++)
            {
                Centroid centroid = new Centroid();
                centroid.Mz = j;
                centroid.Intensity = j;
                spectrum.Centroids.Add(centroid);
            }
            spec.Add(spectrum);
        }
        if (Echo) Console.WriteLine("MemoryTestSpectrum Finished");
    }

    public void MemoryTestSpectrumLight()
    {
        //Create a set of 1000 spectra of 1000 datapoints
        if (Echo) Console.WriteLine("MemoryTestSpectrumLight");
#if (!GLOBALMEM)
        List<SpectrumLight> specLight = new List<SpectrumLight>();
#endif
        for (int i = 0; i < ScanCount; i++)
        {
            SpectrumLight spectrum = new SpectrumLight();
            for (int j = 0; j < ScanSize; j++)
            {
                ProjectMIDAS.Data.SpectrumLight.sCentroid centroid = new ProjectMIDAS.Data.SpectrumLight.sCentroid();
                centroid.Mz = j;
                centroid.Intensity = j;
                spectrum.Centroids.Add(centroid);
            }
            specLight.Add(spectrum);
        }
        if (Echo) Console.WriteLine("MemoryTestSpectrumLight Finished");
    }

    public void MemoryTestSpectrumFixed()
    {
        //Create a set of 1000 spectra of 1000 datapoints
        if (Echo) Console.WriteLine("MemoryTestSpectrumFixed");
#if (!GLOBALMEM)
        List<SpectrumFixed> specFixed = new List<SpectrumFixed>();
#endif
        for (int i = 0; i < ScanCount; i++)
        {
            SpectrumFixed spectrum = new SpectrumFixed(ScanSize);
            for (int j = 0; j < ScanSize; j++)
            {
                spectrum.Centroids[j].Mz = j;
                spectrum.Centroids[j].Intensity = j;
            }
            specFixed.Add(spectrum);
        }
        if (Echo) Console.WriteLine("MemoryTestSpectrumFixed Finished");
    }

    public void Pause()
    {
        Console.WriteLine("Press SPACE to continue");
        do
        {

        } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);
    }
}