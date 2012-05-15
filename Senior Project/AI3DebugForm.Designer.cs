namespace Senior_Project
{
    partial class AI3DebugForm
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
            this.tvAITree = new System.Windows.Forms.TreeView();
            this.picBoard = new System.Windows.Forms.PictureBox();
            this.container = new System.Windows.Forms.SplitContainer();
            this.txtTrace = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picBoard)).BeginInit();
            this.container.Panel1.SuspendLayout();
            this.container.Panel2.SuspendLayout();
            this.container.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvAITree
            // 
            this.tvAITree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.tvAITree.Location = new System.Drawing.Point(3, 4);
            this.tvAITree.Name = "tvAITree";
            this.tvAITree.Size = new System.Drawing.Size(362, 369);
            this.tvAITree.TabIndex = 0;
            // 
            // picBoard
            // 
            this.picBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picBoard.BackColor = System.Drawing.Color.White;
            this.picBoard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBoard.Location = new System.Drawing.Point(371, 4);
            this.picBoard.Name = "picBoard";
            this.picBoard.Size = new System.Drawing.Size(369, 369);
            this.picBoard.TabIndex = 1;
            this.picBoard.TabStop = false;
            // 
            // container
            // 
            this.container.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.container.Location = new System.Drawing.Point(12, 12);
            this.container.Name = "container";
            this.container.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // container.Panel1
            // 
            this.container.Panel1.Controls.Add(this.picBoard);
            this.container.Panel1.Controls.Add(this.tvAITree);
            // 
            // container.Panel2
            // 
            this.container.Panel2.Controls.Add(this.txtTrace);
            this.container.Size = new System.Drawing.Size(745, 569);
            this.container.SplitterDistance = 377;
            this.container.TabIndex = 2;
            // 
            // txtTrace
            // 
            this.txtTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTrace.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTrace.Location = new System.Drawing.Point(3, 3);
            this.txtTrace.Multiline = true;
            this.txtTrace.Name = "txtTrace";
            this.txtTrace.ReadOnly = true;
            this.txtTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtTrace.Size = new System.Drawing.Size(737, 182);
            this.txtTrace.TabIndex = 0;
            this.txtTrace.Text = ">mfw AI doesn\'t work";
            // 
            // AIDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 593);
            this.Controls.Add(this.container);
            this.Name = "AIDebugForm";
            this.Text = "MiniMax Debugger";
            ((System.ComponentModel.ISupportInitialize)(this.picBoard)).EndInit();
            this.container.Panel1.ResumeLayout(false);
            this.container.Panel2.ResumeLayout(false);
            this.container.Panel2.PerformLayout();
            this.container.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvAITree;
        private System.Windows.Forms.PictureBox picBoard;
        private System.Windows.Forms.SplitContainer container;
        private System.Windows.Forms.TextBox txtTrace;
    }
}