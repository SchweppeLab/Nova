# Nova
Nova is a lightweight C# library for mass spectrometry file reading and spectral data management.

Documentation on Nova can be found here: https://schweppelab.github.io/Nova/

Notably, Nova uses Pipes for sending data to and receiving data from client processes: 
* https://schweppelab.github.io/Nova/classes/PipesServer.html
* https://schweppelab.github.io/Nova/classes/PipesClient.html

The latest releases for Nova can be found here: https://schweppelab.github.io/Nova/download/

### Usage
* Nova targets netstandard2.0, so the same build works with real-time MS applications on .NET Framework 4.8 (via [IAPI](https://github.com/thermofisherlsms/iapi)) and with .NET 8+ projects.
* Nova.IO is supported in Core 8 to take advantage of the latest [RawFileReader](https://github.com/thermofisherlsms/RawFileReader).

### Citing
Hoopmann, M. R.; McGann, C. D.; Rose, C. M.; Schweppe, D. K. 
"Nova: A Library for Rapid Development of Mass Spectrometry Software Applications."
*J. Am. Soc. Mass Spectrom.* 2025, 36 (8), 1836–1839. DOI: 10.1021/jasms.5c00141.
[Link](https://pubmed.ncbi.nlm.nih.gov/40690708/)
