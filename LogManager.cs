using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public static class LogManager
    {
        public static void SaveLog(List<string> logs)
        {
            string filename = $"변경_로그_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            File.WriteAllLines(filename, logs, Encoding.UTF8);
            MessageBox.Show($"{filename}으로 로그 저장되었습니다.", "로그 저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
