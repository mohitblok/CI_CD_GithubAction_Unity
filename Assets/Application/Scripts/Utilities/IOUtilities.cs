using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Utilities
{
    public static class IO
    {
        public static void CleanFolder(string absolute)
        {
            DestroyFolder(absolute);
            ForceFolder(absolute);
        }

        public static bool ForceFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
            return FolderExists(folderPath);
        }
        
        public static List<string> GetAllFilesInDirectories(string target_dir)
        {
            var list = new List<string>();

            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                if (list.Contains(file))
                    continue;

                list.Add(file);
            }

            foreach (string dir in dirs)
            {
                list.AddRange(GetAllFilesInDirectories(dir));
            }

            return list;
        }

        public static void CleanFolderContents(string folderPath)
        {
            if (FolderExists(folderPath) == true)
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    DeleteFile(file);
                }
            }
        }
        
        public static void CleanFolderContentsFolders(string folderPath)
        {
            if (FolderExists(folderPath) == true)
            {
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    DeleteFile(file);
                }
            }
            
            foreach (string directory in Directory.GetDirectories(folderPath))
            {
                DestroyFolder(directory);
            }
        }

        public static void DestroyFolder(string folderPath)
        {
            folderPath = folderPath.DesanitiseSlashes();

            if (FolderExists(folderPath) == false)
            {
                return;
            }

            foreach (string directory in Directory.GetDirectories(folderPath))
            {
                DestroyFolder(directory);
            }

            CleanFolderContents(folderPath);

            try
            {
                DeleteFolder(folderPath);
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogException(ex);
                throw;
            }
        }

        private static void DeleteFolder(string path)
        {
            string meta = path.TrimEnd('\\', '/') + ".meta";
            if (FileExists(meta) == true)
            {
                DeleteFile(meta);
            }
            if (FolderExists(path) == true)
            {
                Directory.Delete(path, true);
            }
        }

        public static void CopyFile(string sourcePath, string destinationPath)
        {
            File.Copy(sourcePath, destinationPath, true);
        }

        public static void DeleteFile(string filePath) => File.Delete(filePath);

        public static bool DriveExists(string path) => FolderExists(Path.GetPathRoot(path));
        public static bool FolderExists(string folderPath) => Directory.Exists(folderPath);
        public static bool FileExists(string filePath) => File.Exists(filePath);

        public static string GetFullFileName(string filePath) => Path.GetFileName(filePath);
        public static string GetFileName(string filePath) => Path.GetFileNameWithoutExtension(filePath);

        public static string GetFolderPath(string filePath)
        {
            return filePath.Replace(GetFullFileName(filePath), "");
        }

        public static List<string> GetAllFiles(string folderPath, bool allFolders = true, string pattern = "*")
        {
            return GetFiles(folderPath, pattern, allFolders);
        }
        public static List<string> GetFiles(string folderPath, string pattern, bool allFolders = true)
        {
            return Directory.GetFiles(folderPath, pattern, GetOption(allFolders)).ToList();
        }

        public static List<string> GetAllFolders(string rootFolderPath, bool allFolders = true)
        {
            return Directory.GetDirectories(rootFolderPath, "*", GetOption(allFolders)).ToList();
        }

        private static SearchOption GetOption(bool allFolders)
        {
            return (allFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static bool IsFolderEmpty(string folderPath) => IsFolderEmpty(new DirectoryInfo(folderPath));
        public static bool IsFolderEmpty(DirectoryInfo directoryInfo)
        {
            try
            {
                var files = directoryInfo.GetFiles("*.*");
                files = files.Where(f => !IsMetaFile(f)).ToArray();
                return (files == null || files.Length == 0);
            }
            catch { }
            return false;
        }

        private static bool IsMetaFile(FileInfo file) => file.Extension == ".meta";

        public static string ReadFromFile(string filePath)
        {
            // check if folder exists, if not fail
            //if (FileExists(filePath) == false)
            //{
            //    UnityEngine.Debug.LogError($"Failed to load file at: \"{filePath}\". Path does not exist!\n");
            //    return null;
            //}

            // try to open file and read text
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log($"Failed to load contents of file: \"{filePath}\"\n{e.Message}");
            }
            return null;
        }

        public static void WriteToFile(string filePath, string text)
        {
            ForceFolder(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, text);
        }

        public static bool OpenFolder(string path)
        {
            if (FolderExists(path) == true)
            {
                if (path.StartsWith("/") == true)
                {
                    path = "file:" + path;
                }

                try
                {
                    Process.Start(path);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                    return false;
                }
            }
            return true;
        }
    }
}
