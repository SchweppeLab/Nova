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
  /// Thrown when an MS data file cannot be opened or indexed. Derives from <see cref="IOException"/> so existing
  /// broad file-error handling still catches it.
  /// </summary>
  public class SpectrumFileOpenException : IOException
  {
    /// <summary>
    /// The file that could not be opened.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Creates the exception for a file that could not be opened
    /// </summary>
    /// <param name="fileName">The file that could not be opened</param>
    /// <param name="detail">Why, as reported by the reader</param>
    public SpectrumFileOpenException(string fileName, string detail)
      : base("Failed to open '" + fileName + "': " + detail)
    {
      FileName = fileName;
    }

    /// <summary>
    /// Creates the exception for a file that could not be opened, wrapping the exception that caused it
    /// </summary>
    /// <param name="fileName">The file that could not be opened</param>
    /// <param name="detail">Why, as reported by the reader</param>
    /// <param name="innerException">The exception that caused the failure</param>
    public SpectrumFileOpenException(string fileName, string detail, Exception innerException)
      : base("Failed to open '" + fileName + "': " + detail, innerException)
    {
      FileName = fileName;
    }
  }

  /// <summary>
  /// Implemented by the format readers to carry the reason a failed <c>Open</c> failed, so the
  /// caller gets the real detail instead of a bare "false".
  /// </summary>
  internal interface IOpenFailureDetail
  {
    /// <summary>
    /// Why the most recent <c>Open</c> call returned false, or null if it succeeded.
    /// </summary>
    string? OpenFailure { get; }
  }
}
