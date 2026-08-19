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

namespace TestNova
{
  /// <summary>
  /// Shared helper for locating Test/Files/, used by every test class that reads fixture files
  /// directly from the source tree rather than a copied build output.
  /// </summary>
  internal static class TestFilePaths
  {
    /// <summary>
    /// Resolves the Test/Files directory from the running test assembly's own location
    /// (AppContext.BaseDirectory) rather than Environment.CurrentDirectory -- the working
    /// directory a test runner is invoked from is not guaranteed to match the assembly's
    /// output directory (see HYG-2, docs/known-issues.md), while BaseDirectory always is.
    /// Still assumes the current bin/&lt;config&gt;/net8.0/ output depth (three levels below
    /// the Test project root); if that layout ever changes, update this in one place.
    /// </summary>
    public static string GetFilesDirectory()
    {
      // AppContext.BaseDirectory carries a trailing separator, which makes the first
      // Directory.GetParent call a no-op (it returns the same directory rather than the one
      // above it) -- trim it first so this walks up exactly three levels, same as the
      // Environment.CurrentDirectory-based code this replaced.
      string baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      string dir = string.Empty;
      DirectoryInfo? dirInfo = Directory.GetParent(baseDir);
      if (dirInfo?.Parent?.Parent != null)
      {
        dir = dirInfo.Parent.Parent.FullName;
      }
      return Path.Combine(dir, "Files");
    }
  }
}
