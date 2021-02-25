using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notebook
{
    public class SearchResult
    {
        public string searchFor = "";

        public string nodeText = "";
        public string contentText = "";

        public int position = 0;

        public bool inTitle = false;
        public bool inContent = false;

        public TreeNode node = null;
        public TreeData treeData = null;
        public TabData tabData = null;

        public override string ToString()
        {

            string result = this.searchFor;

            if (inTitle) {
                int start = this.position - 20;


                if (start < 0)
                {
                    start = 0;
                }

                int length = 40 + this.searchFor.Length;

                if (start + length > nodeText.Length)
                {
                    length = nodeText.Length - start;
                }

                string t = nodeText.Substring(start, length);
                result = "@"+this.position.ToString()+":"+ t;
            }

            if (inContent)
            {

                int start = this.position - 20;
                

                if (start < 0) {
                    start = 0;
                }

                int length = 40 + this.searchFor.Length;

                if (start + length > contentText.Length) {
                    length = contentText.Length - start;
                }

                string t = contentText.Substring(start, length);
                result = "#" + this.position.ToString() + ":" + t;
            }

            return result;
        }
    }
}
