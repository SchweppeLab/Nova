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
  public class SpectrumFileReaderFactory
  {
    // Extension-to-format detection and format-to-reader construction both live on
    // FileReader (CheckFileFormat / CreateReader) -- delegated to here rather than
    // duplicated, so the two dispatch paths can't drift out of sync (see CLEAN-3 in
    // docs/known-issues.md).
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

      reader.Open(file);
      return reader;
    }
  }
}
