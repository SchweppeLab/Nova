namespace ScanBroadcaster
{
  partial class ScanBroadcaster
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
      openFileDialog1 = new OpenFileDialog();
      button1 = new Button();
      label1 = new Label();
      button2 = new Button();
      label2 = new Label();
      richTextBox1 = new RichTextBox();
      SuspendLayout();
      // 
      // openFileDialog1
      // 
      openFileDialog1.FileName = "openFileDialog1";
      openFileDialog1.Filter = "All files|*.*|mzML|*.mzML|mzXML|*.mzXML|Thermo Raw|*.raw";
      // 
      // button1
      // 
      button1.Location = new Point(12, 12);
      button1.Name = "button1";
      button1.Size = new Size(120, 40);
      button1.TabIndex = 0;
      button1.Text = "Select File";
      button1.UseVisualStyleBackColor = true;
      button1.Click += button1_Click;
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new Point(138, 20);
      label1.Name = "label1";
      label1.Size = new Size(140, 25);
      label1.TabIndex = 1;
      label1.Text = "(no file selected)";
      // 
      // button2
      // 
      button2.Enabled = false;
      button2.Location = new Point(12, 58);
      button2.Name = "button2";
      button2.Size = new Size(120, 40);
      button2.TabIndex = 2;
      button2.Text = "Broadcast";
      button2.UseVisualStyleBackColor = true;
      button2.Click += button2_Click;
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new Point(12, 134);
      label2.Name = "label2";
      label2.Size = new Size(129, 25);
      label2.TabIndex = 3;
      label2.Text = "Connections: 0";
      // 
      // richTextBox1
      // 
      richTextBox1.Location = new Point(12, 162);
      richTextBox1.Name = "richTextBox1";
      richTextBox1.Size = new Size(776, 276);
      richTextBox1.TabIndex = 4;
      richTextBox1.Text = "";
      // 
      // ScanBroadcaster
      // 
      AutoScaleDimensions = new SizeF(10F, 25F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(800, 450);
      Controls.Add(richTextBox1);
      Controls.Add(label2);
      Controls.Add(button2);
      Controls.Add(label1);
      Controls.Add(button1);
      Name = "ScanBroadcaster";
      Text = "ScanBroadcaster";
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private OpenFileDialog openFileDialog1;
    private Button button1;
    private Label label1;
    private Button button2;
    private Label label2;
    private RichTextBox richTextBox1;
  }
}
