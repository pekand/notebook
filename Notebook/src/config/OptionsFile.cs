using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Notebook
{

    public class OptionsFile
    {
        /*************************************************************************************************************************/
        public String configFileDirectory = "Notebook";

#if DEBUG
        // global configuration file name in debug mode
        public String configFileName = "notebook.debug.xml";
#else
        public String configFileName = "noebook.xml";
#endif

        public String optionsFilePath = ""; 

        public Options Options = null;
        private List<TabData> tabsData = new List<TabData>();
        TreeNodeCollection nodes = null;

        /*************************************************************************************************************************/


        public OptionsFile(Options options, List<TabData> tabsData, TreeNodeCollection nodes)
        {
            this.Options = options;
            this.tabsData = tabsData;
            this.nodes = nodes;

            // use local config file
            string localOptionFilePath = Os.Combine(
                Os.GetCurrentApplicationDirectory(), 
                this.configFileName
            );

            if (Os.FileExists(localOptionFilePath))
            {
                this.optionsFilePath = localOptionFilePath;
            }
            else {

                string globalConfigDirectory = Os.Combine(
                    Os.GetApplicationsDirectory(),
                    this.configFileDirectory
                );

                // create global config directory if not exist
                if (!Os.DirectoryExists(globalConfigDirectory))
                {
                    Os.CreateDirectory(globalConfigDirectory);
                }

                this.optionsFilePath = Os.Combine(
                    globalConfigDirectory,
                    this.configFileName
                );
            }
        }

        public void LoadConfigFile()
        {
            // use global config file if local version not exist
            if (!Os.FileExists(this.optionsFilePath))
            {
                this.optionsFilePath = Os.Combine(
                    this.GetGlobalConfigDirectory(),
                    this.configFileName
                );
            }

            // open config file if exist
            if (Os.FileExists(this.optionsFilePath))
            {
                this.LoadXmlConfigFile();
            }
        }

        private void LoadXmlConfigFile()
        {
            try
            {
                if (Os.FileExists(this.optionsFilePath))
                {

                    string xml = Os.GetFileContent(this.optionsFilePath);

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

        /// <summary>
        /// load configuration</summary>
        public void LoadParams(XElement root)
        {
            
            foreach (XElement option in root.Elements())
            {
                if (option.Name.ToString() == "lastSave")
                {
                    this.Options.lastSave = Convert.StringToLong(option.Value);
                }


                if (option.Name.ToString() == "splitDistance")
                {
                    this.Options.splitDistance = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsX")
                {
                    this.Options.windowsX = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsY")
                {
                    this.Options.windowsY = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsWidth")
                {
                    this.Options.windowsWidth = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsHeight")
                {
                    this.Options.windowsHeight = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "windowsState")
                {
                    this.Options.windowsState = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "pinX")
                {
                    this.Options.pinX = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "pinY")
                {
                    this.Options.pinY = Convert.StringToInt(option.Value, 200);
                }

                if (option.Name.ToString() == "recentFiles" && option.HasElements)
                {
                    this.Options.recentFiles.Clear();

                    foreach (XElement recentFileOption in option.Elements())
                    {
                        if (recentFileOption.Name.ToString() == "item")
                        {
                            this.Options.recentFiles.Add(recentFileOption.Value);
                        }
                    }
                }

                if (option.Name.ToString() == "nodes" && option.HasElements)
                {
                    this.loadNodes(option, this.nodes);
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

                                if (tabElement.Name.ToString() == "path")
                                {
                                    tabData.path = tabElement.Value;
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

                            this.tabsData.Add(tabData);
                        }
                    }
                }
            }
            
            this.Options.RemoveOldRecentFiles();
        }

        public void loadNodes(XElement parent, TreeNodeCollection nodes) {

            foreach (XElement nodesElement in parent.Elements())
            {
                if (nodesElement.Name.ToString() == "node" && nodesElement.HasElements)
                {
                    TreeNode treeNode = new TreeNode();
                    TreeData treeData = new TreeData();
                    treeNode.Tag = treeData;

                    foreach (XElement nodeElement in nodesElement.Elements())
                    {
                        if (nodeElement.Name.ToString() == "id")
                        {
                            treeData.id = nodeElement.Value;
                        }

                        if (nodeElement.Name.ToString() == "name")
                        {
                            treeData.name = nodeElement.Value;
                            treeNode.Text = treeData.name;
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

                            this.loadNodes(nodeElement, treeNode.Nodes);
                        }

                    }

                    if (treeData.expanded)
                    {
                        treeNode.Expand();
                    }

                    nodes.Add(treeNode);
                }
            }
        }


        public void SaveConfigFile()
        {
            try
            {
                XElement root = this.SaveParams();
                
                System.IO.StreamWriter file = new System.IO.StreamWriter(this.optionsFilePath);

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
            XElement root = new XElement("configuration");

            root.Add(new XElement("lastSave", Time.getUnixTime()));
            root.Add(new XElement("splitDistance", this.Options.splitDistance.ToString()));

            root.Add(new XElement("windowsX", this.Options.windowsX.ToString()));
            root.Add(new XElement("windowsY", this.Options.windowsY.ToString()));
            root.Add(new XElement("windowsWidth", this.Options.windowsWidth.ToString()));
            root.Add(new XElement("windowsHeight", this.Options.windowsHeight.ToString()));
            root.Add(new XElement("windowsState", this.Options.windowsState.ToString()));
            root.Add(new XElement("pinX", this.Options.pinX.ToString()));
            root.Add(new XElement("pinY", this.Options.pinY.ToString()));


            /////////////////
            ///
            Options.RemoveOldRecentFiles();

            XElement recentFilesNode = new XElement("recentFiles");

            foreach (String path in this.Options.recentFiles)
            {
                recentFilesNode.Add(new XElement("item", path));
            }
            root.Add(recentFilesNode);

            //////////////

            XElement nodesNode = new XElement("nodes");

            this.saveNodes(nodesNode, this.nodes);
            root.Add(nodesNode);

            //////////////

            XElement tabsNode = new XElement("tabs");

            foreach (TabData data in this.tabsData)
            {
                XElement tabNode = new XElement("tab");

                tabNode.Add(new XElement("id", data.id));

                if (data.nodeid != "") {
                    tabNode.Add(new XElement("nodeid", data.nodeid));
                }

                tabNode.Add(new XElement("name", data.name));
                tabNode.Add(new XElement("path", data.path));
                tabNode.Add(new XElement("text", data.textBox.Text.Replace("\r\n", "\n")));
                tabNode.Add(new XElement("selected", data.selected ? "1" : "0"));
                tabNode.Add(new XElement("saved", data.saved ? "1" : "0"));

                tabsNode.Add(tabNode);
            }

            root.Add(tabsNode);

            return root;
        }

        public void saveNodes(XElement parent, TreeNodeCollection nodes) {
            foreach (TreeNode node in nodes)
            {
                XElement nodeNode = new XElement("node");

                TreeData treeData = (TreeData)node.Tag;

                nodeNode.Add(new XElement("id", treeData.id));
                nodeNode.Add(new XElement("name", treeData.name));
                nodeNode.Add(new XElement("text", treeData.text.Replace("\r\n", "\n")));
                nodeNode.Add(new XElement("expanded", node.IsExpanded ? "1" : "0"));
                nodeNode.Add(new XElement("isroot", treeData.isroot ? "1" : "0"));
                nodeNode.Add(new XElement("folder", treeData.folder ? "1" : "0"));
                nodeNode.Add(new XElement("note", treeData.note ? "1" : "0"));

                if (nodeNode.HasElements)
                {
                    XElement childsNode = new XElement("childs");
                    this.saveNodes(childsNode, node.Nodes);
                    nodeNode.Add(childsNode);
                }

                parent.Add(nodeNode);
            }
        }

        /*************************************************************************************************************************/

        public string  GetGlobalConfigDirectory()
        {
            return Os.Combine(
                Os.GetApplicationsDirectory(),
                this.configFileDirectory
            );
        }
    }
}
