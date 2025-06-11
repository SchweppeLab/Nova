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

using System.IO;
using Nova.Data;
using Nova.Io.Read;
using Nova.Io;
using ScottPlot;
using ScottPlot.Plottables;

namespace ScanViewer
{
  public partial class Form1 : Form
  {

    FileReader reader = new FileReader();
    Spectrum? spectrum;

    readonly Scatter plotSpec;

    public Form1()
    {
      InitializeComponent();
      Coordinates[] co = new Coordinates[1];
      co[0].X = 0;
      co[0].Y = 0;

      plotSpec = plotSpectrum.Plot.Add.Scatter(co);
      plotSpec.MarkerSize = 1;
      plotSpectrum.Plot.Axes.SetLimitsY(0, 100);
      plotSpectrum.Plot.YLabel("Relative Intensity");
      plotSpectrum.Plot.XLabel("m/z");
      plotSpectrum.Plot.Axes.Bottom.Label.FontSize = 24;
      plotSpectrum.Plot.Axes.Left.Label.FontSize = 24;
      plotSpectrum.Plot.Axes.Bottom.TickLabelStyle.FontSize = 18;
      plotSpectrum.Plot.Axes.Left.TickLabelStyle.FontSize = 18;
      ScottPlot.TickGenerators.NumericAutomatic tickGenX = new();
      tickGenX.TickDensity = 1;
      plotSpectrum.Plot.Axes.Bottom.TickGenerator = tickGenX;
      plotSpectrum.Plot.HideGrid();
      plotSpectrum.Plot.FigureBackground.Color = Colors.White;
      plotSpectrum.Refresh();
    }

    private void UpdatePlot()
    {
      double[] x;
      double[] y;
      int a = 0;
      if (spectrum == null) return;

      x = new double[spectrum.DataPoints.Count() * 3 + 2];
      y = new double[spectrum.DataPoints.Count() * 3 + 2];
      x[a] = 140;
      y[a++] = 0;
      for (int i = 0; i < spectrum.DataPoints.Count(); i++)
      {
        //Log(qs.mz[i].ToString()+" " + (qs.intensity[i] / qs.maxIntensity * 100).ToString());
        //centroided peaks need to be plotted with 3 points 
        x[a] = spectrum.DataPoints[i].Mz;
        y[a++] = 0;
        x[a] = spectrum.DataPoints[i].Mz;
        y[a++] = spectrum.DataPoints[i].Intensity / spectrum.BasePeakIntensity * 100;
        x[a] = spectrum.DataPoints[i].Mz;
        y[a++] = 0;
      }
      x[a] = 1200;
      y[a] = 0;

      lock (plotSpectrum.Plot.Sync)
      {
        plotSpectrum.Plot.Clear();
        var scat = plotSpectrum.Plot.Add.Scatter(x, y);
        scat.MarkerSize = 1;
        plotSpectrum.Plot.Axes.SetLimitsX(140, 1250);
      }
      plotSpectrum.Refresh();
    }

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        spectrum = reader.ReadSpectrum(openFileDialog1.FileName, -1, true);
        toolStripStatusLabel1.Text = spectrum.ScanNumber.ToString();
        toolStripStatusLabel2.Text = openFileDialog1.FileName;
        UpdatePlot();
      }
    }

    private void toolStripButton3_Click(object sender, EventArgs e)
    {
      if (spectrum == null) return;
      int tmp = spectrum.ScanNumber;
      spectrum = reader.ReadSpectrum("", -1, true);
      if (spectrum.ScanNumber == 0) spectrum = reader.ReadSpectrum("", tmp, true);
      toolStripStatusLabel1.Text = spectrum.ScanNumber.ToString();
      UpdatePlot();
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
      if (spectrum == null) return;
      int tmp = spectrum.ScanNumber - 1;
      while (tmp > 0)
      {
        spectrum = reader.ReadSpectrum("", tmp, true);
        if (spectrum.ScanNumber == 0) tmp--;
        else break;
      }

      toolStripStatusLabel1.Text = spectrum.ScanNumber.ToString();
      UpdatePlot();
    }
  }
}
