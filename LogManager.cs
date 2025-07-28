using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public static class LogManager
    {
        public static bool SaveLog(List<string>? logs, string? customPath = null)
        {
            if (logs == null || logs.Count == 0)
            {
                MessageBox.Show("저장할 로그가 없습니다.", "로그 저장", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                string filename = customPath ?? $"변경_로그_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string fullPath = Path.GetFullPath(filename);
                
                // Ensure directory exists
                string? directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllLines(fullPath, logs, Encoding.UTF8);
                
                MessageBox.Show($"{Path.GetFileName(fullPath)}으로 로그가 저장되었습니다.\n\n경로: {fullPath}", 
                               "로그 저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"로그 저장 중 오류가 발생했습니다:\n{ex.Message}", 
                               "로그 저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
