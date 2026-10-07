// Copyright 2025 Michael Hoopmann
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Nova.Io.Write
{
  /// <summary>
  /// Instrument components, for describing an instrument configuration
  /// </summary>
  public enum InstrumentComponents
  {
    /// <summary>Electron multiplier detector</summary>
    ElectronMultiplier,
    /// <summary>Electrospray ionization source</summary>
    Electrospray,
    /// <summary>Inductive detector</summary>
    InductiveDetector,
    /// <summary>Ion trap analyzer</summary>
    IonTrap,
    /// <summary>Nanospray ionization source</summary>
    Nanospray,
    /// <summary>Orbitrap analyzer</summary>
    Orbitrap,
    /// <summary>Quadrupole analyzer</summary>
    Quadrupole
  }
}
