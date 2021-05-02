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
        public static String configFileDirectory = "Notebook";

#if DEBUG
        // global configuration file name in debug mode
        public static String defaultFileName = "default.debug.notebook";
#else
        public static String defaultFileName = "default.notebook";
#endif

        public NotebookState state = null;


        public NotebookFile(NotebookState state)
        {
            this.state = state;
        }

        public static string getDefaultFilePath()
        {
            // use local config file
            string localOptionFilePath = Os.Combine(
                Os.GetCurrentApplicationDirectory(),
                NotebookFile.defaultFileName
            );

            if (Os.FileExists(localOptionFilePath))
            {
                return localOptionFilePath;
            }
            else
            {

                string globalConfigDirectory = Os.Combine(
                    Os.GetApplicationsDirectory(),
                    NotebookFile.configFileDirectory
                );

                // create global config directory if not exist
                if (!Os.DirectoryExists(globalConfigDirectory))
                {
                    Os.CreateDirectory(globalConfigDirectory);
                }

                return Os.Combine(
                    globalConfigDirectory,
                    NotebookFile.defaultFileName
                );
            }
        }


        public static bool defaultNotebookFileExists() {

            string defaultFilePath = NotebookFile.getDefaultFilePath();

            if (Os.FileExists(defaultFilePath))
            {
                
                return true;
            }

            return false;
        }

        public void LoadNotebookFile()
        {
            if (Os.FileExists(this.state.path))
            {
                this.LoadXmlNotebookFile();
            }
        }

        private void LoadXmlNotebookFile()
        {
            try
            {
                if (Os.FileExists(this.state.path))
                {

                    string xml = Os.GetFileContent(this.state.path);

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
                    this.state.lastSave = Convert.StringToLong(option.Value);
                }


                if (option.Name.ToString() == "splitDistance")
                {
                    this.state.splitDistance = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsX")
                {
                    this.state.windowsX = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsY")
                {
                    this.state.windowsY = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsWidth")
                {
                    this.state.windowsWidth = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsHeight")
                {
                    this.state.windowsHeight = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsState")
                {
                    this.state.windowsState = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "pinX")
                {
                    this.state.pinX = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "pinY")
                {
                    this.state.pinY = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "nodes" && option.HasElements)
                {
                    this.loadNodes(option, this.state.nodesData);
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
                                        .Replace("\r", "\n");
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

                            this.state.tabsData.Add(tabData);
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
                                .Replace("\r", "\n");
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

                        if (nodeElement.Name.ToString() == "url")
                        {
                            treeData.isUrl = nodeElement.Value == "1";
                        }

                        if (nodeElement.Name.ToString() == "link")
                        {
                            treeData.isLink = nodeElement.Value == "1";
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

        public void SaveNotebookFile()
        {
            if (this.state.path == "") {
                return;
            }

            try
            {
                XElement root = this.SaveParams();

                System.IO.StreamWriter file = new System.IO.StreamWriter(this.state.path);

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
            root.Add(new XElement("splitDistance", this.state.splitDistance.ToString()));

            root.Add(new XElement("windowsX", this.state.windowsX.ToString()));
            root.Add(new XElement("windowsY", this.state.windowsY.ToString()));
            root.Add(new XElement("windowsWidth", this.state.windowsWidth.ToString()));
            root.Add(new XElement("windowsHeight", this.state.windowsHeight.ToString()));
            root.Add(new XElement("windowsState", this.state.windowsState.ToString()));
            root.Add(new XElement("pinX", this.state.pinX.ToString()));
            root.Add(new XElement("pinY", this.state.pinY.ToString()));

            XElement nodesNode = new XElement("nodes");

            //////////////
            
            this.saveNodes(nodesNode, this.state.nodesData);
            root.Add(nodesNode);

            //////////////

            XElement tabsNode = new XElement("tabs");

            foreach (TabData data in this.state.tabsData)
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
                nodeNode.Add(new XElement("url", treeData.isUrl ? "1" : "0"));
                nodeNode.Add(new XElement("link", treeData.isLink ? "1" : "0"));

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
