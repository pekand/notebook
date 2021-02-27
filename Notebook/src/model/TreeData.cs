using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notebook
{
    public class TreeData
    {
        public string id = "";

        public bool expanded = false;

        public string name = "";
        public string text = "";

        public bool isroot = false;
        public bool note = false;
        public bool folder = false;

        public List<TreeData> childs = new List<TreeData>();

        // shortcuts
        public TreeNode node = null;
        public TabData tabData = null;

        public TreeData Clone()
        {
            TreeData o = new TreeData();

            o.id = this.id;
            o.expanded = this.expanded;
            o.name = this.name;
            o.text = this.text;
            o.isroot = this.isroot;
            o.note = this.note;
            o.folder = this.folder;

            if (childs.Count > 0) {
                foreach (TreeData child in childs) {
                    o.childs.Add(child.Clone());
                }
            }

            o.node = null;
            o.tabData = null;

            return o;
        }
    }
}
