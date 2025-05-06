namespace Protostar
{
  partial class ProtostarForm
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProtostarForm));
      toolStrip1 = new ToolStrip();
      toolStripButton1 = new ToolStripButton();
      toolStripButton2 = new ToolStripButton();
      toolStripComboBox1 = new ToolStripComboBox();
      statusStrip1 = new StatusStrip();
      openFileDialog1 = new OpenFileDialog();
      splitContainer1 = new SplitContainer();
      listBox1 = new ListBox();
      splitContainer2 = new SplitContainer();
      plot1 = new ScottPlot.WinForms.FormsPlot();
      label1 = new Label();
      panel1 = new Panel();
      label3 = new Label();
      label2 = new Label();
      button4 = new Button();
      imageList1 = new ImageList(components);
      button3 = new Button();
      comboBox1 = new ComboBox();
      textBox1 = new TextBox();
      textBox2 = new TextBox();
      button2 = new Button();
      button1 = new Button();
      richTextBox1 = new RichTextBox();
      toolStrip1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
      splitContainer2.Panel1.SuspendLayout();
      splitContainer2.Panel2.SuspendLayout();
      splitContainer2.SuspendLayout();
      panel1.SuspendLayout();
      SuspendLayout();
      // 
      // toolStrip1
      // 
      toolStrip1.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      toolStrip1.ImageScalingSize = new Size(32, 32);
      toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripComboBox1 });
      toolStrip1.Location = new Point(0, 0);
      toolStrip1.Name = "toolStrip1";
      toolStrip1.Size = new Size(1178, 47);
      toolStrip1.TabIndex = 0;
      toolStrip1.Text = "toolStrip1";
      // 
      // toolStripButton1
      // 
      toolStripButton1.AutoSize = false;
      toolStripButton1.BackgroundImageLayout = ImageLayout.Stretch;
      toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButton1.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
      toolStripButton1.ImageTransparentColor = Color.Magenta;
      toolStripButton1.Name = "toolStripButton1";
      toolStripButton1.Size = new Size(42, 42);
      toolStripButton1.Text = "Open File";
      toolStripButton1.Click += toolStripButton1_Click;
      // 
      // toolStripButton2
      // 
      toolStripButton2.AutoSize = false;
      toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButton2.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
      toolStripButton2.ImageTransparentColor = Color.Magenta;
      toolStripButton2.Name = "toolStripButton2";
      toolStripButton2.Size = new Size(42, 42);
      toolStripButton2.Text = "Close File";
      toolStripButton2.Click += toolStripButton2_Click;
      // 
      // toolStripComboBox1
      // 
      toolStripComboBox1.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      toolStripComboBox1.Items.AddRange(new object[] { "Chromat", "Spectrum" });
      toolStripComboBox1.Name = "toolStripComboBox1";
      toolStripComboBox1.Size = new Size(121, 47);
      toolStripComboBox1.SelectedIndexChanged += toolStripComboBox1_SelectedIndexChanged;
      // 
      // statusStrip1
      // 
      statusStrip1.ImageScalingSize = new Size(24, 24);
      statusStrip1.Location = new Point(0, 822);
      statusStrip1.Name = "statusStrip1";
      statusStrip1.Size = new Size(1178, 22);
      statusStrip1.TabIndex = 1;
      statusStrip1.Text = "statusStrip1";
      // 
      // openFileDialog1
      // 
      openFileDialog1.FileName = "openFileDialog1";
      openFileDialog1.Filter = "Thermo Raw|*.raw|MzML|*.mzML|MzXML|*.mzXML|All files|*.*";
      openFileDialog1.Multiselect = true;
      // 
      // splitContainer1
      // 
      splitContainer1.Dock = DockStyle.Fill;
      splitContainer1.FixedPanel = FixedPanel.Panel1;
      splitContainer1.IsSplitterFixed = true;
      splitContainer1.Location = new Point(0, 47);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(listBox1);
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(splitContainer2);
      splitContainer1.Size = new Size(1178, 775);
      splitContainer1.SplitterDistance = 384;
      splitContainer1.TabIndex = 2;
      // 
      // listBox1
      // 
      listBox1.Dock = DockStyle.Fill;
      listBox1.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      listBox1.FormattingEnabled = true;
      listBox1.HorizontalScrollbar = true;
      listBox1.Location = new Point(0, 0);
      listBox1.Name = "listBox1";
      listBox1.SelectionMode = SelectionMode.MultiExtended;
      listBox1.Size = new Size(384, 775);
      listBox1.TabIndex = 0;
      listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
      // 
      // splitContainer2
      // 
      splitContainer2.Dock = DockStyle.Fill;
      splitContainer2.Location = new Point(0, 0);
      splitContainer2.Name = "splitContainer2";
      splitContainer2.Orientation = Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      splitContainer2.Panel1.Controls.Add(plot1);
      splitContainer2.Panel1.Controls.Add(label1);
      splitContainer2.Panel1.Controls.Add(panel1);
      splitContainer2.Panel1.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      // 
      // splitContainer2.Panel2
      // 
      splitContainer2.Panel2.Controls.Add(richTextBox1);
      splitContainer2.Size = new Size(790, 775);
      splitContainer2.SplitterDistance = 538;
      splitContainer2.TabIndex = 0;
      // 
      // plot1
      // 
      plot1.DisplayScale = 1.5F;
      plot1.Dock = DockStyle.Fill;
      plot1.Location = new Point(0, 96);
      plot1.Name = "plot1";
      plot1.Size = new Size(790, 442);
      plot1.TabIndex = 0;
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Dock = DockStyle.Top;
      label1.Location = new Point(0, 72);
      label1.Name = "label1";
      label1.Size = new Size(0, 24);
      label1.TabIndex = 2;
      // 
      // panel1
      // 
      panel1.Controls.Add(label3);
      panel1.Controls.Add(label2);
      panel1.Controls.Add(button4);
      panel1.Controls.Add(button3);
      panel1.Controls.Add(comboBox1);
      panel1.Controls.Add(textBox1);
      panel1.Controls.Add(textBox2);
      panel1.Controls.Add(button2);
      panel1.Controls.Add(button1);
      panel1.Dock = DockStyle.Top;
      panel1.Location = new Point(0, 0);
      panel1.Name = "panel1";
      panel1.Size = new Size(790, 72);
      panel1.TabIndex = 1;
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new Point(422, 44);
      label3.Name = "label3";
      label3.Size = new Size(0, 24);
      label3.TabIndex = 11;
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      label2.Location = new Point(3, 44);
      label2.Name = "label2";
      label2.Size = new Size(105, 24);
      label2.TabIndex = 10;
      label2.Text = "0 of 0 Scans";
      // 
      // button4
      // 
      button4.BackgroundImageLayout = ImageLayout.Stretch;
      button4.Enabled = false;
      button4.ImageIndex = 2;
      button4.ImageList = imageList1;
      button4.Location = new Point(233, 3);
      button4.Name = "button4";
      button4.Size = new Size(42, 42);
      button4.TabIndex = 9;
      button4.UseVisualStyleBackColor = true;
      button4.Click += button4_Click;
      // 
      // imageList1
      // 
      imageList1.ColorDepth = ColorDepth.Depth32Bit;
      imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
      imageList1.TransparentColor = Color.Transparent;
      imageList1.Images.SetKeyName(0, "arrow_back_2_32dp_434343_FILL0_wght400_GRAD0_opsz40.png");
      imageList1.Images.SetKeyName(1, "play_arrow_32dp_434343_FILL0_wght400_GRAD0_opsz40.png");
      imageList1.Images.SetKeyName(2, "jump_to_element_32dp_434343_FILL0_wght400_GRAD0_opsz40.png");
      imageList1.Images.SetKeyName(3, "file_open_32dp_434343_FILL0_wght400_GRAD0_opsz40.png");
      imageList1.Images.SetKeyName(4, "scan_delete_32dp_434343_FILL0_wght400_GRAD0_opsz40.png");
      // 
      // button3
      // 
      button3.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      button3.Location = new Point(615, 3);
      button3.Name = "button3";
      button3.Size = new Size(80, 40);
      button3.TabIndex = 6;
      button3.Text = "Match";
      button3.UseVisualStyleBackColor = true;
      button3.Click += button3_Click;
      // 
      // comboBox1
      // 
      comboBox1.FormattingEnabled = true;
      comboBox1.Items.AddRange(new object[] { "2+", "3+", "4+" });
      comboBox1.Location = new Point(353, 36);
      comboBox1.Name = "comboBox1";
      comboBox1.Size = new Size(63, 32);
      comboBox1.TabIndex = 5;
      comboBox1.Text = "2+";
      // 
      // textBox1
      // 
      textBox1.CharacterCasing = CharacterCasing.Upper;
      textBox1.ForeColor = SystemColors.InactiveCaption;
      textBox1.Location = new Point(353, 3);
      textBox1.Name = "textBox1";
      textBox1.Size = new Size(256, 31);
      textBox1.TabIndex = 4;
      textBox1.Text = "ENTER PEPTIDE...";
      textBox1.TextChanged += textBox1_TextChanged;
      // 
      // textBox2
      // 
      textBox2.ForeColor = SystemColors.InactiveCaption;
      textBox2.Location = new Point(99, 9);
      textBox2.Name = "textBox2";
      textBox2.Size = new Size(128, 31);
      textBox2.TabIndex = 8;
      textBox2.Text = "skip to scan...";
      textBox2.KeyPress += textBox2_KeyPress;
      // 
      // button2
      // 
      button2.BackgroundImageLayout = ImageLayout.Stretch;
      button2.ImageIndex = 1;
      button2.ImageList = imageList1;
      button2.Location = new Point(51, 3);
      button2.Name = "button2";
      button2.Size = new Size(42, 42);
      button2.TabIndex = 1;
      button2.UseVisualStyleBackColor = true;
      button2.Click += button2_Click;
      // 
      // button1
      // 
      button1.BackgroundImageLayout = ImageLayout.Stretch;
      button1.ImageIndex = 0;
      button1.ImageList = imageList1;
      button1.Location = new Point(3, 3);
      button1.Name = "button1";
      button1.Size = new Size(42, 42);
      button1.TabIndex = 0;
      button1.UseVisualStyleBackColor = true;
      button1.Click += button1_Click;
      // 
      // richTextBox1
      // 
      richTextBox1.Dock = DockStyle.Fill;
      richTextBox1.Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      richTextBox1.Location = new Point(0, 0);
      richTextBox1.Name = "richTextBox1";
      richTextBox1.Size = new Size(790, 233);
      richTextBox1.TabIndex = 0;
      richTextBox1.Text = "";
      // 
      // ProtostarForm
      // 
      AutoScaleDimensions = new SizeF(144F, 144F);
      AutoScaleMode = AutoScaleMode.Dpi;
      ClientSize = new Size(1178, 844);
      Controls.Add(splitContainer1);
      Controls.Add(statusStrip1);
      Controls.Add(toolStrip1);
      Font = new Font("Segoe UI Variable Display", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
      Name = "ProtostarForm";
      Text = "Protostar";
      toolStrip1.ResumeLayout(false);
      toolStrip1.PerformLayout();
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      splitContainer2.Panel1.ResumeLayout(false);
      splitContainer2.Panel1.PerformLayout();
      splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
      splitContainer2.ResumeLayout(false);
      panel1.ResumeLayout(false);
      panel1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private ToolStrip toolStrip1;
    private StatusStrip statusStrip1;
    private ToolStripButton toolStripButton1;
    private OpenFileDialog openFileDialog1;
    private SplitContainer splitContainer1;
    private ListBox listBox1;
    private SplitContainer splitContainer2;
    private ScottPlot.WinForms.FormsPlot plot1;
    private RichTextBox richTextBox1;
    private Panel panel1;
    private Button button1;
    private ToolStripComboBox toolStripComboBox1;
    private Button button2;
    private Label label1;
    private Button button3;
    private ComboBox comboBox1;
    private TextBox textBox1;
    private TextBox textBox2;
    private Button button4;
    private Label label2;
    private ImageList imageList1;
    private Label label3;
    private ToolStripButton toolStripButton2;
  }
}
