namespace Ruler.Generator
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
            this.zip = new System.Windows.Forms.GroupBox();
            this.ziptxt = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.certpwtxt = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.updatertxt = new System.Windows.Forms.TextBox();
            this.rulertxt = new System.Windows.Forms.TextBox();
            this.pfxtxt = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.vertxt = new System.Windows.Forms.TextBox();
            this.pbStatus = new System.Windows.Forms.ProgressBar();
            this.lblStep = new System.Windows.Forms.Label();
            this.zip.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // zip
            // 
            this.zip.Controls.Add(this.ziptxt);
            this.zip.Controls.Add(this.button1);
            this.zip.Controls.Add(this.label1);
            this.zip.Location = new System.Drawing.Point(397, 22);
            this.zip.Name = "zip";
            this.zip.Size = new System.Drawing.Size(347, 266);
            this.zip.TabIndex = 0;
            this.zip.TabStop = false;
            this.zip.Text = "Sign Zip File";
            // 
            // ziptxt
            // 
            this.ziptxt.Enabled = false;
            this.ziptxt.Location = new System.Drawing.Point(100, 52);
            this.ziptxt.Multiline = true;
            this.ziptxt.Name = "ziptxt";
            this.ziptxt.Size = new System.Drawing.Size(100, 20);
            this.ziptxt.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(221, 50);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Load Zip file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Zip File";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.vertxt);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.certpwtxt);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.updatertxt);
            this.groupBox2.Controls.Add(this.rulertxt);
            this.groupBox2.Controls.Add(this.pfxtxt);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(356, 242);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sign Executables";
            // 
            // certpwtxt
            // 
            this.certpwtxt.Enabled = false;
            this.certpwtxt.Location = new System.Drawing.Point(138, 79);
            this.certpwtxt.Multiline = true;
            this.certpwtxt.Name = "certpwtxt";
            this.certpwtxt.Size = new System.Drawing.Size(212, 20);
            this.certpwtxt.TabIndex = 11;
            this.certpwtxt.TextChanged += new System.EventHandler(this.certpwtxt_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Certificate  Password";
            // 
            // updatertxt
            // 
            this.updatertxt.Enabled = false;
            this.updatertxt.Location = new System.Drawing.Point(110, 156);
            this.updatertxt.Multiline = true;
            this.updatertxt.Name = "updatertxt";
            this.updatertxt.Size = new System.Drawing.Size(100, 20);
            this.updatertxt.TabIndex = 9;
            // 
            // rulertxt
            // 
            this.rulertxt.Enabled = false;
            this.rulertxt.Location = new System.Drawing.Point(110, 112);
            this.rulertxt.Multiline = true;
            this.rulertxt.Name = "rulertxt";
            this.rulertxt.Size = new System.Drawing.Size(100, 20);
            this.rulertxt.TabIndex = 8;
            // 
            // pfxtxt
            // 
            this.pfxtxt.Enabled = false;
            this.pfxtxt.Location = new System.Drawing.Point(110, 56);
            this.pfxtxt.Multiline = true;
            this.pfxtxt.Name = "pfxtxt";
            this.pfxtxt.Size = new System.Drawing.Size(100, 20);
            this.pfxtxt.TabIndex = 7;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(228, 153);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(122, 23);
            this.button6.TabIndex = 5;
            this.button6.Text = "Load Updater App";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(228, 110);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(122, 23);
            this.button5.TabIndex = 4;
            this.button5.Text = "Load Ruler App";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(228, 53);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(122, 23);
            this.button4.TabIndex = 3;
            this.button4.Text = "Load Certiicate File";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 163);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(67, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Updater App";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Ruler App";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Certificate File";
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(88, 398);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(202, 23);
            this.button8.TabIndex = 2;
            this.button8.Text = "Generate Manifest and signature file File";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(81, 307);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(209, 23);
            this.button9.TabIndex = 3;
            this.button9.Text = "Generate a new self signed cetificate";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(81, 353);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(209, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Generate new RSA certificate";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 197);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "App Version";
            // 
            // vertxt
            // 
            this.vertxt.Location = new System.Drawing.Point(110, 197);
            this.vertxt.Multiline = true;
            this.vertxt.Name = "vertxt";
            this.vertxt.Size = new System.Drawing.Size(100, 20);
            this.vertxt.TabIndex = 13;
            // 
            // pbStatus
            // 
            this.pbStatus.Location = new System.Drawing.Point(362, 353);
            this.pbStatus.Maximum = 7;
            this.pbStatus.Name = "pbStatus";
            this.pbStatus.Size = new System.Drawing.Size(350, 23);
            this.pbStatus.Step = 1;
            this.pbStatus.TabIndex = 5;
            // 
            // lblStep
            // 
            this.lblStep.AutoSize = true;
            this.lblStep.Location = new System.Drawing.Point(423, 317);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(0, 13);
            this.lblStep.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblStep);
            this.Controls.Add(this.pbStatus);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.zip);
            this.Name = "Form1";
            this.Text = "Form1";
            this.zip.ResumeLayout(false);
            this.zip.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox zip;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.TextBox updatertxt;
        private System.Windows.Forms.TextBox rulertxt;
        private System.Windows.Forms.TextBox pfxtxt;
        private System.Windows.Forms.TextBox ziptxt;
        private System.Windows.Forms.TextBox certpwtxt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox vertxt;
        private System.Windows.Forms.ProgressBar pbStatus;
        private System.Windows.Forms.Label lblStep;
    }
}

