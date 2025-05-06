# Nova
Nova is a lightweight C# library for mass spectrometry file reading and spectral data management.

Documentation on Nova can be found here: https://schweppelab.github.io/Nova/

Notably, Nova uses Pipes for sending data to and receiving data from client processes: 
* https://schweppelab.github.io/Nova/classes/PipesServer.html
* https://schweppelab.github.io/Nova/classes/PipesClient.html

The latest releases for Nova can be found here: https://schweppelab.github.io/Nova/download/

### Usage
* Nova needs to be in Framework 4.8 if it is to be used with real-time MS applications utilizing [IAPI](https://github.com/thermofisherlsms/iapi).
* Nova.IO is supported in Core 8 to take advantage of the latest [RawFileReader](https://github.com/thermofisherlsms/RawFileReader).
