using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Notebook
{
    public class GlobalOptions
    {
        public String configFileDirectory = "Notebook";

#if DEBUG
        // global configuration file name in debug mode
        public String defaultFileName = "notebook.debug.config";
#else
        public String defaultFileName = "notebook.config";
#endif

        public bool enablePin = true;
        public bool mostTop = false;

        public GlobalOptions()
        {
        }

        public string getDefaultFilePath()
        {
            // use local config file
            string localOptionFilePath = Os.Combine(
                Os.GetCurrentApplicationDirectory(),
                this.defaultFileName
            );

            if (Os.FileExists(localOptionFilePath))
            {
                return localOptionFilePath;
            }
            else
            {

                string globalConfigDirectory = Os.Combine(
                    Os.GetApplicationsDirectory(),
                    this.configFileDirectory
                );

                // create global config directory if not exist
                if (!Os.DirectoryExists(globalConfigDirectory))
                {
                    Os.CreateDirectory(globalConfigDirectory);
                }

                return Os.Combine(
                    globalConfigDirectory,
                    this.defaultFileName
                );
            }
        }

        public bool defaultConfigFileExists()
        {

            string defaultFilePath = this.getDefaultFilePath();

            if (Os.FileExists(defaultFilePath))
            {

                return true;
            }

            return false;
        }

        public void LoadConfigFile()
        {

            string configPath = this.getDefaultFilePath();

            if (Os.FileExists(configPath))
            {
                this.LoadXmlConfigFile(configPath);
            }
        }

        private void LoadXmlConfigFile(string path)
        {
            try
            {
                if (Os.FileExists(path))
                {

                    string xml = Os.GetFileContent(path);

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
                if (option.Name.ToString() == "enablePin")
                {
                    this.enablePin = option.Value == "1";
                }

                if (option.Name.ToString() == "mostTop")
                {
                    this.mostTop = option.Value == "1";
                }

            }
        }

        public void SaveConfigFile()
        {
            string configPath = this.getDefaultFilePath();

            try
            {
                XElement root = this.SaveParams();

                System.IO.StreamWriter file = new System.IO.StreamWriter(configPath);

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

            root.Add(new XElement("enablePin", this.enablePin ? "1" : "0"));
            root.Add(new XElement("mostTop", this.mostTop ? "1" : "0"));

            return root;
        }

    }
}
