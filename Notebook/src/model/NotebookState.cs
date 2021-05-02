using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using System.Runtime.InteropServices;

namespace Notebook
{
    public class NotebookState
    {
        public String path = "";

        private bool modified = false;

        public int windowsX = 0;
        public int windowsY = 0;
        public int windowsWidth = 0;
        public int windowsHeight = 0;
        public int windowsState = 0;

        public int pinX = -1;
        public int pinY = -1;

        public long lastSave = 0;

        public int splitDistance = 200;

        public List<TreeData> nodesData = new List<TreeData>();
        public List<TabData> tabsData = new List<TabData>();

        //shortcuts
        public TreeView treeView = null;
        public TabControl tabControl = null;

        // CONSTUCT

        public NotebookState(TreeView treeView = null, TabControl tabControl = null) {
            this.treeView = treeView;
            this.tabControl = tabControl;
        }


        // CLONE

        public NotebookState Clone()
        {
            NotebookState o = new NotebookState();

            o.path = this.path;

            o.windowsX = this.windowsX;
            o.windowsY = this.windowsY;
            o.windowsWidth = this.windowsWidth;
            o.windowsHeight = this.windowsHeight;
            o.windowsState = this.windowsState;

            o.pinX = this.pinX;
            o.pinY = this.pinY;

            o.lastSave = this.lastSave;

            o.splitDistance = this.splitDistance;

            o.treeView = null;
            o.tabControl = null;


            List<TreeData> nodesData = new List<TreeData>();
            foreach (TreeData treeData in this.nodesData)
            {
                nodesData.Add(treeData.Clone());
            }
            o.nodesData = nodesData;

            List<TabData> tabsData = new List<TabData>();
            foreach (TabData tabData in this.tabsData)
            {
                tabsData.Add(tabData.Clone());
            }
            o.tabsData = tabsData;

            return o;
        }

        // MODIFICATION

        public void Modified()
        {
            if (!this.modified)
            {
                this.modified = true;
            }
        }

        public void unModified()
        {
            this.modified = false;
        }

        public bool isModified()
        {
            return this.modified;
        }


        // STATE

        public void copyStateFromControls()
        {

            nodesData.Clear();
            tabsData.Clear();

            if (treeView != null)
            {
                nodesData = this.getAllNodesData(this.treeView.Nodes);
            }

            if (tabControl != null)
            {
                tabsData = this.getCopyOfAllTabsData();
            }
        }

        public Dictionary<string, TreeData> setTreeState(TreeNodeCollection nodes, List<TreeData> treeDataList, Dictionary<string, TreeData> dataList = null)
        {

            if (dataList == null)
            {
                dataList = new Dictionary<string, TreeData>();
            }

            foreach (TreeData treeData in treeDataList)
            {

                if (dataList.ContainsKey(treeData.id))
                {
                    continue;
                }

                dataList.Add(treeData.id, treeData);

                TreeNode node = new TreeNode();

                node.Tag = treeData;
                node.Text = treeData.name;

                treeData.node = node;

                if (treeData.isroot)
                {
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 0;
                    node.ImageKey = "notebook.ico";
                }

                if (treeData.note)
                {
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                    node.ImageKey = "note.png";
                }

                if (treeData.folder)
                {
                    node.ImageIndex = 2;
                    node.SelectedImageIndex = 2;
                    node.ImageKey = "folder.png";
                }

                if (treeData.isLink)
                {
                    node.ImageIndex = 3;
                    node.SelectedImageIndex = 3;
                    node.ImageKey = "link.png";
                }

                if (treeData.isUrl)
                {
                    node.ImageIndex = 4;
                    node.SelectedImageIndex = 4;
                    node.ImageKey = "url.png";
                }

                nodes.Add(node);

                if (treeData.childs.Count > 0)
                {
                    this.setTreeState(node.Nodes, treeData.childs, dataList);
                }

                if (treeData.expanded)
                {
                    node.Expand();
                }
            }

            return dataList;
        }

        public void setTabsState(List<TabData> tabsData, List<TreeData> treeDataList, Dictionary<string, TreeData> dataList)
        {
            foreach (TabData tabData in tabsData)
            {

                tabData.tabPage = new TabPage();
                tabData.textBox = new ScintillaNET.Scintilla();

                TreeData treeData = dataList[tabData.nodeid];


                TreeNode node = treeData != null ? treeData.node : null;

                tabData.node = node;

                tabData.textBox.EolMode = Eol.Lf;
                tabData.textBox.ConvertEols(Eol.Lf);

                if (node != null)
                {
                    tabData.nodeid = treeData.id;
                    tabData.textBox.Text = treeData.text.Replace("\r\n", "\n");
                    treeData.tabData = tabData;
                }
                else
                {
                    tabData.textBox.Text = tabData.text.Replace("\r\n", "\n");
                }


                tabData.tabPage.Text = tabData.name;

                tabData.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
                tabData.textBox.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tabData.textBox.Location = new System.Drawing.Point(3, 3);
                tabData.textBox.Name = "textBox-" + Time.getUnixTime().ToString();
                tabData.textBox.Size = new System.Drawing.Size(546, 296);
                tabData.textBox.TabIndex = 0;
                tabData.textBox.Tag = tabData;
                tabData.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
                tabData.textBox.IndentationGuides = IndentView.LookBoth;
                tabData.textBox.WrapMode = ScintillaNET.WrapMode.Word;
                tabData.textBox.StyleResetDefault();
                tabData.textBox.Styles[Style.Default].Font = "Consolas";
                tabData.textBox.Styles[Style.Default].Size = 16;
                tabData.textBox.AssignCmdKey(Keys.Control | Keys.W, Command.Null);


                tabData.textBox.EmptyUndoBuffer();
                tabData.textBox.StyleClearAll();

                SendMessage(tabData.textBox.Handle, EM_SETTABSTOPS, 1,
                new int[] { 4 * 4 });

                tabData.tabPage.Controls.Add(tabData.textBox);
                tabControl.TabPages.Add(tabData.tabPage);
                tabControl.SelectedTab = tabData.tabPage;
                tabData.tabPage.Tag = tabData;
            }

            foreach (TabData tabData in tabsData)
            {
                if (tabData.selected)
                {
                    this.selectTab(tabData.tabPage);
                    break;
                }
            }
        }

        public void setState(NotebookState notebookState)
        {
            notebookState.treeView = treeView;
            notebookState.tabControl = tabControl;

            Dictionary<string, TreeData> dataList = this.setTreeState(treeView.Nodes, notebookState.nodesData);
            this.setTabsState(notebookState.tabsData, notebookState.nodesData, dataList);

        }

        public void unloadCurentState()
        {

            this.path = "";
            this.modified = false;

            this.windowsX = 0;
            this.windowsY = 0;
            this.windowsWidth = 0;
            this.windowsHeight = 0;
            this.windowsState = 0;

            this.pinX = -1;
            this.pinY = -1;

            this.lastSave = 0;

            this.splitDistance = 200;

            if (this.treeView != null)
            {
                List<TreeNode> treeNodes = this.getAllNodes(this.treeView.Nodes);

                foreach (TreeNode treeNode in treeNodes)
                {
                    TreeData treeData = (TreeData)treeNode.Tag;
                    treeData.node = null;
                    treeData.tabData = null;

                    treeNode.Tag = null;
                }

                this.treeView.Nodes.Clear();
            }

            if (this.tabControl != null)
            {
                for (int i = 0; i < this.tabControl.TabCount; ++i)
                {
                    TabPage tabpage = (TabPage)this.tabControl.GetControl(i);

                    TabData tabData = (TabData)tabpage.Tag;
                    tabData.node = null;
                    tabData.tabPage = null;
                    tabData.textBox = null;

                    tabpage.Tag = null;
                }

                this.tabControl.TabPages.Clear();
            }
        }

        // NODES

        public bool ContainsNode(TreeNode node1, TreeNode node2)
        {

            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            return ContainsNode(node1, node2.Parent);
        }


        public TreeData getNodeData(TreeNode node)
        {
            TreeData treeData = (TreeData)node.Tag;

            return treeData;
        }

        public List<TreeNode> getAllNodes(TreeNodeCollection collection = null, List<TreeNode> list = null)
        {
            if (collection == null)
            {
                collection = treeView.Nodes;
            }

            if (list == null)
            {
                list = new List<TreeNode>();
            }

            foreach (TreeNode node in collection)
            {
                list.Add(node);

                if (node.Nodes.Count > 0)
                {
                    this.getAllNodes(node.Nodes, list);
                }
            }

            return list;
        }

        public TreeNode findNodeById(string id, TreeNodeCollection nodes = null)
        {

            if (nodes == null)
            {
                nodes = this.treeView.Nodes;
            }

            TreeData treeData = null;
            for (int i = 0; i < nodes.Count; i++)
            {
                treeData = (TreeData)nodes[i].Tag;
                if (treeData.id == id)
                {
                    return nodes[i];
                }

                if (nodes[i].Nodes.Count > 0)
                {
                    TreeNode node = this.findNodeById(id, nodes[i].Nodes);

                    if (node != null)
                    {
                        return node;
                    }
                }

            }

            return null;

        }

        public bool canRemoveNode(TreeNode parent)
        {
            bool canRemove = true;
            if (parent.Nodes.Count > 0)
            {
                for (int i = 0; i < parent.Nodes.Count; i++)
                {
                    if (!canRemoveNode(parent.Nodes[i]))
                    {
                        canRemove = false;
                    }
                }
            }

            if (!canRemove)
            {
                return false;
            }

            return true;
        }


        public TreeNode addRootNode()
        {
            string name = "Notebook";
            TreeNode root = this.addNewNode(name, null, "root");
            root.ImageIndex = 0;
            root.SelectedImageIndex = 0;
            root.ImageKey = "notebook.ico";
            TreeData treeData = (TreeData)root.Tag;
            treeData.isroot = true;

            treeView.Nodes.Add(root);

            return root;
        }

        public TreeNode addNewNode(string name, TreeNode parent = null, string nodeType = "note")
        {
            TreeNode node = new TreeNode();
            TreeData treeData = new TreeData();
            treeData.id = Uid.get();
            treeData.name = name;
            treeData.node = node;
            
            node.Tag = treeData;
            node.Text = name;

            this.setType(node, nodeType);

            if (parent != null)
            {
                parent.Nodes.Add(node);
                parent.Expand();
            }

            return node;
        }

        public void setType(TreeNode node, string nodeType = "note")
        {
            TreeData treeData = this.getNodeData(node);

            if (treeData.isroot) {
                return;
            }

            treeData.isroot = false;
            treeData.folder = false;
            treeData.isLink = false;
            treeData.isUrl = false;

            treeData.note = true;
            node.ImageIndex = 1;
            node.SelectedImageIndex = 1;
            node.ImageKey = "note.png";

            if (nodeType == "root") 
            {
                treeData.isroot = true;
                node.ImageIndex = 0;
                node.SelectedImageIndex = 0;
                node.ImageKey = "notebook.ico";

            } else if (nodeType == "folder") 
            {
                treeData.folder = true;
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;
                node.ImageKey = "folder.png";
            }
            else if (nodeType == "link")
            {
                treeData.isLink = true;
                node.ImageIndex = 3;
                node.SelectedImageIndex = 3;
                node.ImageKey = "link.png";
            }
            else if (nodeType == "url")
            {
                treeData.isUrl = true;
                node.ImageIndex = 4;
                node.SelectedImageIndex = 4;
                node.ImageKey = "url.png";
            }
            
        }

        public void removeNode(TreeNode node)
        {

            if (node == null)
            {
                return;
            }

            if (node.Nodes.Count > 0)
            {
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    removeNode(node.Nodes[i]);
                }
            }

            TreeData treeData = this.getNodeData(node);
            if (treeData.tabData != null && treeData.tabData.tabPage != null)
            {
                this.closeTab(treeData.tabData.tabPage);
            }

            if (treeData.isroot)
            {
                return;
            }

            this.Modified();

            if (node.Parent != null)
            {
                node.Parent.Nodes.Remove(node);
            }
            else
            {
                treeView.Nodes.Remove(node);
            }
        }

        public List<TreeData> getAllNodesData(TreeNodeCollection collection = null, List<TreeData> list = null)
        {
            if (list == null)
            {
                list = new List<TreeData>();
            }

            foreach (TreeNode node in collection)
            {

                TreeData treeData = (TreeData)node.Tag;

                treeData.expanded = node.IsExpanded;

                list.Add(treeData);

                if (node.Nodes.Count > 0)
                {
                    treeData.childs.Clear();
                    this.getAllNodesData(node.Nodes, treeData.childs);
                }
            }

            return list;
        }


        public void expandNode(TreeNode node) {
            

            TreeData treeData = this.getNodeData(node);
            treeData.expanded = true;

            if (!node.IsExpanded)
            {
                node.Expand();
            }
        }

        public void collapseNode(TreeNode node)
        {
            TreeData treeData = this.getNodeData(node);
            treeData.expanded = false;

            if (node.IsExpanded)
            {
                node.Collapse();
            }
        }

        // TABS
        public TabData getTabData(TabPage tab)
        {
            TabData treeData = (TabData)tab.Tag;

            return treeData;
        }

        public List<TabData> getCopyOfAllTabsData()
        {
            List<TabData> tabsData = new List<TabData>();

            for (int i = 0; i < this.tabControl.TabCount; ++i)
            {
                TabPage tabpage = (TabPage)this.tabControl.GetControl(i);
                TabData tabData = ((TabData)tabpage.Tag).Clone();
                tabsData.Add(tabData);
            }

            return tabsData;
        }

        public List<TabData> getAllTabsData()
        {
            List<TabData> tabsData = new List<TabData>();

            for (int i = 0; i < tabControl.TabCount; ++i)
            {
                TabPage tabpage = (TabPage)tabControl.GetControl(i);
                TabData tabData = (TabData)tabpage.Tag;
                tabsData.Add(tabData);
            }

            return tabsData;
        }

        private const int EM_SETTABSTOPS = 0x00CB;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

        public TabData addNewTab(string name = "New", TreeNode node = null, TabData tabData = null)
        {
            if (tabData == null)
            {
                tabData = new TabData();
            }

            tabData.tabPage = new TabPage();
            tabData.textBox = new ScintillaNET.Scintilla();

            tabData.id = Uid.get();
            tabData.name = name;
            tabData.node = node;

            tabData.textBox.EolMode = Eol.Lf;
            tabData.textBox.ConvertEols(Eol.Lf);

            if (node != null)
            {
                TreeData treeData = (TreeData)node.Tag;
                tabData.nodeid = treeData.id;
                tabData.textBox.Text = treeData.text;
                treeData.tabData = tabData;
            }
            else
            {
                tabData.textBox.Text = tabData.text;
            }

            tabData.tabPage.Text = tabData.name;
            tabData.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            tabData.textBox.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tabData.textBox.Location = new System.Drawing.Point(3, 3);
            tabData.textBox.Name = "textBox-" + Time.getUnixTime().ToString();
            tabData.textBox.Size = new System.Drawing.Size(546, 296);
            tabData.textBox.TabIndex = 0;
            tabData.textBox.Tag = tabData;
            tabData.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            tabData.textBox.IndentationGuides = IndentView.LookBoth;
            tabData.textBox.WrapMode = ScintillaNET.WrapMode.Word;
            tabData.textBox.StyleResetDefault();
            tabData.textBox.Styles[Style.Default].Font = "Consolas";
            tabData.textBox.Styles[Style.Default].Size = 16;
            //tabData.textBox.Styles[Style.Default].BackColor = IntToColor(0x212121);
            //tabData.textBox.Styles[Style.Default].ForeColor = IntToColor(0xFFFFFF);
            tabData.textBox.AssignCmdKey(Keys.Control | Keys.W, Command.Null);
            tabData.textBox.AssignCmdKey(Keys.Control | Keys.T, Command.Null);

            tabData.textBox.EmptyUndoBuffer();
            tabData.textBox.StyleClearAll();

            SendMessage(tabData.textBox.Handle, EM_SETTABSTOPS, 1,
            new int[] { 4 * 4 });
            tabControl.SuspendLayout();
            tabData.tabPage.Controls.Add(tabData.textBox);
            tabControl.TabPages.Add(tabData.tabPage);
            tabControl.SelectedTab = tabData.tabPage;
            tabData.tabPage.Tag = tabData;
            tabControl.ResumeLayout();

            return tabData;
        }

        public void selectTab(TabPage tab)
        {

            for (int i = 0; i < tabControl.TabCount; ++i)
            {
                TabData tabData = (TabData)((TabPage)tabControl.GetControl(i)).Tag;
                if (tabControl.GetControl(i) == tab)
                {
                    tabData.selected = true;
                }
                else
                {
                    tabData.selected = false;
                }
            }

            tabControl.SelectedTab = tab;
        }

        public bool selectNodeTab(TreeNode node)
        {
            TabData tabData = null;

            for (int i = 0; i < tabControl.TabCount; ++i)
            {
                TabPage tab = (TabPage)tabControl.GetControl(i);
                tabData = (TabData)tab.Tag;
                if (tabData.node == node)
                {
                    this.selectTab(tab);
                    return true;
                }
            }

            return false;
        }

        public void opentNodeTab(TreeNode node)
        {
            if (this.selectNodeTab(node))
            {
                return;
            }

            TreeData treeData = (TreeData)node.Tag;
            TabData tabData = new TabData();
            this.addNewTab(treeData.name, node, tabData);
            this.selectTab(tabData.tabPage);
        }

        public void closeTab(TabPage tab)
        {
            int i = 0;
            for (i = 0; i < tabControl.TabCount; ++i)
            {
                if (tabControl.GetControl(i) == tab)
                {
                    break;
                }
            }

            TabData tabData = (TabData)tab.Tag;

            if (tabData.node != null)
            {
                TreeData nodeData = (TreeData)tabData.node.Tag;
                nodeData.text = ((ScintillaNET.Scintilla)tab.Controls[0]).Text;

                nodeData.tabData = null;
            }

            tabControl.SuspendLayout();

            tabControl.TabPages.Remove(tab);

            if (tabControl.TabCount > 0)
            {
                if (i < tabControl.TabCount)
                {
                    this.selectTab((TabPage)tabControl.GetControl(i));
                }
                else
                {
                    this.selectTab((TabPage)tabControl.GetControl(tabControl.TabCount - 1));
                }
            }

            tabControl.ResumeLayout();
        }


        public int getHoverTabIndex()
        {
            for (int i = 0; i < this.tabControl.TabPages.Count; i++)
            {
                if (this.tabControl.GetTabRect(i).Contains(this.tabControl.PointToClient(Cursor.Position)))
                    return i;
            }

            return -1;
        }

        public void swapTabPages(TabPage src, TabPage dst)
        {
            int index_src = this.tabControl.TabPages.IndexOf(src);
            int index_dst = this.tabControl.TabPages.IndexOf(dst);
            this.tabControl.TabPages[index_dst] = src;
            this.tabControl.TabPages[index_src] = dst;
            this.tabControl.Refresh();
        }

        public void renameTab(string name, TabPage tab)
        {
            if (tab == null)
            {
                return;
            }


            TabData tadData = (TabData)tab.Tag;

            if (tadData.name != name)
            {
                this.Modified();
            }

            tadData.name = name;
            tadData.tabPage.Text = name;

            if (tadData.node != null)
            {
                tadData.node.Text = name;
                TreeData treeData = (TreeData)tadData.node.Tag;
                treeData.name = name;
            }
        }

        // TEXTBOX
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            ScintillaNET.Scintilla textBox = (ScintillaNET.Scintilla)sender;
            TabData tabData = (TabData)textBox.Tag;

            if (textBox.Text != tabData.text)
            {
                tabData.text = textBox.Text;

                if (tabData.node != null)
                {
                    TreeData treeData = (TreeData)tabData.node.Tag;
                    treeData.text = textBox.Text;
                }

                this.Modified();
            }
        }

    }
}
