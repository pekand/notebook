
namespace Notebook
{
    partial class FormPin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPin));
            this.SuspendLayout();
            // 
            // FormPin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(50, 50);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPin";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Notebook";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.FormPopup_Activated);
            this.Deactivate += new System.EventHandler(this.FormPopup_Deactivate);
            this.Load += new System.EventHandler(this.FormPopup_Load);
            this.Click += new System.EventHandler(this.FormPopup_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormPin_Paint);
            this.DoubleClick += new System.EventHandler(this.FormPopup_DoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormPopup_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormPopup_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FormPopup_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}