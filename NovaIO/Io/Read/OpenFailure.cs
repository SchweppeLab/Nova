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

using System;
using System.IO;

namespace Nova.Io.Read
{
  /// <summary>
  /// Thrown when an MS data file cannot be opened or indexed.
  /// <para>
  /// This exists so a caller can tell "this file could not be read" from "that scan isn't in this
  /// file" -- both of which used to look identical: an empty <c>Spectrum</c>. Before BUG-10 the
  /// readers wrote the reason to the console and reported failure, and the two paths that build a
  /// reader for you (<see cref="SpectrumFileReaderFactory.GetReader"/>, and
  /// <see cref="FileReader"/>'s Read* overloads taking a new file name) then discarded that
  /// failure and handed back a reader that silently yielded zero peaks for every scan.
  /// </para>
  /// <para>
  /// Derives from <see cref="IOException"/> so existing broad file-error handling still catches
  /// it, but is its own type so Nova can re-throw precisely what it means without also changing
  /// how unrelated <see cref="IOException"/>s are handled.
  /// </para>
  /// </summary>
  public class SpectrumFileOpenException : IOException
  {
    /// <summary>
    /// The file that could not be opened.
    /// </summary>
    public string FileName { get; }

    public SpectrumFileOpenException(string fileName, string detail)
      : base("Failed to open '" + fileName + "': " + detail)
    {
      FileName = fileName;
    }

    public SpectrumFileOpenException(string fileName, string detail, Exception innerException)
      : base("Failed to open '" + fileName + "': " + detail, innerException)
    {
      FileName = fileName;
    }
  }

  /// <summary>
  /// Implemented by the format readers to carry the reason a failed <c>Open</c> failed, so the
  /// caller gets the real detail instead of a bare "false".
  /// <para>
  /// Deliberately internal, and implemented explicitly by the readers, so it adds no public API
  /// surface of its own. When this was written <c>ThermoRawReader</c> was still public, which
  /// made the explicit implementation load-bearing; all four readers are internal as of HYG-7,
  /// so it is now consistency rather than necessity.
  /// </para>
  /// </summary>
  internal interface IOpenFailureDetail
  {
    /// <summary>
    /// Why the most recent <c>Open</c> call returned false, or null if it succeeded.
    /// </summary>
    string? OpenFailure { get; }
  }
}
