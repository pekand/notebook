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

                int start = this.position;
                int end = this.position;

                while (start > 0) {
                    start--;
                    if (nodeText[start] == '\n') {
                        break;
                    }
                }

                while (end < nodeText.Length - 1)
                {
                    end++;
                    if (nodeText[end] == '\n')
                    {
                        break;
                    }
                }

                result = nodeText.Substring(start, end - start + 1).Trim();
            }

            if (inContent)
            {
                int start = this.position;
                int end = this.position;

                while (start > 0)
                {
                    start--;
                    if (contentText[start] == '\n')
                    {
                        break;
                    }
                }

                while (end < contentText.Length - 1)
                {
                    end++;
                    if (contentText[end] == '\n')
                    {
                        break;
                    }
                }

                result = contentText.Substring(start, end - start + 1).Trim();
            }

            return result;
        }
    }
}
