namespace AmongUs_proxy.GUI
{
    partial class MyMainMenu
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
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBoxHost = new System.Windows.Forms.GroupBox();
            this.labelStatusHost = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.groupBoxClient = new System.Windows.Forms.GroupBox();
            this.labelStatusClient = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.groupBoxHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.groupBoxClient.SuspendLayout();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 51);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(289, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Connect to the proxy server";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(84, 22);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(211, 23);
            this.textBox1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 80);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(289, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start the proxy server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBoxHost
            // 
            this.groupBoxHost.Controls.Add(this.labelStatusHost);
            this.groupBoxHost.Controls.Add(this.label2);
            this.groupBoxHost.Controls.Add(this.textBox2);
            this.groupBoxHost.Controls.Add(this.numericUpDown1);
            this.groupBoxHost.Controls.Add(this.label1);
            this.groupBoxHost.Controls.Add(this.comboBox1);
            this.groupBoxHost.Controls.Add(this.button1);
            this.groupBoxHost.Location = new System.Drawing.Point(12, 12);
            this.groupBoxHost.Name = "groupBoxHost";
            this.groupBoxHost.Size = new System.Drawing.Size(304, 133);
            this.groupBoxHost.TabIndex = 3;
            this.groupBoxHost.TabStop = false;
            this.groupBoxHost.Text = "Hosting";
            this.groupBoxHost.Visible = false;
            // 
            // labelStatusHost
            // 
            this.labelStatusHost.AutoSize = true;
            this.labelStatusHost.Location = new System.Drawing.Point(6, 106);
            this.labelStatusHost.Name = "labelStatusHost";
            this.labelStatusHost.Size = new System.Drawing.Size(39, 15);
            this.labelStatusHost.TabIndex = 9;
            this.labelStatusHost.Text = "Ready";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Room name";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(92, 51);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(134, 23);
            this.textBox2.TabIndex = 5;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(232, 22);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(66, 23);
            this.numericUpDown1.TabIndex = 6;
            this.numericUpDown1.Value = new decimal(new int[] {
            3070,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Bind interface";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(92, 22);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(134, 23);
            this.comboBox1.TabIndex = 5;
            // 
            // groupBoxClient
            // 
            this.groupBoxClient.Controls.Add(this.labelStatusClient);
            this.groupBoxClient.Controls.Add(this.label3);
            this.groupBoxClient.Controls.Add(this.button2);
            this.groupBoxClient.Controls.Add(this.textBox1);
            this.groupBoxClient.Location = new System.Drawing.Point(12, 12);
            this.groupBoxClient.Name = "groupBoxClient";
            this.groupBoxClient.Size = new System.Drawing.Size(304, 100);
            this.groupBoxClient.TabIndex = 4;
            this.groupBoxClient.TabStop = false;
            this.groupBoxClient.Text = "Client";
            this.groupBoxClient.Visible = false;
            // 
            // labelStatusClient
            // 
            this.labelStatusClient.AutoSize = true;
            this.labelStatusClient.Location = new System.Drawing.Point(6, 77);
            this.labelStatusClient.Name = "labelStatusClient";
            this.labelStatusClient.Size = new System.Drawing.Size(39, 15);
            this.labelStatusClient.TabIndex = 4;
            this.labelStatusClient.Text = "Ready";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Proxy Server";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(38, 46);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(254, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "I want to host the game";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(38, 85);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(254, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "I want to connect to someone\'s host";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(12, 151);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(304, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Back";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Visible = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // MyMainMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 190);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.groupBoxClient);
            this.Controls.Add(this.groupBoxHost);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MyMainMenu";
            this.Text = "Among Us Proxy";
            this.Shown += new System.EventHandler(this.MyMainMenu_Shown);
            this.groupBoxHost.ResumeLayout(false);
            this.groupBoxHost.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.groupBoxClient.ResumeLayout(false);
            this.groupBoxClient.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBoxHost;
        private System.Windows.Forms.GroupBox groupBoxClient;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelStatusHost;
        private System.Windows.Forms.Label labelStatusClient;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}

