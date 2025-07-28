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
        public static List<string> NormalizeAll(string rootDir, ProgressBar? progressBar, Action<string>? logCallback)
        {
            if (string.IsNullOrWhiteSpace(rootDir))
                throw new ArgumentException("Root directory cannot be null or empty.", nameof(rootDir));

            if (!Directory.Exists(rootDir))
                throw new DirectoryNotFoundException($"Root directory not found: {rootDir}");

            var logs = new List<string>();
            
            try
            {
                // Get all items (files and directories) with better error handling
                var allItems = GetAllItems(rootDir).ToList();
                int total = allItems.Count;
                int current = 0;

                // Process items from deepest to shallowest to avoid path conflicts
                foreach (var path in allItems.OrderByDescending(p => p.Length))
                {
                    try
                    {
                        ProcessItem(path, logs, logCallback);
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"[ERROR] Failed to process '{path}': {ex.Message}";
                        logs.Add(errorMsg);
                        logCallback?.Invoke(errorMsg);
                    }

                    current++;
                    UpdateProgress(progressBar, current, total);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"[CRITICAL ERROR] Failed to process directory '{rootDir}': {ex.Message}";
                logs.Add(errorMsg);
                logCallback?.Invoke(errorMsg);
                throw;
            }

            return logs;
        }

        private static IEnumerable<string> GetAllItems(string rootDir)
        {
            var items = new List<string>();
            
            try
            {
                items.AddRange(Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories));
                items.AddRange(Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories));
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Access denied to directory: {rootDir}", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new DirectoryNotFoundException($"Directory not found: {rootDir}", ex);
            }

            return items;
        }

        private static void ProcessItem(string path, List<string> logs, Action<string>? logCallback)
        {
            string name = Path.GetFileName(path);
            string normalized = name.Normalize(NormalizationForm.FormC);
            
            if (name == normalized)
                return; // No normalization needed

            string? parent = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(parent))
            {
                string warning = $"[SKIPPED] {path} - Unable to determine parent directory";
                logs.Add(warning);
                logCallback?.Invoke(warning);
                return;
            }

            string newPath = Path.Combine(parent, normalized);
            
            // Check if target already exists
            if (File.Exists(newPath) || Directory.Exists(newPath))
            {
                string warning = $"[SKIPPED] {path} → {normalized} (target already exists)";
                logs.Add(warning);
                logCallback?.Invoke(warning);
                return;
            }

            try
            {
                if (File.Exists(path))
                    File.Move(path, newPath);
                else if (Directory.Exists(path))
                    Directory.Move(path, newPath);

                string log = $"[RENAMED] {path} → {normalized}";
                logs.Add(log);
                logCallback?.Invoke(log);
            }
            catch (IOException ex)
            {
                string err = $"[FAILED] {path} - IO Error: {ex.Message}";
                logs.Add(err);
                logCallback?.Invoke(err);
            }
            catch (UnauthorizedAccessException ex)
            {
                string err = $"[FAILED] {path} - Access denied: {ex.Message}";
                logs.Add(err);
                logCallback?.Invoke(err);
            }
            catch (Exception ex)
            {
                string err = $"[FAILED] {path} - {ex.Message}";
                logs.Add(err);
                logCallback?.Invoke(err);
            }
        }

        private static void UpdateProgress(ProgressBar? progressBar, int current, int total)
        {
            if (progressBar == null || total == 0)
                return;

            int percentage = Math.Min(100, (int)((double)current / total * 100));
            
            if (progressBar.InvokeRequired)
            {
                progressBar.BeginInvoke(new Action(() => 
                {
                    progressBar.Value = percentage;
                }));
            }
            else
            {
                progressBar.Value = percentage;
            }
        }
    }
}
