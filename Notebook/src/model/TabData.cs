using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace Notebook
{
    public class TabData
    {
        public string id = "";

        public string nodeid = "";
        public string name = "";
        public string path = "";
        public string text = "";
        public bool selected = false;
        public bool saved = false;

        public TreeNode node = null;

        public TabPage tabPage = new TabPage();
        //public TextBox textBox = new TextBox();

        public ScintillaNET.Scintilla textBox  = new ScintillaNET.Scintilla();
    }
}
