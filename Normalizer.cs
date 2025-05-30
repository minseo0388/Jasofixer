using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public static class Normalizer
    {
        public static List<string> NormalizeAll(string rootDir, ProgressBar progressBar, Action<string> logCallback)
        {
            var logs = new List<string>();
            var allItems = Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories)).ToList();

            int total = allItems.Count;
            int current = 0;

            foreach (var path in allItems.OrderByDescending(p => p.Length))
            {
                string name = Path.GetFileName(path);
                string normalized = name.Normalize(NormalizationForm.FormC);
                string parent = Path.GetDirectoryName(path);

                if (name != normalized)
                {
                    string newPath = Path.Combine(parent, normalized);
                    try
                    {
                        if (File.Exists(path)) File.Move(path, newPath);
                        else if (Directory.Exists(path)) Directory.Move(path, newPath);

                        string log = $"[RENAMED] {path} → {normalized}";
                        logs.Add(log);
                        logCallback?.Invoke(log);
                    }
                    catch (Exception ex)
                    {
                        string err = $"[FAILED] {path} - {ex.Message}";
                        logs.Add(err);
                        logCallback?.Invoke(err);
                    }
                }

                current++;
                if (progressBar != null)
                {
                    progressBar.Value = Math.Min(100, (int)((double)current / total * 100));
                    progressBar.Update();
                }
            }

            return logs;
        }
    }
}
