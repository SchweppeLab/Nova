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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Io.Read
{
  /// <summary>
  /// Creates an opened <see cref="ISpectrumFileReader"/> for an MS data file
  /// </summary>
  public class SpectrumFileReaderFactory
  {
    //Format detection and reader construction live on FileReader so the two dispatch paths can't drift
    /// <summary>
    /// Creates a reader for the file, chosen by its extension, and opens it
    /// </summary>
    /// <param name="file">The file to open</param>
    /// <param name="filter">The MS levels to read</param>
    /// <returns>A reader with the file open</returns>
    /// <exception cref="ArgumentException">The file extension is not a recognized format</exception>
    /// <exception cref="SpectrumFileOpenException">The file could not be opened or indexed</exception>
    public static ISpectrumFileReader GetReader(string file, MSFilter filter)
    {
      FileFormat format;
      try
      {
        format = FileReader.CheckFileFormat(file);
      }
      catch (FormatException ex)
      {
        throw new ArgumentException("Unrecognized file extension: " + Path.GetExtension(file), ex);
      }

      ISpectrumFileReader? reader = FileReader.CreateReader(format, filter);
      if (reader == null)
      {
        throw new ArgumentException("Unsupported file extension: " + Path.GetExtension(file));
      }

      if (!reader.Open(file))
      {
        string detail = (reader as IOpenFailureDetail)?.OpenFailure ?? "the reader reported failure without detail.";
        throw new SpectrumFileOpenException(file, detail);
      }
      return reader;
    }
  }
}
