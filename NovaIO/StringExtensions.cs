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

namespace Nova.Io
{
  /// <summary>
  /// Project-local string helpers. Added 2026-08-19 (HYG-5, docs/known-issues.md) to replace
  /// call sites that were unknowingly depending on ThermoFisher.CommonCore.Data's own
  /// IsNullOrEmpty extension method for this -- a fragile, implicit dependency on a
  /// third-party package's incidental API surface for something that has nothing to do with
  /// Thermo.
  /// </summary>
  internal static class StringExtensions
  {
    public static bool IsNullOrEmpty(this string? value)
    {
      return string.IsNullOrEmpty(value);
    }
  }
}
