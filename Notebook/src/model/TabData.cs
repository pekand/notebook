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
        public string text = "";
        public bool selected = false;
        public bool saved = false;

        // shortcuts
        public TreeNode node = null;
        public TabPage tabPage = null;
        public ScintillaNET.Scintilla textBox = null;

        public TabData Clone()
        {
            TabData o = new TabData();

            o.id = this.id;
            o.nodeid = this.nodeid;
            o.name = this.name;
            o.text = this.text;
            o.selected = this.selected;
            o.saved = this.saved;

            o.node = null;
            o.tabPage = null;
            o.textBox = null;

            return o;
        }
    }
}
