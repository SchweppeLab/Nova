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
  /// Parsing of byte positions read out of a file's own index (mzML's <c>indexListOffset</c> and
  /// <c>offset</c> elements, mzXML's <c>indexOffset</c>).
  /// </summary>
  internal static class ByteOffset
  {
    /// <summary>
    /// Parses a byte offset into a file as a 64-bit value.
    /// <para>
    /// Two things here are deliberate and should not be narrowed back:
    /// </para>
    /// <para>
    /// 1. <see cref="long"/>, not <see cref="int"/>. Modern Orbitrap runs routinely produce mzML
    /// files of 2-3 GB, and an int parse throws OverflowException the moment an offset passes
    /// int.MaxValue (2,147,483,647). That was BUG-9: it made every file over 2 GiB unreadable,
    /// and because the open failure was swallowed, every scan came back with zero peaks instead
    /// of an error. Note that an unsigned 32-bit type would not fix it either -- it only moves
    /// the cliff to 4 GiB.
    /// </para>
    /// <para>
    /// 2. <see cref="CultureInfo.InvariantCulture"/>. These values come from a file format, not
    /// from the user's locale. <c>int.Parse</c>/<c>Convert.ToInt32</c> without a format provider
    /// use the *current* culture, which is how this repo shipped a number-parsing bug once
    /// before (commit b486948).
    /// </para>
    /// </summary>
    /// <param name="value">The raw text of an offset element.</param>
    /// <returns>The byte position as a <see cref="long"/>.</returns>
    public static long Parse(string value)
    {
      return long.Parse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);
    }
  }
}
