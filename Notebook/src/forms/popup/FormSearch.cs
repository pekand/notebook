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
    public partial class FormSearch : Form
    {

        private string startSearchFor = "";

        public Point startLocation;
        public FormSearch(string startSearchFor = "")
        {

            this.startSearchFor = startSearchFor;

            InitializeComponent();
        }

        private void FormSearch_Load(object sender, EventArgs e)
        {
            textBoxSearch.Focus();

            if (this.startSearchFor != "")
            {
                textBoxSearch.Text = this.startSearchFor;
                this.search(this.textBoxSearch.Text);
            }

            this.Location = startLocation;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            this.search(this.textBoxSearch.Text);
        }

        public void search(string searchFor) {
            List<SearchResult> results = Program.formNotebook.searchFor(searchFor);

            this.listBox1.Items.Clear();

            int maxCount = 2000;
            foreach (SearchResult result in results)
            {
                this.listBox1.Items.Add(result);

                maxCount--;

                if (maxCount == 0) {
                    break;
                }
            }
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Program.formNotebook.locateSearchResult((SearchResult)listBox1.SelectedItem);
            }
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxSearch.Text.Trim().Length > 3) {
                this.search(this.textBoxSearch.Text);
            }
        }

        public void searchNext() {
            int countItems = listBox1.Items.Count;

            if (countItems > 0)
            {
                int selectedIndex = listBox1.SelectedIndex;
                int next = selectedIndex + 1 < countItems ? selectedIndex + 1 : 0;

                listBox1.SelectedItem = listBox1.Items[next];
                Program.formNotebook.locateSearchResult((SearchResult)listBox1.SelectedItem);
            }
        }

        public void searchPrev()
        {
            int countItems = listBox1.Items.Count;

            if (countItems > 0)
            {
                int selectedIndex = listBox1.SelectedIndex;
                int next = selectedIndex - 1 < 0 ? countItems - 1 : selectedIndex - 1;

                listBox1.SelectedItem = listBox1.Items[next];
                Program.formNotebook.locateSearchResult((SearchResult)listBox1.SelectedItem);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Shift | Keys.F3))
            {
                this.searchPrev();
            }

            if (keyData == (Keys.F3))
            {
                this.searchNext();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {

            SearchResult searchResult = (SearchResult)listBox1.Items[e.Index];

            e.DrawBackground();
            e.Graphics.DrawString(searchResult.ToString().Replace("\r\n", "««").Replace("\r", "«").Replace("\n", "«"), new Font("Consolas", 12, FontStyle.Regular), Brushes.Black, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void FormSearch_Resize(object sender, EventArgs e)
        {
            this.listBox1.Invalidate();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                this.search(this.textBoxSearch.Text);
            }
        }
    }
}
