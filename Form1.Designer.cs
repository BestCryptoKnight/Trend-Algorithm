
namespace OracleAlpha
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.edOut = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.btnContinue = new System.Windows.Forms.Button();
      this.textBox3 = new System.Windows.Forms.TextBox();
      this.textBox2 = new System.Windows.Forms.TextBox();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      this.edStarting = new System.Windows.Forms.NumericUpDown();
      this.cbTrack = new System.Windows.Forms.CheckBox();
      this.cbStartCur = new System.Windows.Forms.ComboBox();
      this.edTradeHist = new System.Windows.Forms.TextBox();
      this.edLastPrice = new System.Windows.Forms.NumericUpDown();
      this.btnExit = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.edStarting)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edLastPrice)).BeginInit();
      this.SuspendLayout();
      // 
      // edOut
      // 
      this.edOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.edOut.Location = new System.Drawing.Point(405, 210);
      this.edOut.Margin = new System.Windows.Forms.Padding(2);
      this.edOut.Multiline = true;
      this.edOut.Name = "edOut";
      this.edOut.Size = new System.Drawing.Size(205, 79);
      this.edOut.TabIndex = 36;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.Color.RoyalBlue;
      this.label3.Location = new System.Drawing.Point(29, 84);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(152, 17);
      this.label3.TabIndex = 35;
      this.label3.Text = "Bittrex API Private Key:";
      this.label3.Visible = false;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.ForeColor = System.Drawing.Color.RoyalBlue;
      this.label2.Location = new System.Drawing.Point(34, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(146, 17);
      this.label2.TabIndex = 34;
      this.label2.Text = "Bittrex API Public Key:";
      this.label2.Visible = false;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.ForeColor = System.Drawing.Color.RoyalBlue;
      this.label1.Location = new System.Drawing.Point(21, 29);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(174, 17);
      this.label1.TabIndex = 33;
      this.label1.Text = "Password to lock api keys:";
      // 
      // btnContinue
      // 
      this.btnContinue.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnContinue.Location = new System.Drawing.Point(201, 140);
      this.btnContinue.Name = "btnContinue";
      this.btnContinue.Size = new System.Drawing.Size(75, 23);
      this.btnContinue.TabIndex = 32;
      this.btnContinue.Text = "&Continue";
      this.btnContinue.UseVisualStyleBackColor = true;
      this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
      // 
      // textBox3
      // 
      this.textBox3.Location = new System.Drawing.Point(201, 81);
      this.textBox3.Name = "textBox3";
      this.textBox3.Size = new System.Drawing.Size(214, 20);
      this.textBox3.TabIndex = 31;
      this.textBox3.Visible = false;
      // 
      // textBox2
      // 
      this.textBox2.Location = new System.Drawing.Point(201, 55);
      this.textBox2.Name = "textBox2";
      this.textBox2.Size = new System.Drawing.Size(214, 20);
      this.textBox2.TabIndex = 30;
      this.textBox2.Visible = false;
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(201, 29);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(214, 20);
      this.textBox1.TabIndex = 29;
      // 
      // timer1
      // 
      this.timer1.Interval = 250;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // edStarting
      // 
      this.edStarting.BackColor = System.Drawing.Color.Black;
      this.edStarting.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edStarting.DecimalPlaces = 8;
      this.edStarting.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edStarting.ForeColor = System.Drawing.Color.FloralWhite;
      this.edStarting.ImeMode = System.Windows.Forms.ImeMode.Off;
      this.edStarting.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
      this.edStarting.Location = new System.Drawing.Point(140, 108);
      this.edStarting.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edStarting.Name = "edStarting";
      this.edStarting.Size = new System.Drawing.Size(129, 19);
      this.edStarting.TabIndex = 37;
      this.edStarting.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.edStarting.Visible = false;
      this.edStarting.ValueChanged += new System.EventHandler(this.edStarting_ValueChanged);
      // 
      // cbTrack
      // 
      this.cbTrack.AutoSize = true;
      this.cbTrack.BackColor = System.Drawing.Color.Black;
      this.cbTrack.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cbTrack.ForeColor = System.Drawing.Color.White;
      this.cbTrack.Location = new System.Drawing.Point(24, 106);
      this.cbTrack.Margin = new System.Windows.Forms.Padding(2);
      this.cbTrack.Name = "cbTrack";
      this.cbTrack.Size = new System.Drawing.Size(119, 21);
      this.cbTrack.TabIndex = 38;
      this.cbTrack.Text = "Track Amount:";
      this.cbTrack.UseVisualStyleBackColor = false;
      this.cbTrack.CheckedChanged += new System.EventHandler(this.cbTrack_CheckedChanged);
      // 
      // cbStartCur
      // 
      this.cbStartCur.BackColor = System.Drawing.Color.Black;
      this.cbStartCur.ForeColor = System.Drawing.Color.White;
      this.cbStartCur.FormattingEnabled = true;
      this.cbStartCur.Items.AddRange(new object[] {
            "ADA",
            "ETH",
            "BTC",
            "USD"});
      this.cbStartCur.Location = new System.Drawing.Point(274, 108);
      this.cbStartCur.Margin = new System.Windows.Forms.Padding(2);
      this.cbStartCur.Name = "cbStartCur";
      this.cbStartCur.Size = new System.Drawing.Size(51, 21);
      this.cbStartCur.TabIndex = 39;
      this.cbStartCur.Text = "ADA";
      // 
      // edTradeHist
      // 
      this.edTradeHist.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.edTradeHist.Location = new System.Drawing.Point(0, 290);
      this.edTradeHist.Margin = new System.Windows.Forms.Padding(2);
      this.edTradeHist.Multiline = true;
      this.edTradeHist.Name = "edTradeHist";
      this.edTradeHist.Size = new System.Drawing.Size(611, 79);
      this.edTradeHist.TabIndex = 40;
      // 
      // edLastPrice
      // 
      this.edLastPrice.BackColor = System.Drawing.Color.Black;
      this.edLastPrice.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.edLastPrice.DecimalPlaces = 8;
      this.edLastPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.edLastPrice.ForeColor = System.Drawing.Color.FloralWhite;
      this.edLastPrice.ImeMode = System.Windows.Forms.ImeMode.Off;
      this.edLastPrice.Increment = new decimal(new int[] {
            1,
            0,
            0,
            524288});
      this.edLastPrice.Location = new System.Drawing.Point(328, 109);
      this.edLastPrice.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
      this.edLastPrice.Name = "edLastPrice";
      this.edLastPrice.Size = new System.Drawing.Size(129, 19);
      this.edLastPrice.TabIndex = 41;
      this.edLastPrice.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
      this.edLastPrice.Visible = false;
      // 
      // btnExit
      // 
      this.btnExit.BackColor = System.Drawing.Color.Firebrick;
      this.btnExit.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.btnExit.ForeColor = System.Drawing.SystemColors.ButtonFace;
      this.btnExit.Location = new System.Drawing.Point(463, 109);
      this.btnExit.Name = "btnExit";
      this.btnExit.Size = new System.Drawing.Size(83, 22);
      this.btnExit.TabIndex = 42;
      this.btnExit.Text = "Exit";
      this.btnExit.UseVisualStyleBackColor = false;
      this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(611, 369);
      this.Controls.Add(this.btnExit);
      this.Controls.Add(this.edLastPrice);
      this.Controls.Add(this.edTradeHist);
      this.Controls.Add(this.cbStartCur);
      this.Controls.Add(this.cbTrack);
      this.Controls.Add(this.edStarting);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.btnContinue);
      this.Controls.Add(this.textBox3);
      this.Controls.Add(this.textBox2);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.edOut);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "Form1";
      this.Text = "Crypto Rockets  ADA";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
      this.Load += new System.EventHandler(this.Form1_Load);
      this.Shown += new System.EventHandler(this.Form1_Shown);
      this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
      ((System.ComponentModel.ISupportInitialize)(this.edStarting)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edLastPrice)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox edOut;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnContinue;
    private System.Windows.Forms.TextBox textBox3;
    private System.Windows.Forms.TextBox textBox2;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Timer timer1;
    private System.Windows.Forms.NumericUpDown edStarting;
    private System.Windows.Forms.CheckBox cbTrack;
    private System.Windows.Forms.ComboBox cbStartCur;
    private System.Windows.Forms.TextBox edTradeHist;
    private System.Windows.Forms.NumericUpDown edLastPrice;
    private System.Windows.Forms.Button btnExit;
    }
}

