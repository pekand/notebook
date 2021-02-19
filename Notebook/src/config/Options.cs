using System;
using System.Collections.Generic;

namespace Notebook
{
    /// <summary>
    /// global program parmeters for all instances </summary>
    public class Options //UID0014460148
    {

        public int windowsX = 0;
        public int windowsY = 0;
        public int windowsWidth = 0;
        public int windowsHeight = 0;
        public int windowsState = 0;

        public int pinX = -1;
        public int pinY = -1;

        public long lastSave = 0;

        public int splitDistance = 200;

        public List<String> recentFiles = new List<String>();

        /*************************************************************************************************************************/
        // Recent files


        public void AddRecentFile(String path)
        {
            if (Os.FileExists(path))
            {
                this.recentFiles.Remove(path);
                this.recentFiles.Insert(0, path);
            }
        }

        public void RemoveOldRecentFiles()
        {
            List<String> newRecentFiles = new List<String>();

            foreach (String path in this.recentFiles)
            {
                if(Os.FileExists(path))
                {
                    newRecentFiles.Add(path);
                }
            }
            this.recentFiles = newRecentFiles;
        }
    }
}
