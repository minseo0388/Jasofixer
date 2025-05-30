using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public class MainForm : Form
    {
        private Button btnSelectFolder;
        private Button btnNormalize;
        private CheckBox chkBackup;
        private CheckBox chkSaveLog;
        private ProgressBar progressBar;
        private TextBox logBox;

        public MainForm()
        {
            this.Text = "한글 자소 정규화 도구";
            this.Width = 700;
            this.Height = 500;

            btnSelectFolder = new Button() { Text = "폴더 선택 및 정규화 시작", Left = 20, Top = 20, Width = 250, Height = 40 };
            btnSelectFolder.Click += BtnSelectFolder_Click;

            chkBackup = new CheckBox() { Text = "정규화 전에 백업", Left = 290, Top = 30, Checked = true };
            chkSaveLog = new CheckBox() { Text = "변경 로그 파일 저장", Left = 430, Top = 30, Checked = true };

            progressBar = new ProgressBar() { Left = 20, Top = 70, Width = 640, Height = 20 };
            logBox = new TextBox() { Left = 20, Top = 100, Width = 640, Height = 320, Multiline = true, ScrollBars = ScrollBars.Vertical };

            this.Controls.Add(btnSelectFolder);
            this.Controls.Add(chkBackup);
            this.Controls.Add(chkSaveLog);
            this.Controls.Add(progressBar);
            this.Controls.Add(logBox);
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderDialog.SelectedPath;

                if (chkBackup.Checked)
                {
                    string backupPath = selectedPath + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    try
                    {
                        CopyDirectory(selectedPath, backupPath);
                        MessageBox.Show("백업 완료: " + backupPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("백업 실패: " + ex.Message);
                        return;
                    }
                }

                logBox.Clear();
                progressBar.Value = 0;
                List<string> logs = NormalizeAll(selectedPath, progressBar, msg => logBox.AppendText(msg + "\r\n"));

                MessageBox.Show("정규화 완료!");

                if (chkSaveLog.Checked && logs.Any())
                {
                    SaveLog(logs);
                }
            }
        }

        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            foreach (string dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(sourceDir, targetDir));
            }

            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, file.Replace(sourceDir, targetDir));
            }
        }

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
                        if (File.Exists(path))
                            File.Move(path, newPath);
                        else if (Directory.Exists(path))
                            Directory.Move(path, newPath);

                        logs.Add($"[RENAMED] {path} → {normalized}");
                        logCallback?.Invoke($"[RENAMED] {path} → {normalized}");
                    }
                    catch (Exception ex)
                    {
                        logs.Add($"[FAILED] {path} - {ex.Message}");
                        logCallback?.Invoke($"[FAILED] {path} - {ex.Message}");
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

        public static void SaveLog(List<string> logs)
        {
            string filename = $"변경_로그_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            File.WriteAllLines(filename, logs, Encoding.UTF8);
            MessageBox.Show($"로그 저장됨: {filename}");
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
