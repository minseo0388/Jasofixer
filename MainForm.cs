using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public class MainForm : Form
    {
        private Button btnSelectFolder;
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
                        BackupManager.CopyDirectory(selectedPath, backupPath);
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
                List<string> logs = Normalizer.NormalizeAll(selectedPath, progressBar, msg => logBox.AppendText(msg + "\r\n"));

                MessageBox.Show("정규화 완료!");

                if (chkSaveLog.Checked && logs.Count > 0)
                {
                    LogManager.SaveLog(logs);
                }
            }
        }
    }
}
