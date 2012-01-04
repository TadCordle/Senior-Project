namespace Senior_Project
{
    partial class frmOpponent
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnAI = new System.Windows.Forms.Button();
            this.btnHuman = new System.Windows.Forms.Button();
            this.btnAIvsAI = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Game Type";
            // 
            // btnAI
            // 
            this.btnAI.Location = new System.Drawing.Point(29, 32);
            this.btnAI.Name = "btnAI";
            this.btnAI.Size = new System.Drawing.Size(123, 23);
            this.btnAI.TabIndex = 0;
            this.btnAI.Text = "Human vs. CPU";
            this.btnAI.UseVisualStyleBackColor = true;
            this.btnAI.Click += new System.EventHandler(this.btnAI_Click);
            // 
            // btnHuman
            // 
            this.btnHuman.Location = new System.Drawing.Point(29, 61);
            this.btnHuman.Name = "btnHuman";
            this.btnHuman.Size = new System.Drawing.Size(123, 23);
            this.btnHuman.TabIndex = 1;
            this.btnHuman.Text = "Human vs. Human";
            this.btnHuman.UseVisualStyleBackColor = true;
            this.btnHuman.Click += new System.EventHandler(this.btnHuman_Click);
            // 
            // btnAIvsAI
            // 
            this.btnAIvsAI.Location = new System.Drawing.Point(29, 90);
            this.btnAIvsAI.Name = "btnAIvsAI";
            this.btnAIvsAI.Size = new System.Drawing.Size(123, 23);
            this.btnAIvsAI.TabIndex = 0;
            this.btnAIvsAI.Text = "CPU vs. CPU";
            this.btnAIvsAI.UseVisualStyleBackColor = true;
            this.btnAIvsAI.Click += new System.EventHandler(this.btnAIvsAI_Click);
            // 
            // frmOpponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(182, 125);
            this.Controls.Add(this.btnHuman);
            this.Controls.Add(this.btnAIvsAI);
            this.Controls.Add(this.btnAI);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOpponent";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "New Game";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAI;
        private System.Windows.Forms.Button btnHuman;
        private System.Windows.Forms.Button btnAIvsAI;
    }
}