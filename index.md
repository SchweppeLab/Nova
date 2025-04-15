---
layout: default
---

# Nova

Lightweight C# library for mass spectrometry file reading and spectral data management.



```csharp
// Example C# code
using Nova.Io.Read;
using Nova.Data;

int MS1Count=0;
int MS2Count=0;
FileReader reader = new FileReader("TheBestDataEver.mzML",MSFilter.MS1|MSFilter.MS2);
foreach(Spectrum spec in reader)
{
  if(spec.msLevel==1)
  {
    MS1Count++;
  }
  else
  {
    MS2Count++;
  }
}
Console.WriteLine("There are " + MS1Count + " MS scans and " + MS2Count + " MS/MS scan.");
```
