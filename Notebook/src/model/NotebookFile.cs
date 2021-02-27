using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Notebook
{

    public class NotebookFile
    {

        
        public NotebookState notebook = null;


        public NotebookFile(NotebookState notebook)
        {
            this.notebook = notebook;
        }

        public void LoadNotebookFile(string path)
        {
            if (Os.FileExists(path))
            {
                this.notebook.path = path;
                this.LoadXmlNotebookFile();
            }
        }

        private void LoadXmlNotebookFile()
        {
            try
            {
                if (Os.FileExists(this.notebook.path))
                {

                    string xml = Os.GetFileContent(this.notebook.path);

                    XmlReaderSettings xws = new XmlReaderSettings
                    {
                        CheckCharacters = false
                    };


                    using (XmlReader xr = XmlReader.Create(new StringReader(xml), xws))
                    {
                        XElement root = XElement.Load(xr);

                        this.LoadParams(root);
                    }

                }

            }
            catch (Exception ex)
            {

            }

        }

        public void LoadParams(XElement root)
        {

            foreach (XElement option in root.Elements())
            {
                if (option.Name.ToString() == "lastSave")
                {
                    this.notebook.lastSave = Convert.StringToLong(option.Value);
                }


                if (option.Name.ToString() == "splitDistance")
                {
                    this.notebook.splitDistance = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsX")
                {
                    this.notebook.windowsX = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsY")
                {
                    this.notebook.windowsY = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsWidth")
                {
                    this.notebook.windowsWidth = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsHeight")
                {
                    this.notebook.windowsHeight = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsState")
                {
                    this.notebook.windowsState = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "pinX")
                {
                    this.notebook.pinX = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "pinY")
                {
                    this.notebook.pinY = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "nodes" && option.HasElements)
                {
                    this.loadNodes(option, this.notebook.nodesData);
                }

                if (option.Name.ToString() == "tabs" && option.HasElements)
                {

                    foreach (XElement tabsElement in option.Elements())
                    {
                        if (tabsElement.Name.ToString() == "tab" && tabsElement.HasElements)
                        {
                            TabData tabData = new TabData();

                            foreach (XElement tabElement in tabsElement.Elements())
                            {

                                if (tabElement.Name.ToString() == "id")
                                {
                                    tabData.id = tabElement.Value;
                                }

                                if (tabElement.Name.ToString() == "nodeid")
                                {
                                    tabData.nodeid = tabElement.Value;
                                }

                                if (tabElement.Name.ToString() == "name")
                                {
                                    tabData.name = tabElement.Value;
                                }

                                if (tabElement.Name.ToString() == "text")
                                {
                                    tabData.text = tabElement.Value
                                        .Replace("\r\n", "\n")
                                        .Replace("\r", "\n")
                                        .Replace("\n", "\r\n");
                                }

                                if (tabElement.Name.ToString() == "selected")
                                {
                                    tabData.selected = tabElement.Value == "1";
                                }

                                if (tabElement.Name.ToString() == "saved")
                                {
                                    tabData.saved = tabElement.Value == "1";
                                }
                            }

                            this.notebook.tabsData.Add(tabData);
                        }
                    }
                }
            }
        }

        public void loadNodes(XElement parent, List<TreeData> nodes)
        {

            foreach (XElement nodesElement in parent.Elements())
            {
                if (nodesElement.Name.ToString() == "node" && nodesElement.HasElements)
                {
                    TreeData treeData = new TreeData();

                    foreach (XElement nodeElement in nodesElement.Elements())
                    {
                        if (nodeElement.Name.ToString() == "id")
                        {
                            treeData.id = nodeElement.Value;
                        }

                        if (nodeElement.Name.ToString() == "name")
                        {
                            treeData.name = nodeElement.Value;
                        }


                        if (nodeElement.Name.ToString() == "text")
                        {
                            treeData.text = nodeElement.Value
                                .Replace("\r\n", "\n")
                                .Replace("\r", "\n")
                                .Replace("\n", "\r\n");
                        }

                        if (nodeElement.Name.ToString() == "expanded")
                        {
                            treeData.expanded = nodeElement.Value == "1";
                        }

                        if (nodeElement.Name.ToString() == "isroot")
                        {
                            treeData.isroot = nodeElement.Value == "1";
                        }

                        if (nodeElement.Name.ToString() == "folder")
                        {
                            treeData.folder = nodeElement.Value == "1";
                        }

                        if (nodeElement.Name.ToString() == "note")
                        {
                            treeData.note = nodeElement.Value == "1";
                        }

                        if (nodeElement.Name.ToString() == "childs" && nodeElement.HasElements)
                        {
                            this.loadNodes(nodeElement, treeData.childs);
                        }

                    }

                    nodes.Add(treeData);
                }
            }
        }

        public void SaveNotebookFile(string path)
        {
            if (path == "") {
                return;
            }

            try
            {
                XElement root = this.SaveParams();

                System.IO.StreamWriter file = new System.IO.StreamWriter(path);

                string xml = "";

                StringBuilder sb = new StringBuilder();
                XmlWriterSettings xws = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    CheckCharacters = false,
                    Indent = true
                };

                using (XmlWriter xw = XmlWriter.Create(sb, xws))
                {
                    root.WriteTo(xw);
                }

                xml = sb.ToString();

                file.Write(xml);
                file.Close();
            }
            catch (Exception ex)
            {

            }
        }

        public XElement SaveParams()
        {
            XElement root = new XElement("notebook");

            root.Add(new XElement("lastSave", Time.getUnixTime()));
            root.Add(new XElement("splitDistance", this.notebook.splitDistance.ToString()));

            root.Add(new XElement("windowsX", this.notebook.windowsX.ToString()));
            root.Add(new XElement("windowsY", this.notebook.windowsY.ToString()));
            root.Add(new XElement("windowsWidth", this.notebook.windowsWidth.ToString()));
            root.Add(new XElement("windowsHeight", this.notebook.windowsHeight.ToString()));
            root.Add(new XElement("windowsState", this.notebook.windowsState.ToString()));
            root.Add(new XElement("pinX", this.notebook.pinX.ToString()));
            root.Add(new XElement("pinY", this.notebook.pinY.ToString()));

            XElement nodesNode = new XElement("nodes");

            //////////////
            
            this.saveNodes(nodesNode, this.notebook.nodesData);
            root.Add(nodesNode);

            //////////////

            XElement tabsNode = new XElement("tabs");

            foreach (TabData data in this.notebook.tabsData)
            {
                XElement tabNode = new XElement("tab");

                tabNode.Add(new XElement("id", data.id));

                if (data.nodeid != "")
                {
                    tabNode.Add(new XElement("nodeid", data.nodeid));
                }

                tabNode.Add(new XElement("name", data.name));
                tabNode.Add(new XElement("text", data.text.Replace("\r\n", "\n")));
                tabNode.Add(new XElement("selected", data.selected ? "1" : "0"));
                tabNode.Add(new XElement("saved", data.saved ? "1" : "0"));

                tabsNode.Add(tabNode);
            }

            root.Add(tabsNode);

            return root;
        }

        public void saveNodes(XElement parent, List<TreeData> nodesData)
        {
            foreach (TreeData treeData in nodesData)
            {
                XElement nodeNode = new XElement("node");

                nodeNode.Add(new XElement("id", treeData.id));
                nodeNode.Add(new XElement("name", treeData.name));
                nodeNode.Add(new XElement("text", treeData.text.Replace("\r\n", "\n")));
                nodeNode.Add(new XElement("expanded", treeData.expanded ? "1" : "0"));
                nodeNode.Add(new XElement("isroot", treeData.isroot ? "1" : "0"));
                nodeNode.Add(new XElement("folder", treeData.folder ? "1" : "0"));
                nodeNode.Add(new XElement("note", treeData.note ? "1" : "0"));

                if (treeData.childs.Count > 0)
                {
                    XElement childsNode = new XElement("childs");

                    this.saveNodes(childsNode, treeData.childs);

                    nodeNode.Add(childsNode);
                }

                parent.Add(nodeNode);
            }
        }
    }
}
