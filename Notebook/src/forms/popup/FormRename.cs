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
    public partial class FormRename : Form
    {
        public string text = "";
        public bool cancled = false;
        public bool saved = false;
        public Point startLocation;

        public FormRename()
        {
            InitializeComponent();
        }

        private void FormRename_Load(object sender, EventArgs e)
        {
            this.cancled = false;
            this.saved = false;

            textBox.Text = text;

            this.Location = startLocation;

            textBox.Focus();

        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.cancled = true;
            this.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.text = this.textBox.Text;
            this.saved = true;
            this.Close();
        }
    }
}
