namespace Senior_Project
{
    partial class frmPlayHelp
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
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.picHelpPic = new System.Windows.Forms.PictureBox();
            this.lblHelpText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picHelpPic)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRight
            // 
            this.btnRight.Font = new System.Drawing.Font("Maiandra GD", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRight.ForeColor = System.Drawing.Color.Black;
            this.btnRight.Location = new System.Drawing.Point(217, 373);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(38, 38);
            this.btnRight.TabIndex = 0;
            this.btnRight.Text = ">";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Enabled = false;
            this.btnLeft.Font = new System.Drawing.Font("Maiandra GD", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLeft.ForeColor = System.Drawing.Color.Black;
            this.btnLeft.Location = new System.Drawing.Point(173, 373);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(38, 38);
            this.btnLeft.TabIndex = 0;
            this.btnLeft.Text = "<";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // picHelpPic
            // 
            this.picHelpPic.BackColor = System.Drawing.Color.White;
            this.picHelpPic.Location = new System.Drawing.Point(92, 12);
            this.picHelpPic.Name = "picHelpPic";
            this.picHelpPic.Size = new System.Drawing.Size(252, 252);
            this.picHelpPic.TabIndex = 1;
            this.picHelpPic.TabStop = false;
            // 
            // lblHelpText
            // 
            this.lblHelpText.AutoSize = true;
            this.lblHelpText.Location = new System.Drawing.Point(12, 294);
            this.lblHelpText.Name = "lblHelpText";
            this.lblHelpText.Size = new System.Drawing.Size(0, 13);
            this.lblHelpText.TabIndex = 2;
            // 
            // frmPlayHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 423);
            this.Controls.Add(this.lblHelpText);
            this.Controls.Add(this.picHelpPic);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.btnRight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmPlayHelp";
            this.Text = "Help - Game Rules";
            this.Load += new System.EventHandler(this.frmPlayHelp_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picHelpPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.PictureBox picHelpPic;
        private System.Windows.Forms.Label lblHelpText;
    }
}