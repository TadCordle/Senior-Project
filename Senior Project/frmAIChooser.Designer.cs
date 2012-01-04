namespace Senior_Project
{
	partial class frmAIChooser
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
            this.cmbAI1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbAI2 = new System.Windows.Forms.ComboBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBrowseAI2 = new System.Windows.Forms.Button();
            this.btnBrowseAI1 = new System.Windows.Forms.Button();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // cmbAI1
            // 
            this.cmbAI1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAI1.FormattingEnabled = true;
            this.cmbAI1.Location = new System.Drawing.Point(12, 25);
            this.cmbAI1.Name = "cmbAI1";
            this.cmbAI1.Size = new System.Drawing.Size(277, 21);
            this.cmbAI1.Sorted = true;
            this.cmbAI1.TabIndex = 0;
            this.cmbAI1.SelectedIndexChanged += new System.EventHandler(this.cmbAI1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "AI 1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "AI 2";
            // 
            // cmbAI2
            // 
            this.cmbAI2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAI2.FormattingEnabled = true;
            this.cmbAI2.Location = new System.Drawing.Point(12, 70);
            this.cmbAI2.Name = "cmbAI2";
            this.cmbAI2.Size = new System.Drawing.Size(277, 21);
            this.cmbAI2.Sorted = true;
            this.cmbAI2.TabIndex = 4;
            this.cmbAI2.SelectedIndexChanged += new System.EventHandler(this.cmbAI2_SelectedIndexChanged);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(202, 114);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(78, 24);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(99, 114);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 24);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBrowseAI2
            // 
            this.btnBrowseAI2.Location = new System.Drawing.Point(295, 67);
            this.btnBrowseAI2.Name = "btnBrowseAI2";
            this.btnBrowseAI2.Size = new System.Drawing.Size(78, 24);
            this.btnBrowseAI2.TabIndex = 5;
            this.btnBrowseAI2.Text = "Browse";
            this.btnBrowseAI2.UseVisualStyleBackColor = true;
            this.btnBrowseAI2.Click += new System.EventHandler(this.btnBrowseAI2_Click);
            // 
            // btnBrowseAI1
            // 
            this.btnBrowseAI1.Location = new System.Drawing.Point(295, 22);
            this.btnBrowseAI1.Name = "btnBrowseAI1";
            this.btnBrowseAI1.Size = new System.Drawing.Size(78, 24);
            this.btnBrowseAI1.TabIndex = 2;
            this.btnBrowseAI1.Text = "Browse";
            this.btnBrowseAI1.UseVisualStyleBackColor = true;
            this.btnBrowseAI1.Click += new System.EventHandler(this.btnBrowseAI1_Click);
            // 
            // ofd
            // 
            this.ofd.DefaultExt = "cs";
            this.ofd.Filter = "C# files|*.cs";
            this.ofd.Title = "Choose AI File";
            // 
            // frmAIChooser
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(379, 150);
            this.Controls.Add(this.btnBrowseAI1);
            this.Controls.Add(this.btnBrowseAI2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbAI2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbAI1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAIChooser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Choose AI";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox cmbAI1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbAI2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnBrowseAI2;
		private System.Windows.Forms.Button btnBrowseAI1;
		private System.Windows.Forms.OpenFileDialog ofd;
	}
}