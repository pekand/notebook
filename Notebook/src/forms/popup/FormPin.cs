using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notebook
{
    public partial class FormPin : Form
    {
        public FormPin()
        {
            InitializeComponent();
        }

        private void FormPopup_Activated(object sender, EventArgs e)
        {

        }

        private void FormPopup_Load(object sender, EventArgs e)
        {
            this.Width = 50;
            this.Height = 50;

            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;

        }

        private void FormPopup_Deactivate(object sender, EventArgs e)
        {
            
        }

        private void FormPopup_Click(object sender, EventArgs e)
        {
            
        }

        private void FormPopup_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private bool mouseDown;
        private Point lastLocation;
        private int leftPosition = 0;
        private int topPosition = 0;

        private void FormPopup_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;

            this.leftPosition = this.Left;
            this.topPosition = this.Top;
        }

        private void FormPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void FormPopup_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;

            

            if (this.leftPosition == this.Left && this.topPosition == this.Top) {
                Program.formNotebook.ShowForm();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 6 && m.WParam.ToInt32() == 1)
            {
                if (Control.FromHandle(m.LParam) == null)
                {
                    Program.formNotebook.ShowForm();
                }
            }
        }

        private void FormPin_Paint(object sender, PaintEventArgs e)
        {
            Bitmap image = new Bitmap(Notebook.Properties.Resources.Note);
            Graphics x = this.CreateGraphics();
            x.DrawImage(image, new Rectangle(0, 0, 50, 50));
        }
    }
}
