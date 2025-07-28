using System;
using System.IO;

namespace HangulJasofixer
{
    public static class BackupManager
    {
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (string.IsNullOrWhiteSpace(sourceDir))
                throw new ArgumentException("Source directory cannot be null or empty.", nameof(sourceDir));
            
            if (string.IsNullOrWhiteSpace(targetDir))
                throw new ArgumentException("Target directory cannot be null or empty.", nameof(targetDir));

            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            try
            {
                CopyDirectoryRecursive(sourceDir, targetDir);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to copy directory from '{sourceDir}' to '{targetDir}': {ex.Message}", ex);
            }
        }

        private static void CopyDirectoryRecursive(string sourceDir, string targetDir)
        {
            var sourceDirInfo = new DirectoryInfo(sourceDir);
            
            // Create target directory if it doesn't exist
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // Copy all files in the current directory
            foreach (FileInfo file in sourceDirInfo.GetFiles())
            {
                string targetFile = Path.Combine(targetDir, file.Name);
                try
                {
                    file.CopyTo(targetFile, true);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to copy file '{file.FullName}' to '{targetFile}': {ex.Message}", ex);
                }
            }

            // Recursively copy subdirectories
            foreach (DirectoryInfo subDir in sourceDirInfo.GetDirectories())
            {
                string targetSubDir = Path.Combine(targetDir, subDir.Name);
                CopyDirectoryRecursive(subDir.FullName, targetSubDir);
            }
        }

        private static bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
    }
}
