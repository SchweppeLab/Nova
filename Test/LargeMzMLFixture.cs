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

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace TestNova
{
  /// <summary>
  /// Builds a throwaway indexed mzML whose spectra sit at byte offsets past 2^31 (and, on request,
  /// past 2^32), for the BUG-9 regression tests.
  ///
  /// <para>
  /// <b>Why this isn't a checked-in file.</b> A fixture big enough to reproduce BUG-9 is 2.5 GB+,
  /// which has no business in a Git repository. It is generated into the OS temp directory at test
  /// time and deleted in <see cref="Dispose"/>; nothing lands in Test/Files/.
  /// </para>
  ///
  /// <para>
  /// <b>Why it costs nothing.</b> The file is marked sparse (NTFS <c>FSCTL_SET_SPARSE</c>) before
  /// anything is written, so the multi-gigabyte span between the header and the spectra is a hole:
  /// it reads back as zeros without ever being written to or occupying disk. Only a few KB of real
  /// XML is written, at scattered positions. Measured locally: ~4 ms to build and ~0 bytes
  /// allocated for a 4.5 GiB fixture. Nova never parses these files as one document -- Open() seeks
  /// straight to the index and ParseSpectrum() seeks straight to one spectrum -- so a file that is
  /// mostly hole exercises exactly the code path under test.
  /// </para>
  ///
  /// <para>
  /// <b>The safety gate matters more than the trick.</b> If the sparse flag does not take (non-NTFS
  /// temp volume, a future runner image change, a P/Invoke that fails), then SetLength followed by a
  /// write near the end makes NTFS zero-fill the gap for real -- gigabytes physically written on a
  /// CI runner with limited free space, failing for a reason that looks nothing like its cause.
  /// So <see cref="TryCreate"/> verifies the flag actually applied and bails out <i>before</i>
  /// writing anything past the hole, and the tests report Inconclusive rather than proceeding.
  /// Do not remove that check.
  /// </para>
  /// </summary>
  internal sealed class LargeMzMLFixture : IDisposable
  {
    private const uint FSCTL_SET_SPARSE = 0x000900C4;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode,
      IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize,
      out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint GetCompressedFileSizeW(string lpFileName, out uint lpFileSizeHigh);

    /// <summary>Full path to the generated mzML.</summary>
    public string Path { get; private set; } = string.Empty;

    /// <summary>Scan numbers present in the file, in file order (1-based, contiguous).</summary>
    public int[] ScanNumbers { get; private set; } = Array.Empty<int>();

    /// <summary>Byte offset of each scan, parallel to <see cref="ScanNumbers"/>.</summary>
    public long[] Offsets { get; private set; } = Array.Empty<long>();

    /// <summary>Byte offset the file's own indexListOffset element points at.</summary>
    public long IndexListOffset { get; private set; }

    /// <summary>Bytes actually allocated on disk, for diagnostics. Near zero when sparse works.</summary>
    public long AllocatedBytes { get; private set; }

    /// <summary>The expected m/z values for a given scan number, matching what was written.</summary>
    public static double[] ExpectedMz(int scanNumber)
    {
      int idx = scanNumber - 1;
      return new[] { 100.0 + idx, 200.0 + idx, 300.0 + idx };
    }

    /// <summary>The expected intensities for a given scan number, matching what was written.</summary>
    public static double[] ExpectedIntensity(int scanNumber)
    {
      int idx = scanNumber - 1;
      return new[] { 1000.0 + idx, 2000.0 + idx, 3000.0 + idx };
    }

    /// <summary>The expected retention time, in minutes, for a given scan number.</summary>
    public static double ExpectedRetentionTime(int scanNumber)
    {
      return 1.5 * scanNumber;
    }

    /// <summary>
    /// Attempts to build the fixture. Returns false with a human-readable <paramref name="skipReason"/>
    /// when the environment can't support it, in which case nothing of consequence was written.
    /// </summary>
    /// <param name="logicalSize">The size the file should report.</param>
    /// <param name="spectrumOffsets">Byte offset for each spectrum, ascending, within the file.</param>
    public static bool TryCreate(long logicalSize, long[] spectrumOffsets,
      out LargeMzMLFixture? fixture, out string skipReason)
    {
      fixture = null;
      skipReason = string.Empty;

      if (!OperatingSystem.IsWindows())
      {
        skipReason = "sparse-file fixtures are implemented for Windows/NTFS only.";
        return false;
      }

      string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "NovaLargeFileTests");
      Directory.CreateDirectory(dir);

      DriveInfo drive = new DriveInfo(System.IO.Path.GetPathRoot(dir)!);
      if (!string.Equals(drive.DriveFormat, "NTFS", StringComparison.OrdinalIgnoreCase))
      {
        skipReason = $"temp volume {drive.Name} is {drive.DriveFormat}, not NTFS; sparse files unavailable.";
        return false;
      }

      string path = System.IO.Path.Combine(dir,
        $"NovaLarge_{logicalSize}_{Guid.NewGuid():N}.mzML");

      try
      {
        var f = new LargeMzMLFixture { Path = path };
        using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite))
        {
          if (!DeviceIoControl(fs.SafeFileHandle, FSCTL_SET_SPARSE,
                IntPtr.Zero, 0, IntPtr.Zero, 0, out _, IntPtr.Zero))
          {
            skipReason = $"FSCTL_SET_SPARSE failed (error {Marshal.GetLastWin32Error()}).";
            fs.Dispose();
            TryDelete(path);
            return false;
          }

          // THE GATE: confirm the flag really applied before writing anything past a hole.
          // Without this, the SetLength below would make NTFS zero-fill gigabytes for real.
          if ((new FileInfo(path).Attributes & FileAttributes.SparseFile) == 0)
          {
            skipReason = "the SparseFile attribute did not apply; refusing to materialize a multi-GB file.";
            fs.Dispose();
            TryDelete(path);
            return false;
          }

          fs.SetLength(logicalSize);
          f.Build(fs, logicalSize, spectrumOffsets);
        }

        f.AllocatedBytes = GetAllocatedSize(path);
        fixture = f;
        return true;
      }
      catch (Exception ex)
      {
        skipReason = "could not build the fixture: " + ex.Message;
        TryDelete(path);
        return false;
      }
    }

    private void Build(FileStream fs, long logicalSize, long[] spectrumOffsets)
    {
      var enc = new UTF8Encoding(false);
      void WriteAt(long pos, string s)
      {
        fs.Seek(pos, SeekOrigin.Begin);
        byte[] b = enc.GetBytes(s);
        fs.Write(b, 0, b.Length);
      }

      WriteAt(0,
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<indexedmzML>\n <mzML>\n  <run id=\"novaLargeFixture\">\n" +
        "   <spectrumList count=\"" + spectrumOffsets.Length + "\">\n");

      ScanNumbers = new int[spectrumOffsets.Length];
      Offsets = (long[])spectrumOffsets.Clone();
      for (int i = 0; i < spectrumOffsets.Length; i++)
      {
        ScanNumbers[i] = i + 1;
        WriteAt(spectrumOffsets[i], SpectrumXml(ScanNumbers[i]));
      }

      // The index goes after the last spectrum, and the trailer at the very end of the file so
      // that Nova's 200-byte tail scan finds indexListOffset exactly where a real file has it.
      long indexPos = fs.Position + 64;
      var sb = new StringBuilder();
      sb.Append("<indexList count=\"1\">\n <index name=\"spectrum\">\n");
      for (int i = 0; i < ScanNumbers.Length; i++)
      {
        sb.Append("  <offset idRef=\"controllerType=0 controllerNumber=1 scan=")
          .Append(ScanNumbers[i].ToString(CultureInfo.InvariantCulture)).Append("\">")
          .Append(spectrumOffsets[i].ToString(CultureInfo.InvariantCulture)).Append("</offset>\n");
      }
      sb.Append(" </index>\n</indexList>\n");
      WriteAt(indexPos, sb.ToString());
      IndexListOffset = indexPos;

      string tail = "<indexListOffset>" + indexPos.ToString(CultureInfo.InvariantCulture) +
        "</indexListOffset>\n<fileChecksum></fileChecksum>\n</indexedmzML>\n";
      WriteAt(logicalSize - enc.GetByteCount(tail), tail);
      fs.SetLength(logicalSize);
    }

    // One MS2 spectrum, 3 peaks, 64-bit uncompressed m/z and intensity arrays.
    private static string SpectrumXml(int scanNumber)
    {
      string mzB64 = Convert.ToBase64String(ExpectedMz(scanNumber).SelectMany(BitConverter.GetBytes).ToArray());
      string inB64 = Convert.ToBase64String(ExpectedIntensity(scanNumber).SelectMany(BitConverter.GetBytes).ToArray());

      return "<spectrum index=\"" + (scanNumber - 1) +
        "\" id=\"controllerType=0 controllerNumber=1 scan=" + scanNumber + "\" defaultArrayLength=\"3\">\n" +
        " <cvParam cvRef=\"MS\" accession=\"MS:1000511\" name=\"ms level\" value=\"2\"/>\n" +
        " <cvParam cvRef=\"MS\" accession=\"MS:1000127\" name=\"centroid spectrum\" value=\"\"/>\n" +
        " <scanList count=\"1\"><scan>\n" +
        "  <cvParam cvRef=\"MS\" accession=\"MS:1000016\" name=\"scan start time\" value=\"" +
        ExpectedRetentionTime(scanNumber).ToString("R", CultureInfo.InvariantCulture) +
        "\" unitAccession=\"UO:0000031\" unitName=\"minute\"/>\n" +
        " </scan></scanList>\n" +
        " <binaryDataArrayList count=\"2\">\n" +
        "  <binaryDataArray encodedLength=\"" + mzB64.Length + "\">\n" +
        "   <cvParam cvRef=\"MS\" accession=\"MS:1000523\" name=\"64-bit float\" value=\"\"/>\n" +
        "   <cvParam cvRef=\"MS\" accession=\"MS:1000514\" name=\"m/z array\" value=\"\"/>\n" +
        "   <binary>" + mzB64 + "</binary>\n" +
        "  </binaryDataArray>\n" +
        "  <binaryDataArray encodedLength=\"" + inB64.Length + "\">\n" +
        "   <cvParam cvRef=\"MS\" accession=\"MS:1000523\" name=\"64-bit float\" value=\"\"/>\n" +
        "   <cvParam cvRef=\"MS\" accession=\"MS:1000515\" name=\"intensity array\" value=\"\"/>\n" +
        "   <binary>" + inB64 + "</binary>\n" +
        "  </binaryDataArray>\n" +
        " </binaryDataArrayList>\n" +
        "</spectrum>\n";
    }

    private static long GetAllocatedSize(string path)
    {
      uint low = GetCompressedFileSizeW(path, out uint high);
      if (low == 0xFFFFFFFF && Marshal.GetLastWin32Error() != 0) return -1;
      return ((long)high << 32) | low;
    }

    private static void TryDelete(string path)
    {
      try { if (File.Exists(path)) File.Delete(path); } catch { /* best effort */ }
    }

    public void Dispose()
    {
      TryDelete(Path);
    }
  }
}
