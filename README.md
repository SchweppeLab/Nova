# Nova
Nova is a lightweight C# library for mass spectrometry file reading and spectral data management.

Documentation on Nova can be found here: https://schweppelab.github.io/Nova/

Notably, Nova uses Pipes for sending data to and receiving data from client processes: https://schweppelab.github.io/Nova/classes/PipesServer.html

The latest releases for Nova can be found here: https://schweppelab.github.io/Nova/download/

### Usage
* Needs to be in Framework 4.8 solely so that it can be packaged with Helios. This is because Helios needs to deserialize Spectrum objects passed from external software (such as VirtualMS).
