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
using System.Globalization;

namespace Nova.Io.Read
{
  /// <summary>
  /// Parses byte positions read from a file's own index
  /// </summary>
  internal static class ByteOffset
  {
    /// <summary>
    /// Parses a byte offset into a file as a 64-bit value, culture-invariant
    /// </summary>
    /// <param name="value">The text of an offset element</param>
    /// <returns>The byte position</returns>
    public static long Parse(string value)
    {
      return long.Parse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);
    }
  }
}
