using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ScintillaNET;

namespace Notebook
{
    public partial class FormNotebook : Form
    {

        private bool modified = false;

        private Options options = new Options();

        private int countTabs = 0;

        private bool isDoubleClick = false;

        private bool saveWindowPosition = false;

        public FormNotebook()
        {
            InitializeComponent();
        }

        // FORM
        private void FormNotebook_Load(object sender, EventArgs e)
        {
            this.tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;

            List<TabData> tabsData = new List<TabData>();
            OptionsFile optionsFile = new OptionsFile(this.options, tabsData, this.treeView.Nodes);
            optionsFile.LoadConfigFile();

            tabControl.TabPages.Remove(tabPageSettings);
            tabControl.TabPages.Remove(tabPageEdit);
            tabControl.SelectedTab = tabPageSettings;

            for (int i = 0; i < tabsData.Count; ++i)
            {
                // find node for tab
                if (tabsData[i].nodeid != "")
                {
                    tabsData[i].node = this.finNodeById(tabsData[i].nodeid);

                    if (tabsData[i].node == null) {
                        tabsData[i].nodeid = "";
                    }
                }

                // select tab 
                if (tabsData[i].selected) {
                    this.selectTab(tabsData[i].tabPage);
                }
            }

            // add first node if treeview is empty
            if (this.treeView.Nodes.Count == 0)
            {
                this.addRootNode();
            }

            // add tabs to tabControll
            for (int i = 0; i < tabsData.Count; i++)
            {
                this.addNewTab(tabsData[i].name, tabsData[i].node, tabsData[i]);
            }

            

            this.restoreWindowPosition();
        }

        private void restoreWindowPosition() {

            if (this.options.windowsWidth == 0 || this.options.windowsHeight == 0)
            {
                this.Left = 100;
                this.Top = 100;
                this.Width = 500;
                this.Height = 500;
                return;
            }

            this.Left = this.options.windowsX;
            this.Top = this.options.windowsY;
            this.Width = this.options.windowsWidth;
            this.Height = this.options.windowsHeight;


            if (this.options.windowsState == 1)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            if (this.options.windowsState == 2)
            {
                this.WindowState = FormWindowState.Minimized;
            }

            if (this.options.windowsState == 3)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.splitContainer.SplitterDistance = this.options.splitDistance;

            saveWindowPosition = true;
        }

        public void saveConfig()
        {
            List<TabData> tabsData = this.getAllTabsData();
            OptionsFile optionsFile = new OptionsFile(this.options, tabsData, this.treeView.Nodes);
            optionsFile.SaveConfigFile();

            this.modified = false;
            this.SetTitle();
        }

        private void FormNotebook_FormClosing(object sender, FormClosingEventArgs e)
        {
            // prevent flash of pin form before close form
            enablePin = false;

            // save text from abs to nodes
            if (tabControl.TabCount > 0)
            {
                for (int i = 0; i < tabControl.TabCount; ++i)
                {
                    TabPage tab = (TabPage)tabControl.GetControl(i);
                    TabData tabData = this.getTabData(tab);
                    if (tabData.node!= null)
                    {
                        TreeData treeData = this.getNodeData(tabData.node);
                        treeData.text = ((ScintillaNET.Scintilla)tab.Controls[0]).Text;
                    }
                }
            }

            this.saveConfig();
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (saveWindowPosition)
            {
                this.options.splitDistance = this.splitContainer.SplitterDistance;
            }
        }

        private void FormNotebook_Move(object sender, EventArgs e)
        {
            if (saveWindowPosition)
            {

                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.options.windowsState = 1;
                    return;
                }

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.options.windowsState = 2;
                    return;
                }

                if (this.WindowState == FormWindowState.Normal)
                {
                    this.options.windowsState = 3;
                }

                this.options.windowsX = this.Left;
                this.options.windowsY = this.Top;
                this.options.windowsWidth = this.Width;
                this.options.windowsHeight = this.Height;
            }

        }

        private void SetTitle()
        {
            this.Text = "Notebook" + (this.modified ? "*" : "");
        }


        private void autosaveTimer() {
            timer.Enabled = false;
            timer.Interval = 50000;
            timer.Enabled = true;
        }

        private void Modified() {

            if (!this.modified) {
                this.modified = true;
                this.SetTitle();
                this.autosaveTimer();
            }
            
        }


        private bool enablePin = true;


        private bool isPinned = false;

        private FormPin formPopup = new FormPin();

        public bool IsOnScreen(Form form)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                Rectangle formRectangle = new Rectangle(form.Left, form.Top, form.Width, form.Height);


                Point formTopLeft = new Point(formRectangle.Left, formRectangle.Top);
                Point formTopRiht = new Point(formRectangle.Left+ formRectangle.Width, formRectangle.Top);
                Point formBottomLeft = new Point(formRectangle.Left, formRectangle.Top + formRectangle.Height);
                Point formBottomRight = new Point(formRectangle.Left + formRectangle.Width, formRectangle.Top + formRectangle.Height);

                if (
                    screen.WorkingArea.Contains(formTopLeft)||
                    screen.WorkingArea.Contains(formTopRiht)||
                    screen.WorkingArea.Contains(formBottomLeft)||
                    screen.WorkingArea.Contains(formBottomRight)
                )
                {
                    return true;
                }

                /*if (screen.WorkingArea.Contains(formRectangle))
                {
                    return true;
                }*/
            }

            return false;
        }


        private bool CanPin() {
            return enablePin && !isPinned && (this.formSearch == null || !this.formSearch.Visible);
        }
        private void ShowPin() {
            if (!CanPin())
            {
                return;
            }

            isPinned = true;


            if (this.options.pinX == -1 && this.options.pinY == -1) {
                formPopup.Left = this.Left + this.Width - 50;
                formPopup.Top = this.Top + this.Height - 50;
            } else {
                formPopup.Left = this.options.pinX;
                formPopup.Top = this.options.pinY;
            }


            if (!this.IsOnScreen(formPopup)) {
                Rectangle rec = Screen.FromControl(formPopup).Bounds;
                formPopup.Left = rec.Width - 200;
                formPopup.Top = rec.Height - 200;
            }

            formPopup.Show();
            this.HideForm();
        }

        private bool isHidden = false;
        private int positionLeft = 0;
        private int positionTop = 0;

        public void HideForm() {
            if (isHidden)
            {
                return;
            }

            isHidden = true;

            saveWindowPosition = false;
            positionLeft = this.Left;
            positionTop = this.Top;
            this.Left = -10000;
            this.Top = -10000;
            
        }

        public void ShowForm()
        {
            if (!isHidden)
            {
                return;
            }

            formPopup.Hide();

            this.options.pinX = formPopup.Left;
            this.options.pinY = formPopup.Top;

            isHidden = false;
            isPinned = false;

            this.Left = positionLeft;
            this.Top = positionTop;


            if (!this.IsOnScreen(this))
            {
                Rectangle rec = Screen.FromControl(this).Bounds;
                this.Width = rec.Width / 2;
                this.Height = rec.Height / 2;
                this.Left = (rec.Width - (this.Width)) / 2;
                this.Top = (rec.Height - (this.Height)) / 2;
            }

            this.BringToFront();
            saveWindowPosition = true;
        }

        private void FormNotebook_Activated(object sender, EventArgs e)
        {
            if (isHidden) {
                ShowForm();
            }
        }

        private void FormNotebook_Deactivate(object sender, EventArgs e)
        {
            this.ShowPin();
        }

        private void FormNotebook_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized || WindowState == FormWindowState.Maximized)
            {
                enablePin = false;
            }
            else {
                enablePin = true;
            }
        }

        // MENUSTRIP

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabData tabData = this.addNewTab("New");
            this.selectTab(tabData.tabPage);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveConfig();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.TabPages.Add(tabPageSettings);
            tabControl.SelectedTab = tabPageSettings;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private FormSearch formSearch = null;
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchFor = "";

            if (this.tabControl.SelectedTab != null) {
                TabData tabData = this.getTabData(this.tabControl.SelectedTab);
                searchFor = tabData.textBox.SelectedText;
            }

            formSearch = new FormSearch(searchFor);
            formSearch.startLocation = new Point(Cursor.Position.X - 100, Cursor.Position.Y-100);
            formSearch.Owner = this;
            formSearch.Show();
        }

        // SHORTCUT

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.W))
            {

                TabPage tab = tabControl.SelectedTab;

                if (tab != null)
                {
                    this.closeTab(tab);
                }
            }


            if (keyData == (Keys.Shift | Keys.F3))
            {
                if (this.formSearch != null) 
                {
                    this.formSearch.searchPrev();
                }
            }

            if (keyData == (Keys.F3))
            {
                if (this.formSearch != null)
                {
                    this.formSearch.searchNext();
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // SETTINGS

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {

        }

        // TREEVIEW

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

            foreach (TreeNode node in collection) {
                list.Add(node);

                if (node.Nodes.Count > 0) {
                    this.getAllNodes(node.Nodes, list);
                }
            }

            return list;
        }

        public TreeData getNodeData(TreeNode node) {
            TreeData treeData = (TreeData)node.Tag;

            return treeData;
        }

        public TreeNode finNodeById(string id, TreeNodeCollection nodes = null) {

            if (nodes == null) {
                nodes = treeView.Nodes;
            }

            TreeData treeData = null;
            for (int i = 0; i< nodes.Count; i++) {
                treeData = (TreeData)nodes[i].Tag;
                if (treeData.id == id) {
                    return nodes[i];
                }

                if (nodes[i].Nodes.Count>0) {
                    TreeNode node = this.finNodeById(id, nodes[i].Nodes);

                    if (node != null) {
                        return node;
                    }
                }

            }

            return null;

        }

        public TreeNode addRootNode()
        {
            string name = "Notebook";
            TreeNode root = this.addNewNode(name);
            root.ImageIndex = 0;
            root.SelectedImageIndex = 0;
            root.ImageKey = "notebook.ico";
            TreeData treeData = (TreeData)root.Tag;
            treeData.isroot = true;


            treeView.Nodes.Add(root);

            return root;
        }

        public TreeNode addNewNode(string name, TreeNode parent = null)
        {
            TreeNode node = new TreeNode();
            TreeData treeData = new TreeData();
            treeData.id = Uid.get();
            treeData.name = name;
            node.Tag = treeData;
            node.Text = name;
            node.ImageIndex = 1;
            node.SelectedImageIndex = 1;
            node.ImageKey = "note.png";

            if (parent != null)
            {
                parent.Nodes.Add(node);
                parent.Expand();
            }

            return node;
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

        public void removeNode(TreeNode node)
        {

            if (node == null) {
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

            if (treeData.isroot) {
                return;
            }

            this.Modified();

            if (node.Parent != null) {
                node.Parent.Nodes.Remove(node);
            } else {
                treeView.Nodes.Remove(node);
            }
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            isDoubleClick = e.Clicks > 1;

            TreeNode selected = treeView.GetNodeAt(e.X, e.Y);

            if (selected != null)
            {
                treeView.SelectedNode = selected;
            }


        }

        public bool selectNodeTab(TreeNode node) {
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

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;
            

            if (selected == null)
            {
                return;
            }

            this.selectNodeTab(selected);
        }

        public void opentNodeTab(TreeNode node)
        {
            if (this.selectNodeTab(node)) {
                return;
            }
            
            TreeData treeData = (TreeData)node.Tag;
            TabData tabData = new TabData();
            tabData.textBox.Focus();
            this.addNewTab(treeData.name, node, tabData);
            this.selectTab(tabData.tabPage);
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;
            

            if (selected == null)
            {
                return;
            }

            this.opentNodeTab(selected);
        }
        
        private void treeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (isDoubleClick && e.Action == TreeViewAction.Collapse)
                e.Cancel = true;
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (isDoubleClick && e.Action == TreeViewAction.Expand)
                e.Cancel = true;
        }

        private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {

            if (e.Node == null)
            {
                return;
            }

            TreeData treeData = this.getNodeData(e.Node);

            //if (treeData.isroot)
            //  e.CancelEdit = true;
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node == null || e.Label == null)
            {
                return;
            }

            TreeData treeData = this.getNodeData(e.Node);

            if (treeData.name != e.Label) { 
    
                treeData.name = e.Label;
                e.Node.Text = e.Label;
                if (treeData.tabData != null && treeData.tabData.tabPage != null) {
                    treeData.tabData.name = e.Label;
                    treeData.tabData.tabPage.Text = e.Label;
                }

                this.Modified();
            }

            this.treeView.LabelEdit = false;
        }


        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            treeView.SelectedNode = treeView.GetNodeAt(targetPoint);

            TreeNode targetNode = treeView.SelectedNode;

            if (targetNode == null)
            {
                return;
            }
            Rectangle targetNodeBounds = treeView.SelectedNode.Bounds;

            int blockSize = targetNodeBounds.Height / 3;

            bool addNodeUp = (targetPoint.Y < targetNodeBounds.Y + blockSize);
            bool addNodeIn = (targetNodeBounds.Y + blockSize <= targetPoint.Y && targetPoint.Y < targetNodeBounds.Y + 2 * blockSize);
            bool addNodeDown = (targetNodeBounds.Y + 2 * blockSize <= targetPoint.Y);

            Graphics g = treeView.CreateGraphics();
            Pen customPen = new Pen(Color.DimGray, 3) { DashStyle = DashStyle.Dash };
            Pen customPen2 = new Pen(SystemColors.Control, 3);

            g.DrawLine(customPen2, new Point(0, targetNode.Bounds.Top + 1), new Point(treeView.Width - 4, targetNode.Bounds.Top + 1));
            g.DrawLine(customPen2, new Point(0, targetNode.Bounds.Bottom - 1), new Point(treeView.Width - 4, targetNode.Bounds.Bottom - 1));

            if (addNodeUp)
            {
                g.DrawLine(customPen, new Point(0, targetNode.Bounds.Top + 1), new Point(treeView.Width - 4, targetNode.Bounds.Top + 1));
            }

            if (addNodeIn)
            {

            }

            if (addNodeDown)
            {
                g.DrawLine(customPen, new Point(0, targetNode.Bounds.Bottom - 1), new Point(treeView.Width - 4, targetNode.Bounds.Bottom - 1));
            }

            customPen.Dispose();
            g.Dispose();
        }

        private void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            SolidBrush myBrush = new SolidBrush(e.Node == treeView.SelectedNode ? Color.FromArgb(204, 204, 255) : SystemColors.Control);
            e.Graphics.FillRectangle(myBrush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, e.Node.ForeColor, TextFormatFlags.GlyphOverhangPadding);
        }

        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {

            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            return ContainsNode(node1, node2.Parent);
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);

            if (targetNode == null)
            {
                return;
            }

            TreeData targetNodeData = (TreeData)targetNode.Tag;

            Rectangle targetNodeBounds = targetNode.Bounds;

            int blockSize = targetNodeBounds.Height / 3;

            bool addNodeUp = (targetPoint.Y < targetNodeBounds.Y + blockSize);
            bool addNodeIn = (targetNodeBounds.Y + blockSize <= targetPoint.Y && targetPoint.Y < targetNodeBounds.Y + 2 * blockSize);
            bool addNodeDown = (targetNodeBounds.Y + 2 * blockSize <= targetPoint.Y);

            if (e.Data.GetDataPresent(DataFormats.Text, false))
            {
                string text = (string)(e.Data.GetData(DataFormats.Text, false));
            }

            if (targetNode == null)
            {
                treeView.Invalidate();
                return;
            }

            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (draggedNode == null)
            {
                treeView.Invalidate();
                return;
            }

            TreeData draggedNodeData = (TreeData)draggedNode.Tag;

            if (ContainsNode(draggedNode, targetNode))
            {
                treeView.Invalidate();
                return;
            }

            if (e.Effect == DragDropEffects.Move)
            {

                if (addNodeUp && !targetNodeData.isroot)
                {
                    int previousNodeIndex = targetNode.Parent.Nodes.IndexOf(targetNode) - 1;

                    // mov to nodebefore target node as last child
                    if (previousNodeIndex >= 0 && targetNode.Parent.Nodes[previousNodeIndex].Nodes.Count > 0 && targetNode.Parent.Nodes[previousNodeIndex].IsExpanded)
                    {
                        TreeNode previousNode = targetNode.Parent.Nodes[previousNodeIndex];
                        draggedNode.Remove();
                        previousNode.Nodes.Add(draggedNode);

                        this.Modified();

                    }
                    else
                    {
                        // move before target node
                        if (!draggedNode.Equals(targetNode))
                        {
                            TreeNode targetParentNode = targetNode.Parent;
                            draggedNode.Remove();
                            int targetNodePosition = targetParentNode.Nodes.IndexOf(targetNode);
                            targetParentNode.Nodes.Insert(targetNodePosition, draggedNode);

                            this.Modified();
                        }
                    }
                }

                // move to target node
                if (addNodeIn && (!draggedNode.Equals(targetNode)))
                {
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                    targetNode.Expand();

                    this.Modified();
                }

                if (addNodeDown && !targetNodeData.isroot)
                {
                    // move to target child node as firs child
                    if (targetNode.Nodes.Count > 0 && targetNode.IsExpanded)
                    {
                        draggedNode.Remove();
                        int targetNodePosition = 0;
                        targetNode.Nodes.Insert(targetNodePosition, draggedNode);

                        this.Modified();

                    }
                    else
                    {
                        // move after target node
                        if (!draggedNode.Equals(targetNode))
                        {
                            TreeNode targetParentNode = targetNode.Parent;
                            draggedNode.Remove();
                            int targetNodePosition = targetParentNode.Nodes.IndexOf(targetNode);
                            targetNode.Parent.Nodes.Insert(targetNodePosition + 1, draggedNode);

                            this.Modified();
                        }
                    }
                }
            }

            treeView.Invalidate();
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                if (treeView.SelectedNode != null)
                {
                    this.treeView.LabelEdit = true;
                    treeView.SelectedNode.BeginEdit();
                }
            }
        }

        // TREEVIEW MENU

        private void addNoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;

            if (selected == null)
            {
                return;
            }

            this.Modified();

            this.addNewNode("Note", selected);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;

            if (selected == null)
            {
                return;
            }

            TreeData treeDate = (TreeData)selected.Tag;

            if (treeDate.isroot)
            {
                return;
            }

            if (this.canRemoveNode(selected))
            {
                this.removeNode(selected);

                if (selected.Parent != null)
                {
                    selected.Parent.Nodes.Remove(selected);
                }
                else
                {
                    treeView.Nodes.Remove(selected);
                }

            }

        }


        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                this.treeView.LabelEdit = true;
                treeView.SelectedNode.BeginEdit();
            }
        }

        private void expandToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                treeView.SelectedNode.ExpandAll();
            }
        }

        private void collapseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                treeView.SelectedNode.Collapse();
            }
        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                TreeData treeData = this.getNodeData(treeView.SelectedNode);

                if (!treeData.isroot && !treeData.folder) {
                    treeData.folder = true;
                    treeData.note = false;

                    treeView.SelectedNode.ImageIndex = 2;
                    treeView.SelectedNode.SelectedImageIndex = 2;
                    treeView.SelectedNode.ImageKey = "folder.png";

                    this.Modified();
                }

                
            }
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                TreeData treeData = this.getNodeData(treeView.SelectedNode);

                if (!treeData.isroot && !treeData.note)
                {
                    treeData.folder = true;
                    treeData.note = false;

                    treeView.SelectedNode.ImageIndex = 1;
                    treeView.SelectedNode.SelectedImageIndex = 1;
                    treeView.SelectedNode.ImageKey = "note.png";
                    this.Modified();
                }


            }
        }

        // TABS

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

        public TabData getTabData(TabPage tab)
        {
            TabData treeData = (TabData)tab.Tag;

            return treeData;
        }

        private const int EM_SETTABSTOPS = 0x00CB;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private TabData addNewTab(string name = "New", TreeNode node = null, TabData tabData = null)
        {
            ++countTabs;

            if (tabData == null) {
                tabData = new TabData();
            }

            tabData.id = Uid.get();
            tabData.name = name;
            tabData.node = node;

            if (node != null)
            {
                TreeData treeData = (TreeData)node.Tag;
                tabData.nodeid = treeData.id;
                tabData.textBox.Text = treeData.text;
                treeData.tabData = tabData;


            }
            else {
                tabData.textBox.Text = tabData.text;
            }

            tabData.tabPage.Text = tabData.name;
            tabData.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            tabData.textBox.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tabData.textBox.Location = new System.Drawing.Point(3, 3);
            tabData.textBox.Name = "textBox" + (countTabs);
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


            tabData.textBox.EmptyUndoBuffer();
            tabData.textBox.StyleClearAll();

            SendMessage(tabData.textBox.Handle, EM_SETTABSTOPS, 1,
            new int[] { 4 * 4 });

            tabData.tabPage.Controls.Add(tabData.textBox);
            tabControl.TabPages.Add(tabData.tabPage);
            tabControl.SelectedTab = tabData.tabPage;
            tabData.tabPage.Tag = tabData;

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
                else {
                    tabData.selected = false;
                }
            }

            tabControl.SelectedTab = tab;
        }

        public void closeTab(TabPage tab)
        {
            int i = 0;
            for (i=0; i < tabControl.TabCount; ++i)
            {
                if (tabControl.GetControl(i) == tab) {
                    break;
                }
            }

            TabData tabData = (TabData)tab.Tag;

            if (tabData.node != null) {
                TreeData nodeData = (TreeData)tabData.node.Tag;
                nodeData.text = ((ScintillaNET.Scintilla)tab.Controls[0]).Text;

                nodeData.tabData = null;
            }

            tabControl.TabPages.Remove(tab);

            if (tabControl.TabCount > 0)
            {
                if (i < tabControl.TabCount)
                {
                    this.selectTab((TabPage)tabControl.GetControl(i));
                }
                else
                {
                    this.selectTab((TabPage)tabControl.GetControl(tabControl.TabCount-1));
                }
            }
        }

        public void renameTab(string name, TabPage tab)
        {
            if (tab == null)
            {
                return;
            }

           

            TabData tadData = (TabData)tab.Tag;

            if (tadData.name != name) {
                this.Modified();
            }

            tadData.name = name;
            tadData.tabPage.Text = name;

            if (tadData.node != null) {
                tadData.node.Text = name;
                TreeData treeData = (TreeData)tadData.node.Tag;
                treeData.name = name;
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage selected = tabControl.SelectedTab;

            if (selected == null)
            {
                return;
            }

            TabData tadData = (TabData)selected.Tag;

            FormRename formRename = new FormRename();

            this.enablePin = false;

            formRename.text = tadData.name;

            formRename.startLocation = new Point(Cursor.Position.X - 100, Cursor.Position.Y - 100);

            formRename.ShowDialog();

            if (formRename.saved) {
                this.renameTab(formRename.text, selected);
            }

            this.enablePin = true;
        }

        private void closeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            TabPage selected = tabControl.SelectedTab;

            if (selected == null)
            {
                return;
            }

            this.closeTab(selected);
        }

        private int getHoverTabIndex(TabControl tc)
        {
            for (int i = 0; i < tc.TabPages.Count; i++)
            {
                if (tc.GetTabRect(i).Contains(tc.PointToClient(Cursor.Position)))
                    return i;
            }

            return -1;
        }

        private void swapTabPages(TabControl tc, TabPage src, TabPage dst)
        {
            int index_src = tc.TabPages.IndexOf(src);
            int index_dst = tc.TabPages.IndexOf(dst);
            tc.TabPages[index_dst] = src;
            tc.TabPages[index_src] = dst;
            tc.Refresh();
        }

        private void tabControl_MouseDown(object sender, MouseEventArgs e)
        {

            for (int i = 0; i < this.tabControl.TabPages.Count; i++)
            {
                Rectangle r = tabControl.GetTabRect(i);
                Rectangle closeButton = new Rectangle(r.Right - 25, r.Top + 4, 20, 20);
                if (closeButton.Contains(e.Location))
                {
                    TabPage tabPage = this.tabControl.TabPages[i];
                    this.closeTab(tabPage);
                    return;
                }
            }

            // store clicked tab
            TabControl tc = (TabControl)sender;
            int hover_index = this.getHoverTabIndex(tc);
            if (hover_index >= 0) { tc.Tag = tc.TabPages[hover_index]; }
        }

        private void tabControl_MouseMove(object sender, MouseEventArgs e)
        {
            // mouse button down? tab was clicked?
            TabControl tc = (TabControl)sender;
            if ((e.Button != MouseButtons.Left) || (tc.Tag == null)) return;
            TabPage clickedTab = (TabPage)tc.Tag;
            int clicked_index = tc.TabPages.IndexOf(clickedTab);

            // start drag n drop
            tc.DoDragDrop(clickedTab, DragDropEffects.All);
        }

        private void tabControl_MouseUp(object sender, MouseEventArgs e)
        {
            // clear stored tab
            TabControl tc = (TabControl)sender;
            tc.Tag = null;
        }

        private void tabControl_DragOver(object sender, DragEventArgs e)
        {
            TabControl tc = (TabControl)sender;

            // a tab is draged?
            if (e.Data.GetData(typeof(TabPage)) == null) return;
            TabPage dragTab = (TabPage)e.Data.GetData(typeof(TabPage));
            int dragTab_index = tc.TabPages.IndexOf(dragTab);

            // hover over a tab?
            int hoverTab_index = this.getHoverTabIndex(tc);
            if (hoverTab_index < 0) { e.Effect = DragDropEffects.None; return; }
            TabPage hoverTab = tc.TabPages[hoverTab_index];
            e.Effect = DragDropEffects.Move;

            // start of drag?
            if (dragTab == hoverTab) return;

            // swap dragTab & hoverTab - avoids toggeling
            Rectangle dragTabRect = tc.GetTabRect(dragTab_index);
            Rectangle hoverTabRect = tc.GetTabRect(hoverTab_index);

            if (dragTabRect.Width < hoverTabRect.Width)
            {
                Point tcLocation = tc.PointToScreen(tc.Location);

                if (dragTab_index < hoverTab_index)
                {
                    if ((e.X - tcLocation.X) > ((hoverTabRect.X + hoverTabRect.Width) - dragTabRect.Width))
                        this.swapTabPages(tc, dragTab, hoverTab);
                }
                else if (dragTab_index > hoverTab_index)
                {
                    if ((e.X - tcLocation.X) < (hoverTabRect.X + dragTabRect.Width))
                        this.swapTabPages(tc, dragTab, hoverTab);
                }
            }
            else this.swapTabPages(tc, dragTab, hoverTab);

            // select new pos of dragTab
            tc.SelectedIndex = tc.TabPages.IndexOf(dragTab);
        }

        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Font drawFont = new Font("Consolas", 10);
            e.Graphics.DrawString(this.tabControl.TabPages[e.Index].Text, drawFont, Brushes.Black, e.Bounds.Left, e.Bounds.Top + 6);

            SolidBrush blueBrush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml("#C2CDB9"));
            Rectangle rect = new Rectangle(e.Bounds.Right - 25, e.Bounds.Top + 4, 20, 20);
            e.Graphics.FillRectangle(blueBrush, rect);

            e.Graphics.DrawString("❌", e.Font, Brushes.Black, e.Bounds.Right - 25, e.Bounds.Top + 4);

            e.DrawFocusRectangle();
        }


        // TEXTBOX
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            ScintillaNET.Scintilla textBox = (ScintillaNET.Scintilla)sender;
            TabData tabData = (TabData)textBox.Tag;

            if (textBox.Text != tabData.text) {
                tabData.text = textBox.Text;

                if (tabData.node != null) {
                    TreeData treeData = (TreeData)tabData.node.Tag;
                    treeData.text = textBox.Text;
                }

                this.Modified();
            }
        }

        // TIMER

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.modified) {
                this.saveConfig();
            }

            timer.Enabled = false;
        }

        // SEARCH


        public List<int> searchInString(string searchFor, string searchIn) {
            List<int> results = new List<int>();

            searchFor = searchFor.ToUpper();
            searchIn = searchIn.ToUpper();

            if (searchFor == "" || searchIn == "") {
                return results;
            }

            int pos = -1;

            while ((pos = searchIn.IndexOf(searchFor, pos+1)) > -1) {
                results.Add(pos);
            }

            return results;
        }

        public List<SearchResult> searchFor(string searchFor = "") {
            List<SearchResult> results = new List<SearchResult>();

            List<TreeNode> treeNodes = this.getAllNodes();

            foreach (TreeNode node in treeNodes) {
                bool foundInTitle = false;
                bool foundInContent = false;

                TreeData treeData = this.getNodeData(node);
                TabData tabData = null;


                string title = node.Text;
                string text = "";

                if (treeData.tabData != null && treeData.tabData.textBox != null)
                {
                    tabData = this.getTabData(treeData.tabData.tabPage);
                    text = tabData.textBox.Text;

                }
                else {
                    if (treeData != null) {
                        text = treeData.text;
                    }
                }

                List<int> positionsInTitle = this.searchInString(searchFor, title);

                foreach (int pos in positionsInTitle)
                {
                    SearchResult result = new SearchResult();
                    result.searchFor = searchFor;
                    result.nodeText = node.Text;
                    result.inTitle = true;
                    result.inContent = false;
                    result.position = pos;
                    result.node = node;
                    result.treeData = treeData;
                    result.tabData = tabData;
                    results.Add(result);
                }

                List<int> positionsInText = this.searchInString(searchFor, text);

                foreach (int pos in positionsInText)
                {
                    SearchResult result = new SearchResult();
                    result.searchFor = searchFor;
                    result.contentText = text;
                    result.inTitle = false;
                    result.inContent = true;
                    result.position = pos;
                    result.node = node;
                    result.treeData = treeData;
                    result.tabData = tabData;
                    results.Add(result);
                }
            }
            
            return results;
        }

        public void GoToPositionInTab(int position, TreeNode node) {
            if (node == null) {
                return;
            }

            TreeData treeData = this.getNodeData(node);

            if (treeData == null)
            {
                return;
            }

            TabData tabData = treeData.tabData;

            if (tabData == null || tabData.textBox == null)
            {
                return;
            }

            tabData.textBox.GotoPosition(position);
        }

        public void locateSearchResult(SearchResult result)
        {
            if (result.node != null) {
                this.treeView.SelectedNode = result.node;
                result.node.EnsureVisible();
                this.Focus();
                this.opentNodeTab(result.node);
                this.GoToPositionInTab(result.position, result.node);
            }
        }

    }
}
