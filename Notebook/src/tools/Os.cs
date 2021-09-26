using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace Notebook
{

    public class Os //UID8599434163
    {

        /*************************************************************************************************************************/
        // ASSEMBLY

        public static class Assembly 
        {
            /// <summary>
            /// get current app version</summary>
            public static string GetThisAssemblyVersion()
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fvi.FileVersion;
            }

            /// get current app executable path</summary>
            public static string GetThisAssemblyLocation()
            {
                return System.Reflection.Assembly.GetExecutingAssembly().Location;
            }

            /// <summary>
            /// get current running application executable directory
            /// Example: c:\Program Files\Infinite Diagram\
            /// </summary> 
            public static String GetCurrentApplicationDirectory()
            {
                return Os.Path.Directory.GetDirectoryName(Application.ExecutablePath);
            }

            /// <summary>
            /// get config file directory
            /// Example: C:\Users\user_name\AppData\Roaming\
            /// </summary> 
            public static string GetApplicationsDirectory()
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }

        /*************************************************************************************************************************/
        // PATH EXTENSION

        public static class Path
        {
            public static class File
            {
                /// <summary>
                /// check if file exist independent on os </summary>
                public static bool Exists(string path)
                {
                    return System.IO.File.Exists(NormalizePath(path));
                }

                /// <summary>
                /// check if path is file</summary>
                public static bool IsFile(string path)
                {
                    return Os.Path.File.Exists(path);
                }

                /// <summary>
                /// get file extension</summary>
                public static string GetExtension(string file)
                {
                    string ext = "";
                    if (file != "" && Os.Path.File.Exists(file))
                    {
                        ext = System.IO.Path.GetExtension(file).ToLower();
                    }

                    return ext;
                }

                public static long Size(string path)
                {
                    if (!Os.Path.File.Exists(path))
                    {
                        return 0;
                    }

                    return new System.IO.FileInfo(path).Length;
                }

                /*************************************************************************************************************************/
                // WRITE AND READ FILE OPERATIONS

                /// <summary>
                /// create empty file</summary>
                public static void CreateEmptyFile(string path)
                {
                    System.IO.File.Create(path).Dispose();
                }

                /// <summary>
                /// write string content to file</summary>
                public static void WriteAllText(string path, string data)
                {
                    File.WriteAllText(path, data);
                }

                /// <summary>
                /// write string content to file</summary>
                public static string ReadAllText(string path)
                {
                    return File.ReadAllText(path);
                }

                /// <summary>
                /// get file content as string</summary>
                public static string GetFileContent(string file)
                {
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(file, Encoding.UTF8))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                    catch (System.IO.IOException ex)
                    {

                    }

                    return null;
                }

                /// <summary>
                /// write string content to file</summary>
                public static void WriteAllBytes(string path, byte[] data)
                {
                    File.WriteAllBytes(path, data);
                }

                /// <summary>
                /// write string content to file</summary>
                public static byte[] ReadAllBytes(string path)
                {
                    return File.ReadAllBytes(path);
                }

                /// <summary>
                /// get file name or directory name from path</summary>
                public static string GetFileName(string path)
                {
                    return System.IO.Path.GetFileName(path);
                }

                /// <summary>
                /// get file name or directory name from path</summary>
                public static string GetFileNameWithoutExtension(string path)
                {
                    return Os.Path.File.GetFileNameWithoutExtension(path);
                }

                /// <summary>
                /// get parent directory of FileName path </summary>
                public static string GetFileDirectory(string FileName)
                {
                    if (FileName.Trim().Length > 0 && Os.Path.File.Exists(FileName))
                    {
                        return new FileInfo(FileName).Directory.FullName;
                    }
                    return null;
                }

                /*************************************************************************************************************************/
                // SEARCH IN FILE

                /// <summary>
                /// find line number with first search string occurrence </summary>
                public static int FndLineNumber(string fileName, string search)
                {
                    int pos = 0;
                    string line;
                    using (StreamReader file = new StreamReader(fileName))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            pos++;
                            if (line.Contains(search))
                            {
                                return pos;
                            }
                        }
                    }

                    return pos;
                }
            }


            public static class Directory
            {
                /*************************************************************************************************************************/
                // DIRECTORY OPERATIONS

                /// <summary>
                /// check if path is directory</summary>
                public static bool IsDirectory(string path)
                {
                    return Os.Path.Directory.Exists(path);
                }

                /// <summary>
                /// check if directory exist independent on os </summary>
                public static bool Exists(string path)
                {
                    return System.IO.Directory.Exists(NormalizePath(path));
                }

                /// <summary>
                /// get file name or directory name from path</summary>
                public static string GetDirectoryName(string path)
                {
                    return System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
                }

                /// <summary>
                /// set current directory</summary>
                public static void SetCurrentDirectory(string path)
                {
                    System.IO.Directory.SetCurrentDirectory(path);
                }

                /// <summary>
                /// create directory</summary>
                public static bool Create(string path)
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(path);

                        return true;
                    }
                    catch (Exception e)
                    {

                    }
                    return false;
                }

                /// <summary>
                ///remove directory and content
                /// Example: C:\Users\user_name\AppData\Roaming\
                /// </summary> 
                public static void RemoveDirectory(string path)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.Delete(path, true);
                    }
                }

                public static long DirectorySize(string path, int level = 100)
                {
                    if (level == 0)
                    {
                        return 0;
                    }

                    if (!Os.Path.Directory.Exists(path))
                    {
                        return 0;
                    }

                    DirectoryInfo d = new DirectoryInfo(path);

                    long size = 0;
                    // Add file sizes.
                    FileInfo[] fis = d.GetFiles();
                    foreach (FileInfo fi in fis)
                    {
                        size += fi.Length;
                    }
                    // Add subdirectory sizes.
                    DirectoryInfo[] dis = d.GetDirectories();
                    foreach (DirectoryInfo di in dis)
                    {
                        size += DirectorySize(di.FullName, level--);
                    }
                    return size;
                }


                /*************************************************************************************************************************/
                // TEMP

                /// <summary>
                /// get temporary directory</summary>
                public static string GetTempPath()
                {
                    return System.IO.Path.GetTempPath();
                }

                /*************************************************************************************************************************/
                // SEARCH IN DIRECTORY

                /// <summary>
                /// Scans a folder and all of its subfolders recursively, and updates the List of files
                /// </summary>
                /// <param name="path">Full path for scened directory</param>
                /// <param name="files">out - file list</param>
                /// <param name="directories">out - directories list</param>
                public static void Search(string path, List<string> files, List<string> directories)
                {
                    try
                    {
                        foreach (string f in System.IO.Directory.GetFiles(path))
                        {
                            files.Add(f);
                        }

                        foreach (string d in System.IO.Directory.GetDirectories(path))
                        {
                            directories.Add(d);
                            Search(d, files, directories);
                        }

                    }
                    catch (System.Exception ex)
                    {

                    }
                }
            }

            /*************************************************************************************************************************/
            // FILE OPERATIONS

            /// <summary>
            /// check if directory or file exist independent on os </summary>
            public static bool Exists(string path)
            {
                return Os.Path.File.Exists(path) || Os.Path.Directory.Exists(path);
            }         

            /*************************************************************************************************************************/
            // PATH OPERATIONS

            /// <summary>
            /// get full path</summary>
            public static string GetFullPath(string path)
            {
                return Path.GetFullPath(path);
            }

            /// <summary>
            /// concat path and subdir</summary>
            public static string Combine(string path, string subdir)
            {
                return System.IO.Path.Combine(path, subdir);
            }

            /// <summary>
            /// convert slash dependent on current os </summary>
            public static string NormalizePath(string path)
            {
                return path.Replace("/", "\\");
            }

            /// <summary>
            /// normalize path and get full path from relative path </summary>
            public static string NormalizedFullPath(string path)
            {
                return Path.GetFullPath(NormalizePath(path));
            }

            /// <summary>
            /// meke filePath relative to currentPath. 
            /// If is set inCurrentDir path is converted to relative only 
            /// if currentPath is parent of filePath</summary>
            public static string MakeRelative(string filePath, string currentPath, bool inCurrentDir = true)
            {
                filePath = filePath.Trim();
                currentPath = currentPath.Trim();

                if (currentPath == "")
                {
                    return filePath;
                }

                if (!Os.Path.File.Exists(filePath) && !Os.Path.Directory.Exists(filePath))
                {
                    return filePath;
                }

                filePath = Os.Path.GetFullPath(filePath);

                if (Os.Path.File.Exists(currentPath))
                {
                    currentPath = Os.Path.Directory.GetDirectoryName(currentPath);
                }

                if (!Os.Path.Directory.Exists(currentPath))
                {
                    return filePath;
                }

                currentPath = Os.Path.GetFullPath(currentPath);

                Uri pathUri = new Uri(filePath);
                // Folders must end in a slash
                if (!currentPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    currentPath += System.IO.Path.DirectorySeparatorChar;
                }

                int pos = filePath.ToLower().IndexOf(currentPath.ToLower());
                if (inCurrentDir && pos != 0) // skip files outside of currentPath
                {
                    return filePath;
                }

                Uri folderUri = new Uri(currentPath);
                return Uri.UnescapeDataString(
                    folderUri.MakeRelativeUri(pathUri)
                    .ToString()
                    .Replace('/', System.IO.Path.DirectorySeparatorChar)
                );
            }

            public delegate void CopyProgressDelegate(long count);

            public static void CopyByBlock(string inputPath, string outputPath, CopyProgressDelegate callback = null)
            {
                using (FileStream input = System.IO.File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (FileStream output = System.IO.File.Open(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        int bytesRead;
                        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            callback?.Invoke(bytesRead);
                        }
                    }
                }
            }

            /// <summary>
            /// copy file or directory </summary>
            public static bool Copy(string SourcePath, string DestinationPath, CopyProgressDelegate callback = null)
            {
                try
                {
                    if (Directory.Exists(SourcePath))
                    {
                        foreach (string dirPath in System.IO.Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories))
                            Os.Path.Directory.Create(dirPath.Replace(SourcePath, DestinationPath));

                        foreach (string newPath in System.IO.Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
                            CopyByBlock(newPath, newPath.Replace(SourcePath, DestinationPath), callback);
                    }
                    else if (File.Exists(SourcePath))
                    {
                        CopyByBlock(SourcePath, Os.Path.Combine(DestinationPath, Os.Path.File.GetFileName(SourcePath)), callback);
                    }
                    return true;
                }
                catch (Exception ex)
                {

                    return false;
                }
            }

            /// <summary>
            /// get path separator dependent on os </summary>
            public static string GetSeparator()
            {
                return System.IO.Path.DirectorySeparatorChar.ToString();
            }

        }


        /*************************************************************************************************************************/
        // OPEN

        public static class Process
        {
            /// <summary>
            /// run application in current os </summary>
            public static void RunProcess(string path)
            {
                path = Os.Path.NormalizePath(path);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }

            /// <summary>
            /// open path in explorer in current os </summary>
            public static void OpenDirectory(string path)
            {
                path = Os.Path.NormalizePath(path);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }

            /// <summary>
            /// open path in system if exist  </summary>
            public static void OpenPathInSystem(string path)
            {
                if (Os.Path.File.Exists(path))       // OPEN FILE
                {
                    try
                    {
                        string parent_diectory = Os.Path.File.GetFileDirectory(path);
                        System.Diagnostics.Process.Start(parent_diectory);
                    }
                    catch (Exception ex) { }
                }
                else if (Os.Path.Directory.Exists(path))  // OPEN DIRECTORY
                {
                    try
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    catch (Exception ex) { }
                }
            }

            /// <summary>
            /// open directory in system</summary>
            public static void ShowDirectoryInExternalApplication(string path)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", path);
                }
                catch (Exception ex)
                {

                }
            }

            /// <summary>
            /// run command in system and discard output </summary>
            public static void RunCommandAndExit(string cmd, string parameters = "")
            {

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + "\"" + cmd + ((parameters != "") ? " " + parameters : "") + "\"";

                process.StartInfo = startInfo;
                process.Start();
            }
        }


        public static class Tools
        {
            /*************************************************************************************************************************/
            // TOOLS

            /// <summary>
            /// add slash before slash and quote </summary>
            public static string Escape(string text)
            {
                return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
            }

            /// <summary>
            /// convert win path slash to linux type slash </summary>
            public static string ToBackslash(string text)
            {
                return text.Replace("\\", "/");
            }
        }

        /*************************************************************************************************************************/
        // CLIPBOARD

        public static class Clipboard
        {
            /// <summary>
            /// get string from clipboard </summary>
            public static string GetTextFormClipboard()
            {
                DataObject retrievedData = (DataObject)System.Windows.Forms.Clipboard.GetDataObject();
                string clipboard = "";
                if (retrievedData != null && retrievedData.GetDataPresent(DataFormats.Text))  // [PASTE] [TEXT] insert text
                {
                    clipboard = retrievedData.GetData(DataFormats.Text) as string;
                }

                return clipboard;
            }
        }

    }
}
