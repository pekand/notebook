using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ScintillaNET;

namespace Notebook
{
    public partial class FormNotebook : Form
    {

        private NotebookState state = null;

        private bool isDoubleClick = false;

        private bool saveWindowPosition = false;

        public FormNotebook()
        {
            InitializeComponent();
        }

        // FORM
        private void FormNotebook_Load(object sender, EventArgs e)
        {
            this.TopMost = Program.options.mostTop;

            this.tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;

            // remove example pages in designer
            tabControl.TabPages.Remove(tabPageSettings);
            tabControl.TabPages.Remove(tabPageEdit);
            tabControl.SelectedTab = null;


            if (NotebookFile.defaultNotebookFileExists())
            {

                this.openNotebookFile(NotebookFile.getDefaultFilePath());
            }
            else {
                this.newNotebookFile();

                this.state.path = NotebookFile.getDefaultFilePath();
            }

            this.restoreWindowPosition();
            this.SetTitle();
        }

        private void restoreWindowPosition() {

            if (this.state.windowsWidth == 0 || this.state.windowsHeight == 0)
            {
                this.Left = 100;
                this.Top = 100;
                this.Width = 500;
                this.Height = 500;
                return;
            }

            this.Left = this.state.windowsX;
            this.Top = this.state.windowsY;
            this.Width = this.state.windowsWidth;
            this.Height = this.state.windowsHeight;


            if (this.state.windowsState == 1)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            if (this.state.windowsState == 2)
            {
                this.WindowState = FormWindowState.Minimized;
            }

            if (this.state.windowsState == 3)
            {
                this.WindowState = FormWindowState.Normal;
            }

            this.splitContainer.SplitterDistance = this.state.splitDistance;

            saveWindowPosition = true;
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
                    TabData tabData = this.state.getTabData(tab);
                    if (tabData.node!= null)
                    {
                        TreeData treeData = this.state.getNodeData(tabData.node);
                        treeData.text = ((ScintillaNET.Scintilla)tab.Controls[0]).Text;
                    }
                }
            }

            this.saveNotebook();
            Program.options.SaveConfigFile();
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (saveWindowPosition)
            {
                this.state.splitDistance = this.splitContainer.SplitterDistance;
            }
        }

        private void FormNotebook_Move(object sender, EventArgs e)
        {
            if (saveWindowPosition)
            {

                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.state.windowsState = 1;
                    return;
                }

                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.state.windowsState = 2;
                    return;
                }

                if (this.WindowState == FormWindowState.Normal)
                {
                    this.state.windowsState = 3;
                }

                this.state.windowsX = this.Left;
                this.state.windowsY = this.Top;
                this.state.windowsWidth = this.Width;
                this.state.windowsHeight = this.Height;
            }

        }

        private void SetTitle()
        {
            string path = "";

            if (this.state.path != "") {
                path = " - " + this.state.path;
            }

            this.Text = "Notebook" + (this.state.isModified() ? "*" : "") + path;
        }


        private void autosaveTimer() {
            timer.Enabled = false;
            timer.Interval = 50000;
            timer.Enabled = true;
        }

        private void Modified() {

            if (!this.state.isModified()) {
                this.state.Modified();
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
            return
                Program.options.enablePin &&
                enablePin && 
                !isPinned && 
                (this.formSearch == null || !this.formSearch.Visible) &&
                !this.dialogShown
            ;
        }
        private void ShowPin() {

            if (!CanPin())
            {
                return;
            }

            isPinned = true;


            if (this.state.pinX == -1 && this.state.pinY == -1) {
                formPopup.Left = this.Left + this.Width - 50;
                formPopup.Top = this.Top + this.Height - 50;
            } else {
                formPopup.Left = this.state.pinX;
                formPopup.Top = this.state.pinY;
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


        private bool dialogShown = false;
        private FormSearch formSearch = null;

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

            this.state.pinX = formPopup.Left;
            this.state.pinY = formPopup.Top;

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
            this.newNotebookFile();
        }

        public void newNotebookFile()
        {
            if (this.state != null) {
                this.state.unloadCurentState();
                this.state = null;
            }

            NotebookState notebookState = new NotebookState(this.treeView, this.tabControl);
            NotebookFile notebook = new NotebookFile(notebookState);
            this.state = notebookState;
            this.state.setState(notebookState);

            if (this.state.treeView.Nodes.Count == 0)
            {
                TreeNode rootNode = this.state.addRootNode();
                this.state.addNewTab(rootNode.Name, rootNode);
            }

            this.SetTitle();
        }

        public void openNotebookFile(string path) {

            if (this.state != null)
            {
                this.state.unloadCurentState();
                this.state = null;
            }

            NotebookState notebookState = new NotebookState(this.treeView, this.tabControl);
            notebookState.path = path;

            NotebookFile notebook = new NotebookFile(notebookState);
            notebook.LoadNotebookFile();

            this.state = notebookState;
            this.state.setState(notebookState);
            this.SetTitle();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dialogShown = true;
            
            DialogResult result = this.openFileDialog.ShowDialog();

            if (result == DialogResult.OK) { // check if file exists
                string path = this.openFileDialog.FileName;
                this.openNotebookFile(path);
                
            }

            this.dialogShown = true;
        }

        public void saveNotebook()
        {
            this.state.copyStateFromControls();
            NotebookFile notebook = new NotebookFile(this.state);
            notebook.SaveNotebookFile();

            this.state.unModified();
            this.SetTitle();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.saveNotebook();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.dialogShown = true;

            DialogResult result = this.saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.state.path = this.saveFileDialog.FileName;
                this.saveNotebook();
            }

            this.dialogShown = true;
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


        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchFor = "";

            if (this.tabControl.SelectedTab != null) {
                TabData tabData = this.state.getTabData(this.tabControl.SelectedTab);
                searchFor = tabData.textBox.SelectedText;
            }

            formSearch = new FormSearch(searchFor);
            formSearch.startLocation = new Point(Cursor.Position.X - 100, Cursor.Position.Y-100);
            formSearch.Owner = this;
            formSearch.Show();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pinWindowToolStripMenuItem.Checked = Program.options.enablePin;
            mostTopToolStripMenuItem.Checked = Program.options.mostTop;
        }

        private void mostTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.options.mostTop = !Program.options.mostTop;
            this.TopMost = Program.options.mostTop;
            mostTopToolStripMenuItem.Checked = Program.options.mostTop;
        }

        private void pinWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.options.enablePin = !Program.options.enablePin;
            pinWindowToolStripMenuItem.Checked = Program.options.enablePin;
        }

        // SHORTCUT

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.W))
            {

                TabPage tab = tabControl.SelectedTab;

                if (tab != null)
                {
                    this.state.closeTab(tab);
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

            if (keyData == (Keys.F12))
            {
                Program.console.Show();
            }

            

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // SETTINGS

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {

        }

        // TREEVIEW

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            isDoubleClick = e.Clicks > 1;

            TreeNode selected = treeView.GetNodeAt(e.X, e.Y);

            if (selected != null)
            {
                treeView.SelectedNode = selected;
            }


        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;
            

            if (selected == null)
            {
                return;
            }

            this.state.selectNodeTab(selected);
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;
            
            
            if (selected == null)
            {
                return;
            }


            TreeData treeData = this.state.getNodeData(selected);

            if (treeData.isUrl)
            {
                Network.OpenUrl(treeData.text);
            } 
            else if (treeData.isLink) 
            {
                Os.Process.OpenDirectory(treeData.text);
            } 
            else if (treeData.isroot || treeData.note || treeData.folder)
            {
                this.state.opentNodeTab(selected);
            }
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

        private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            this.state.collapseNode(e.Node);

        }

        private void treeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            this.state.expandNode(e.Node);
        }

        private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {

            if (e.Node == null)
            {
                return;
            }

            TreeData treeData = this.state.getNodeData(e.Node);

            //if (treeData.isroot)
            //  e.CancelEdit = true;
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node == null || e.Label == null)
            {
                return;
            }

            TreeData treeData = this.state.getNodeData(e.Node);

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

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(new Point(e.X, e.Y));

            TreeNode targetNode = treeView.GetNodeAt(targetPoint);

            if (targetNode == null)
            {
                return;
            }

            TreeData targetNodeData = (TreeData)targetNode.Tag;

            string action = "";
            string text = "";


            if (e.Data.GetDataPresent("UniformResourceLocator", false)) {
                object data = e.Data.GetData("UniformResourceLocator");
                MemoryStream ms = data as MemoryStream;
                byte[] bytes = ms.ToArray();
                Encoding encod = Encoding.ASCII;
                String url = encod.GetString(bytes);
                text = url.Substring(0, url.IndexOf('\0'));

                TreeNode newNode = this.state.addNewNode("Note", targetNode, "url");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;
            } else if (e.Data.GetDataPresent(DataFormats.Html, false))
            {
                text = (string)e.Data.GetData(DataFormats.Text);
                TreeNode newNode = this.state.addNewNode("Note", targetNode, "note");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;
            }
            else  if (e.Data.GetDataPresent(DataFormats.UnicodeText, false)) {
                text = (string)e.Data.GetData(DataFormats.UnicodeText);
                TreeNode newNode =  this.state.addNewNode("Note", targetNode, "note");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;
            } else if (e.Data.GetDataPresent(DataFormats.Text, false)) {
                text = (string)e.Data.GetData(DataFormats.Text);
                TreeNode newNode = this.state.addNewNode("Note", targetNode, "note");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;
            } else if (e.Data.GetDataPresent(DataFormats.FileDrop, false)) {
                string[] files = (string[])(e.Data.GetData(DataFormats.FileDrop, false));

                foreach (string file in files)
                {
                    string path = file;

                    TreeNode newNode = this.state.addNewNode(Os.Path.File.GetFileName(path), targetNode, "link");
                    TreeData newNodeData = this.state.getNodeData(newNode);
                    newNodeData.text = path;
                }
            }

            Rectangle targetNodeBounds = targetNode.Bounds;

            int blockSize = targetNodeBounds.Height / 3;

            bool addNodeUp = (targetPoint.Y < targetNodeBounds.Y + blockSize);
            bool addNodeIn = (targetNodeBounds.Y + blockSize <= targetPoint.Y && targetPoint.Y < targetNodeBounds.Y + 2 * blockSize);
            bool addNodeDown = (targetNodeBounds.Y + 2 * blockSize <= targetPoint.Y);

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

            if (this.state.ContainsNode(draggedNode, targetNode))
            {
                treeView.Invalidate();
                return;
            }

            if (e.Effect == DragDropEffects.Move)
            {

                if (addNodeUp && !targetNodeData.isroot)
                {
                    int previousNodeIndex = targetNode.Parent.Nodes.IndexOf(targetNode) - 1;

                    // move to node before target node as last child
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

            this.state.addNewNode("Note", selected);
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

            if (this.state.canRemoveNode(selected))
            {
                this.state.removeNode(selected);

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

                TreeData treeData = this.state.getNodeData(treeView.SelectedNode);

                if (treeData.isroot) {
                    foreach (TreeNode node in treeView.SelectedNode.Nodes) {
                        node.Collapse();
                    }
                }
                else {
                    treeView.SelectedNode.Collapse();
                }
            }
        }

        public class NodeSorter : IComparer
        {
            TreeNode parentNode = null;

            public NodeSorter(TreeNode parentNode) {
                this.parentNode = parentNode;
            }

            public int Compare(object x, object y)
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;

                if (tx.Parent != this.parentNode || ty.Parent != this.parentNode) {
                    return 0;
                }

                if (tx.Name == null || ty.Name == null)
                    return 0;

                int comparison = String.Compare(tx.Text, ty.Text, comparisonType: StringComparison.OrdinalIgnoreCase);

                if (comparison < 0)
                    return -1;
                else if (comparison > 0)
                    return 1;
                else
                    return 0;
            }
        }

        private void sortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            treeView.TreeViewNodeSorter = new NodeSorter(treeView.SelectedNode);
            treeView.Sort();
        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                TreeData treeData = this.state.getNodeData(treeView.SelectedNode);

                if (!treeData.isroot && !treeData.folder) {
                    treeData.folder = true;
                    treeData.note = false;
                    treeData.isLink = false;
                    treeData.isUrl = false;

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
                TreeData treeData = this.state.getNodeData(treeView.SelectedNode);

                if (!treeData.isroot && !treeData.note)
                {
                    treeData.folder = false;
                    treeData.note = true;
                    treeData.isLink = false;
                    treeData.isUrl = false;

                    treeView.SelectedNode.ImageIndex = 1;
                    treeView.SelectedNode.SelectedImageIndex = 1;
                    treeView.SelectedNode.ImageKey = "note.png";
                    this.Modified();
                }


            }
        }

        private void urlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                TreeData treeData = this.state.getNodeData(treeView.SelectedNode);

                if (!treeData.isroot && !treeData.isUrl)
                {
                    treeData.folder = false;
                    treeData.note = false;
                    treeData.isLink = false;
                    treeData.isUrl = true;

                    treeView.SelectedNode.ImageIndex = 4;
                    treeView.SelectedNode.SelectedImageIndex = 4;
                    treeView.SelectedNode.ImageKey = "url.png";
                    this.Modified();
                }
            }
        }

        private void linkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode != null)
            {
                TreeData treeData = this.state.getNodeData(treeView.SelectedNode);

                if (!treeData.isroot && !treeData.isLink)
                {
                    treeData.folder = false;
                    treeData.note = false;
                    treeData.isLink = true;
                    treeData.isUrl = false;

                    treeView.SelectedNode.ImageIndex = 3;
                    treeView.SelectedNode.SelectedImageIndex = 3;
                    treeView.SelectedNode.ImageKey = "link.png";
                    this.Modified();
                }
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selected = treeView.SelectedNode;


            if (selected == null)
            {
                return;
            }

            this.state.opentNodeTab(selected);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TreeNode targetNode = treeView.SelectedNode;

            if (targetNode == null)
            {
                return;
            }

            IDataObject data_object = Clipboard.GetDataObject();

            if (data_object.GetDataPresent("UniformResourceLocator", false))
            {
                object data = data_object.GetData("UniformResourceLocator");
                MemoryStream ms = data as MemoryStream;
                byte[] bytes = ms.ToArray();
                Encoding encod = Encoding.ASCII;
                String url = encod.GetString(bytes);
                string text = url.Substring(0, url.IndexOf('\0'));

                TreeNode newNode = this.state.addNewNode("Note", targetNode, "url");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;
            }
            else if (data_object.GetDataPresent(DataFormats.Html, false))
            {
                string text = (string)data_object.GetData(DataFormats.Text);
                TreeNode newNode = this.state.addNewNode("Note", targetNode, "note");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;
            }
            else if (data_object.GetDataPresent(DataFormats.UnicodeText, false))
            {
                string text = (string)data_object.GetData(DataFormats.UnicodeText);
                TreeNode newNode = this.state.addNewNode("Note", targetNode, "note");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;

                if (Patterns.IsURL(text))
                {
                    this.state.setType(newNode, "url");
                    string title = Network.GetWebPageTitle(text);
                    newNode.Text = title;
                    newNodeData.name = title;

                }
                else if (Os.Path.File.Exists(text))
                {
                    this.state.setType(newNode, "link");
                    string title = Os.Path.File.GetFileNameWithoutExtension(text);
                    newNode.Text = title;
                    newNodeData.name = title;
                }
                else if (Os.Path.Directory.Exists(text))
                {
                    this.state.setType(newNode, "link");
                    string title = Os.Path.Directory.GetDirectoryName(text);
                    newNode.Text = title;
                    newNodeData.name = title;
                }
            }
            else if (data_object.GetDataPresent(DataFormats.Text, false))
            {
                string text = (string)data_object.GetData(DataFormats.Text);
                TreeNode newNode = this.state.addNewNode("Note", targetNode, "note");
                TreeData newNodeData = this.state.getNodeData(newNode);
                newNodeData.text = text;

                if (Patterns.IsURL(text))
                {
                    this.state.setType(newNode, "url");
                    string title = Network.GetWebPageTitle(text);
                    newNode.Text = title;
                    newNodeData.name = title;
                }
                else if (Os.Path.File.Exists(text))
                {
                    this.state.setType(newNode, "link");
                    string title = Os.Path.File.GetFileNameWithoutExtension(text);
                    newNode.Text = title;
                    newNodeData.name = title;
                }
                else if (Os.Path.Directory.Exists(text))
                {
                    this.state.setType(newNode, "link");
                    string title = Os.Path.Directory.GetDirectoryName(text);
                    newNode.Text = title;
                    newNodeData.name = title;
                }
            } else if (data_object.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) data_object.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    string path = file;

                    TreeNode newNode = this.state.addNewNode(Os.Path.File.GetFileName(path), targetNode, "link");
                    TreeData newNodeData = this.state.getNodeData(newNode);
                    newNodeData.text = path;
                }
            }
        }

        // TABS

        private void tabControl_MouseDown(object sender, MouseEventArgs e)
        {

            TabPage checkTab = null;

            for (int i = 0; i < this.tabControl.TabPages.Count; ++i)
            {
                if (this.tabControl.GetTabRect(i).Contains(e.Location))
                {
                    checkTab = tabControl.TabPages[i];
                    break;
                }
            }

            if (checkTab != null) {
                this.state.selectTab(checkTab);
            }


            for (int i = 0; i < this.tabControl.TabPages.Count; i++)
            {
                Rectangle r = tabControl.GetTabRect(i);
                Rectangle closeButton = new Rectangle(r.Right - 25, r.Top + 4, 20, 20);
                if (closeButton.Contains(e.Location))
                {
                    TabPage tabPage = this.tabControl.TabPages[i];
                    this.state.closeTab(tabPage);
                    return;
                }
            }

            // store clicked tab
            TabControl tc = (TabControl)sender;
            int hover_index = this.state.getHoverTabIndex();
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
            int hoverTab_index = this.state.getHoverTabIndex();
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
                        this.state.swapTabPages(dragTab, hoverTab);
                }
                else if (dragTab_index > hoverTab_index)
                {
                    if ((e.X - tcLocation.X) < (hoverTabRect.X + dragTabRect.Width))
                        this.state.swapTabPages(dragTab, hoverTab);
                }
            }
            else this.state.swapTabPages(dragTab, hoverTab);

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


        // TABS MENU

        private void locateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage selected = tabControl.SelectedTab;

            if (selected == null)
            {
                return;
            }

            TabData tadData = (TabData)selected.Tag;


            if (tadData.node == null) {
                return;
            }

            this.treeView.SuspendLayout();
            this.treeView.SelectedNode = tadData.node;
            TreeNode node = this.treeView.SelectedNode.Parent;
            while (node != null) {
                node.Expand();
                node = node.Parent;
            }
            this.treeView.ResumeLayout();

            tadData.node.EnsureVisible();
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

            if (formRename.saved)
            {
                this.state.renameTab(formRename.text, selected);
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

            this.state.closeTab(selected);
        }

        private void insertTimestampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == null)
            {
                return;
            }

            TabData tabData = this.state.getTabData(tabControl.SelectedTab);
            tabData.textBox.ReplaceSelection(" " + DateTime.Now.ToString("s") + " ");
        }

        // TIMER

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.state.isModified()) {
                this.saveNotebook();
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

            List<TreeNode> treeNodes = this.state.getAllNodes();

            foreach (TreeNode node in treeNodes) {
                bool foundInTitle = false;
                bool foundInContent = false;

                TreeData treeData = this.state.getNodeData(node);
                TabData tabData = null;


                string title = node.Text;
                string text = "";

                if (treeData.tabData != null && treeData.tabData.textBox != null)
                {
                    tabData = this.state.getTabData(treeData.tabData.tabPage);
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

            TreeData treeData = this.state.getNodeData(node);

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
                this.state.opentNodeTab(result.node);
                this.GoToPositionInTab(result.position, result.node);
            }
        }


    }
}
