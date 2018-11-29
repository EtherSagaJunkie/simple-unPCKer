namespace BeySoft
{
    partial class MainForm
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnExtract = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(93, 55);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(348, 54);
            this.lblStatus.TabIndex = 35;
            this.lblStatus.Text = "STATUS && INFO\r\nSTATUS && INFO\r\nSTATUS && INFO\r\nSTATUS && INFO";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(12, 70);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(75, 23);
            this.btnExtract.TabIndex = 34;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(447, 16);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 33;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(93, 18);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(348, 20);
            this.txtFileName.TabIndex = 32;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(12, 16);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 31;
            this.btnOpen.Text = "Open File";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Location = new System.Drawing.Point(439, 103);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(83, 13);
            this.lblCopyright.TabIndex = 36;
            this.lblCopyright.Text = "© BeySoft 2018";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(534, 125);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.btnOpen);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(550, 163);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(550, 163);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple UnPCKer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Label lblCopyright;
    }
}

