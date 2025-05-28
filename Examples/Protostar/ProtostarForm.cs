using System.Collections.Generic;
using System.Windows.Forms;
using Nova.Data;
using Nova.Io.Read;
using Nova.Io.Write;
using ScottPlot;
using ThermoFisher.CommonCore.Data.Business;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Protostar
{
  public partial class ProtostarForm : Form
  {
    List<ProtostarFile> Files = new List<ProtostarFile>();
    bool RefreshPlot = false;
    bool ChromatMode = true;
    bool IsReplotting = false;
    ScottPlot.Palettes.Nord pal = new ScottPlot.Palettes.Nord();
    double[] AA = new double[128];

    readonly System.Windows.Forms.Timer UpdatePlotTimer = new() { Interval = 100, Enabled = true };

    public ProtostarForm()
    {
      InitializeComponent();
      toolStripComboBox1.SelectedIndex = 0;
      panel1.Visible = false;
      panel1.Enabled = false;

      textBox1.Enter += new EventHandler(textBox1_Enter);
      textBox1.Leave += new EventHandler(textBox1_Leave);
      textBox2.Enter += new EventHandler(textBox2_Enter);
      textBox2.Leave += new EventHandler(textBox2_Leave);

      AA['G'] = AA['g'] = 57.0214636;
      AA['A'] = AA['a'] = 71.0371136;
      AA['S'] = AA['s'] = 87.0320282;
      AA['P'] = AA['p'] = 97.0527636;
      AA['V'] = AA['v'] = 99.0684136;
      AA['T'] = AA['t'] = 101.0476782;
      AA['C'] = AA['c'] = 103.0091854 + 57.02146;
      AA['L'] = AA['l'] = 113.0840636;
      AA['I'] = AA['i'] = 113.0840636;
      AA['N'] = AA['n'] = 114.0429272;
      AA['D'] = AA['d'] = 115.0269428;
      AA['Q'] = AA['q'] = 128.0585772;
      AA['K'] = AA['k'] = 128.0949626;
      AA['E'] = AA['e'] = 129.0425928;
      AA['M'] = AA['m'] = 131.0404854;
      AA['H'] = AA['h'] = 137.0589116;
      AA['F'] = AA['f'] = 147.0684136;
      AA['R'] = AA['r'] = 156.1011106;
      AA['Y'] = AA['y'] = 163.0633282;
      AA['W'] = AA['w'] = 186.0793126;

      plot1.Plot.Add.Palette = pal;
      plot1.Plot.HideGrid();
      plot1.Plot.Axes.SetLimitsY(0, 100);
      plot1.Plot.Axes.Bottom.Label.FontSize = 24;
      plot1.Plot.Axes.Left.Label.FontSize = 24;
      plot1.Plot.Axes.Bottom.TickLabelStyle.FontSize = 18;
      plot1.Plot.Axes.Left.TickLabelStyle.FontSize = 18;

      UpdatePlotTimer.Tick += (s, e) =>
      {
        if (RefreshPlot)
        {
          plot1.Refresh();
          RefreshPlot = false;
        }
      };
    }

    private void Log(string msg)
    {
      richTextBox1.Text += msg + Environment.NewLine;
    }

    private double[] MakeB(string pep)
    {
      double m = 0;// 1.007825;
      double[] bion = new double[pep.Length - 1];
      for (int i = 0; i < pep.Length - 1; i++)
      {
        m += AA[pep[i]];
        bion[i] = m;
      }
      return bion;
    }

    private double[] MakeY(string pep)
    {
      double m = 18.0105646;
      double[] yion = new double[pep.Length - 1];
      int a = 0;
      for (int i = pep.Length - 1; i > 0; i--)
      {
        m += AA[pep[i]];
        yion[a++] = m;
      }
      return yion;
    }

    //This is a slow function, so make sure the UpdatePlotTimer doesn't try to call this function while
    //we are already in it?
    private void Replot()
    {
      //make sure all items are loaded. If not, stop now, and this function will be called again
      //when another file loads.
      foreach (ProtostarFile o in listBox1.SelectedItems)
      {
        if (!o.IsLoaded) return;
      }

      IsReplotting = true;
      lock (plot1.Plot.Sync)
      {
        plot1.Plot.Clear();
      }

      double max = 0;
      foreach (ProtostarFile o in listBox1.SelectedItems)
      {
        if (ChromatMode)
        {
          if (!o.IsLoaded || o.Chromat == null || o.Chromat.Count==0) continue;
          lock (plot1.Plot.Sync)
          {
            double[] x = new double[o.Chromat.Count];
            double[] y = new double[o.Chromat.Count];
            for (int a = 0; a < o.Chromat.Count; a++)
            {
              x[a] = o.Chromat.DataPoints[a].RT;
              y[a] = o.Chromat.DataPoints[a].Intensity;
              if (y[a] > max) max = y[a];
            }
            var scat = plot1.Plot.Add.Scatter(x, y);
            scat.Color = pal.GetColor(listBox1.Items.IndexOf(o));
            scat.MarkerSize = 1;
            plot1.Plot.YLabel("Intensity");
            plot1.Plot.XLabel("Time (min)");
            plot1.Plot.Axes.SetupMultiplierNotation(plot1.Plot.Axes.Left);
            plot1.Plot.Axes.SetLimitsY(0, max);
            plot1.Plot.Axes.SetLimitsX(0, o.Chromat.DataPoints[o.Chromat.Count - 1].RT);
          }
        }
        else
        {
          if (o.Scan == null) continue;
          double[] x = new double[o.Scan.Count * 3 + 2];
          double[] y = new double[o.Scan.Count * 3 + 2];
          int a = 0;
          x[a] = o.Scan.StartMz;
          y[a++] = 0;
          for (int i = 0; i < o.Scan.DataPoints.Count(); i++)
          {
            //Log(qs.mz[i].ToString()+" " + (qs.intensity[i] / qs.maxIntensity * 100).ToString());
            //centroided peaks need to be plotted with 3 points 
            x[a] = o.Scan.DataPoints[i].Mz;
            y[a++] = 0;
            x[a] = o.Scan.DataPoints[i].Mz;
            y[a++] = o.Scan.DataPoints[i].Intensity / o.Scan.BasePeakIntensity * 100;
            x[a] = o.Scan.DataPoints[i].Mz;
            y[a++] = 0;
          }
          x[a] = o.Scan.EndMz;
          y[a] = 0;

          lock (plot1.Plot.Sync)
          {
            var scat = plot1.Plot.Add.Scatter(x, y);
            scat.Color = pal.GetColor(listBox1.Items.IndexOf(o));
            scat.MarkerSize = 1;
            plot1.Plot.Axes.SetLimitsX(o.Scan.StartMz, o.Scan.EndMz);
            plot1.Plot.YLabel("Relative Intensity");
            plot1.Plot.XLabel("m/z");
            plot1.Plot.Axes.SetLimitsY(0, 100);
          }
        }
      }
      plot1.Plot.Axes.AutoScale();
      RefreshPlot = true;
      IsReplotting = false;
    }

    private void StopChromatogram(object? sender, EventArgs e)
    {
      if (sender != null)
      {
        ((ProtostarFile)sender).DoneLoading();
        if (!ChromatMode && listBox1.SelectedItem != null && listBox1.SelectedItem == sender)
        {
          label1.Text = ((ProtostarFile)sender).Scan.ScanFilter;
          label2.Text = ((ProtostarFile)sender).Scan.ScanNumber.ToString() + " of " + ((ProtostarFile)sender).ScanCount.ToString() + " Scans";
          label3.Text = "";
          Replot();
        }
      }

      if (ChromatMode) Replot();
    }

    private async void toolStripButton1_Click(object sender, EventArgs e)
    {

      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        List<Task> tasks = new List<Task>();
        listBox1.SelectedIndices.Clear();
        foreach (string file in openFileDialog1.FileNames)
        {
          bool match = false;
          foreach (ProtostarFile obj in listBox1.Items)
          {
            if (obj.Filename == file)
            {
              match = true;
              break;
            }
          }
          if (match) continue;
          int index = listBox1.Items.Add(new ProtostarFile(file));
          listBox1.SelectedIndices.Add(index);
          ((ProtostarFile)listBox1.Items[index]).FinishedLoading += StopChromatogram;
          var t = new Task(() => ((ProtostarFile)listBox1.Items[index]).LoadFile());
          t.Start();
          tasks.Add(t);
        }
        await Task.WhenAll(tasks.ToArray());
      }
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!ChromatMode)
      {
        if (listBox1.SelectedItem != null)
        {
          if (!((ProtostarFile)listBox1.SelectedItem).IsLoaded) return;
          label1.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanFilter;
          label2.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanNumber.ToString() + " of " + ((ProtostarFile)listBox1.SelectedItem).ScanCount.ToString() + " Scans";
          label3.Text = "";
        }
      }
      if (listBox1.SelectedItems.Count > 0)
      {
        toolStripButton2.Enabled = true;
      } else
      {
        toolStripButton2.Enabled = false;
      }
      Replot();
    }

    private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (toolStripComboBox1.SelectedItem.ToString() == "Spectrum")
      {
        panel1.Enabled = true;
        panel1.Visible = true;
        label1.Enabled = true;
        label1.Visible = true;
        listBox1.SelectionMode = SelectionMode.One;
        ChromatMode = false;
        //Log(((ProtostarFile)listBox1.SelectedItem).ScanCount.ToString());
        //Log(((ProtostarFile)listBox1.SelectedItem).Scan.ScanNumber.ToString());
        //Log( ((ProtostarFile)listBox1.SelectedItem).Scan.Count.ToString() );  
        if (listBox1.SelectedItem != null)
        {
          if (((ProtostarFile)listBox1.SelectedItem).IsLoaded)
          {
            label1.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanFilter;
            label2.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanNumber.ToString() + " of " + ((ProtostarFile)listBox1.SelectedItem).ScanCount.ToString() + " Scans";
            label3.Text = "";
          }
        }
      }
      else
      {
        panel1.Enabled = false;
        panel1.Visible = false;
        label1.Enabled = false;
        label1.Visible = false;
        listBox1.SelectionMode = SelectionMode.MultiExtended;
        ChromatMode = true;
      }
      Replot();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (listBox1.SelectedItem != null)
      {
        if (!((ProtostarFile)listBox1.SelectedItem).IsLoaded) return;
        ((ProtostarFile)listBox1.SelectedItem).PrevScan();
        label1.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanFilter;
        label2.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanNumber.ToString() + " of " + ((ProtostarFile)listBox1.SelectedItem).ScanCount.ToString() + " Scans";
        label3.Text = "";
        Replot();
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      if (listBox1.SelectedItem != null)
      {
        if (!((ProtostarFile)listBox1.SelectedItem).IsLoaded) return;
        ((ProtostarFile)listBox1.SelectedItem).NextScan();
        label1.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanFilter;
        label2.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanNumber.ToString() + " of " + ((ProtostarFile)listBox1.SelectedItem).ScanCount.ToString() + " Scans";
        label3.Text = "";
        Replot();
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      ProtostarFile? active = (ProtostarFile)listBox1.SelectedItem;
      if (active == null) return;
      SpectrumEx? scan = active.Scan;
      if (scan == null) return;

      //Make b and y ions
      double[] bion = MakeB(textBox1.Text);
      double[] yion = MakeY(textBox1.Text);
      int z = comboBox1.SelectedIndex + 1;

      List<double> ions = new List<double>();

      Replot();
      lock (plot1.Plot.Sync)
      {
        for (int a = 1; a <= z; a++)
        {
          double[] x = new double[bion.Length * 3 + 2];
          double[] y = new double[bion.Length * 3 + 2];
          int i = 0;
          x[i] = 0;
          y[i++] = 0;
          foreach (double d in bion)
          {
            double mz = (d + 1.007276466 * a) / a;
            //if(mz>scan.LowestMz-1 && mz<scan.HighestMz+1) ions.Add(mz);
            ions.Add(mz);
            x[i] = mz;
            y[i++] = 0;
            x[i] = mz;
            y[i++] = -40 + 5 * a;
            x[i] = mz;
            y[i++] = 0;
            var scat = plot1.Plot.Add.Scatter(x, y);
            scat.MarkerSize = 1;
          }
          x[i] = x[i - 1] + 1;
          y[i] = 0;

        }

        for (int a = 1; a <= z; a++)
        {
          double[] x = new double[yion.Length * 3 + 2];
          double[] y = new double[yion.Length * 3 + 2];
          int i = 0;
          x[i] = 0;
          y[i++] = 0;
          foreach (double d in yion)
          {
            double mz = (d + 1.007276466 * a) / a;
            //if (mz > scan.LowestMz - 1 && mz < scan.HighestMz + 1) ions.Add(mz);
            ions.Add(mz);
            x[i] = mz;
            y[i++] = 0;
            x[i] = mz;
            y[i++] = -40 + 5 * a;
            x[i] = mz;
            y[i++] = 0;
            var scat = plot1.Plot.Add.Scatter(x, y);
            scat.MarkerSize = 1;
          }
          x[i] = x[i - 1] + 1;
          y[i] = 0;
        }

        plot1.Plot.Axes.SetLimitsX(scan.StartMz, scan.EndMz);
        plot1.Plot.YLabel("Relative Intensity");
        plot1.Plot.XLabel("m/z");
        plot1.Plot.Axes.SetLimitsY(-40, 100);

      }
      RefreshPlot = true;

      //Make xcorr??
      double xcorr = active.XCorr(ions);
      label3.Text = "XCorr = " + xcorr.ToString();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      //if (!System.Text.RegularExpressions.Regex.IsMatch(textBox1.Text, "^[A-Z ]"))
      if (!textBox1.Text.All(chr => char.IsLetter(chr)))
      {
        string newText = string.Empty;
        foreach (char chr in textBox1.Text)
        {
          if (char.IsLetter(chr)) newText += chr;
        }
        textBox1.Text = newText;
        textBox1.SelectionStart = textBox1.Text.Length;
      }

    }

    private void textBox1_Enter(object sender, EventArgs e)
    {
      if (textBox1.ForeColor == System.Drawing.Color.Black) return;
      textBox1.Text = "";
      textBox1.ForeColor = System.Drawing.Color.Black;
    }
    private void textBox1_Leave(object sender, EventArgs e)
    {
      if (textBox1.Text.Trim() == "")
      {
        textBox1.Text = "Enter Peptide...";
        textBox1.ForeColor = System.Drawing.Color.Gray;
      }
    }

    private void textBox2_Enter(object sender, EventArgs e)
    {
      if (textBox2.ForeColor == System.Drawing.Color.Black) return;
      textBox2.Text = "";
      textBox2.ForeColor = System.Drawing.Color.Black;
    }
    private void textBox2_Leave(object sender, EventArgs e)
    {
      if (textBox2.Text.Trim() == "")
      {
        textBox2.Text = "skip to scan...";
        textBox2.ForeColor = System.Drawing.Color.Gray;
        button4.Enabled = false;
      }
    }

    private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
      {
        e.Handled = true;
      }
      if (textBox2.Text.Length > 0) button4.Enabled = true;
      else button4.Enabled = false;
    }

    private void button4_Click(object sender, EventArgs e)
    {
      int scanNum = Convert.ToInt32(textBox2.Text);
      if (listBox1.SelectedItem != null)
      {
        if (!((ProtostarFile)listBox1.SelectedItem).IsLoaded) return;
        ((ProtostarFile)listBox1.SelectedItem).NextScan(scanNum);
        label1.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanFilter;
        label2.Text = ((ProtostarFile)listBox1.SelectedItem).Scan.ScanNumber.ToString() + " of " + ((ProtostarFile)listBox1.SelectedItem).ScanCount.ToString() + " Scans";
        label3.Text = "";
        Replot();
      }
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
      while (listBox1.SelectedItems.Count > 0)
      {
        listBox1.Items.Remove(listBox1.SelectedItems[0]);
      }
    }
  }
}
