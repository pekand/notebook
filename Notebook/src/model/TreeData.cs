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

        public string name = "";
        public string text = "";
        public bool isroot = false;
        public bool note = false;
        public bool folder = false;
        public bool expanded = false;

        public TabData tabData = null;
    }
}
