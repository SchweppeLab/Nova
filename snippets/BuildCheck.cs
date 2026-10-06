// Temporary: proves the toolchain restores and compiles against the release
// packages. Replaced by real snippet files once the setup is confirmed.

using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets
{
  internal static class BuildCheck
  {
    internal static void TouchBothPackages()
    {
      // From Nova.IO
      FileReader reader = new FileReader(MSFilter.MS1 | MSFilter.MS2);

      // From Nova
      Spectrum spectrum = new Spectrum();

      System.Console.WriteLine(reader.ScanCount + " " + spectrum.Count);
    }
  }
}
