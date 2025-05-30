using System.IO;

namespace HangulJasofixer
{
    public static class BackupManager
    {
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            foreach (string dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dir.Replace(sourceDir, targetDir));

            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
                File.Copy(file, file.Replace(sourceDir, targetDir));
        }
    }
}
