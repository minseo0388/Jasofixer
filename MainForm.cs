using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public class MainForm : Form
    {
        private Button btnSelectFolder = null!;
        private Button btnCancel = null!;
        private CheckBox chkBackup = null!;
        private CheckBox chkSaveLog = null!;
        private ProgressBar progressBar = null!;
        private TextBox logBox = null!;
        private Label lblStatus = null!;
        private bool isProcessing = false;
        private CancellationTokenSource? cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Text = "한글 자소 정규화 도구 v2.0";
            this.Width = 720;
            this.Height = 550;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(720, 550);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Main button
            btnSelectFolder = new Button() 
            { 
                Text = "폴더 선택 및 정규화 시작", 
                Left = 20, 
                Top = 20, 
                Width = 220, 
                Height = 45,
                Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold),
                UseVisualStyleBackColor = true
            };

            // Cancel button
            btnCancel = new Button() 
            { 
                Text = "취소", 
                Left = 250, 
                Top = 20, 
                Width = 60, 
                Height = 45,
                Font = new System.Drawing.Font("맑은 고딕", 9F),
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            // Checkboxes
            chkBackup = new CheckBox() 
            { 
                Text = "정규화 전에 백업 생성", 
                Left = 330, 
                Top = 25, 
                Width = 180, 
                Height = 25,
                Checked = true,
                Font = new System.Drawing.Font("맑은 고딕", 9F)
            };
            
            chkSaveLog = new CheckBox() 
            { 
                Text = "변경 로그 파일 저장", 
                Left = 330, 
                Top = 50, 
                Width = 180, 
                Height = 25,
                Checked = true,
                Font = new System.Drawing.Font("맑은 고딕", 9F)
            };

            // Progress bar
            progressBar = new ProgressBar() 
            { 
                Left = 20, 
                Top = 80, 
                Width = 660, 
                Height = 30,
                Style = ProgressBarStyle.Continuous
            };

            // Status label
            lblStatus = new Label()
            {
                Text = "준비됨 - 폴더를 선택해주세요",
                Left = 20,
                Top = 120,
                Width = 660,
                Height = 25,
                Font = new System.Drawing.Font("맑은 고딕", 9F),
                ForeColor = System.Drawing.Color.DarkBlue
            };
            
            // Log textbox
            logBox = new TextBox() 
            { 
                Left = 20, 
                Top = 150, 
                Width = 660, 
                Height = 320, 
                Multiline = true, 
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 8.5F),
                BackColor = System.Drawing.Color.FromArgb(248, 248, 248),
                BorderStyle = BorderStyle.Fixed3D
            };

            // Add controls
            this.Controls.AddRange(new Control[] {
                btnSelectFolder, btnCancel, chkBackup, chkSaveLog, 
                progressBar, lblStatus, logBox
            });
        }

        private void SetupEventHandlers()
        {
            btnSelectFolder.Click += BtnSelectFolder_Click;
            btnCancel.Click += BtnCancel_Click;
            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (isProcessing)
            {
                var result = MessageBox.Show(
                    "작업이 진행 중입니다. 정말로 종료하시겠습니까?",
                    "확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (isProcessing && cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                LogMessage("[INFO] 사용자가 작업 취소를 요청했습니다...");
                UpdateStatus("취소 중...");
            }
        }

        private async void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            if (isProcessing)
            {
                MessageBox.Show("작업이 진행 중입니다. 잠시만 기다려주세요.", "알림", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "정규화할 폴더를 선택하세요";
                folderDialog.ShowNewFolderButton = false;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    await ProcessFolderAsync(folderDialog.SelectedPath);
                }
            }
        }

        private async Task ProcessFolderAsync(string selectedPath)
        {
            SetProcessingState(true);
            cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                // Validate the selected path
                if (string.IsNullOrWhiteSpace(selectedPath))
                {
                    throw new ArgumentException("Selected path cannot be null or empty.");
                }

                if (!Directory.Exists(selectedPath))
                {
                    throw new DirectoryNotFoundException($"Selected directory does not exist: {selectedPath}");
                }

                await Task.Run(() => ProcessFolder(selectedPath, cancellationTokenSource.Token), cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                LogMessage("[INFO] 작업이 사용자에 의해 취소되었습니다.");
                UpdateStatus("작업 취소됨");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"처리 중 오류가 발생했습니다:\n{ex.Message}", 
                               "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"[ERROR] 전체 처리 실패: {ex.Message}");
            }
            finally
            {
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                SetProcessingState(false);
            }
        }

        private void ProcessFolder(string selectedPath, CancellationToken cancellationToken = default)
        {
            List<string> logs = new List<string>();
            
            LogMessage($"선택된 폴더: {selectedPath}");
            LogMessage(new string('=', 80));
            
            try
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Backup if requested
                if (chkBackup.Checked)
                {
                    UpdateStatus("백업 생성 중...");
                    
                    string backupPath = selectedPath + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    LogMessage("백업 시작...");
                    
                    // Check for cancellation before backup
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    BackupManager.CopyDirectory(selectedPath, backupPath);
                    LogMessage($"백업 완료: {backupPath}");
                    
                    this.Invoke(new Action(() => 
                    {
                        MessageBox.Show($"백업이 완료되었습니다:\n{backupPath}", 
                                       "백업 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }

                // Check for cancellation before normalization
                cancellationToken.ThrowIfCancellationRequested();

                // Normalize files
                UpdateStatus("파일명 정규화 중...");
                LogMessage("정규화 시작...");
                SetProgress(0);
                
                logs.AddRange(Normalizer.NormalizeAll(selectedPath, progressBar, LogMessage));

                LogMessage(new string('=', 80));
                LogMessage($"정규화 완료! 총 {logs.Count}개의 변경사항이 처리되었습니다.");
                
                UpdateStatus($"완료 - {logs.Count}개 항목 처리됨");

                this.Invoke(new Action(() => 
                {
                    MessageBox.Show($"정규화가 완료되었습니다!\n\n처리된 항목: {logs.Count}개", 
                                   "작업 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));

                // Save log if requested
                if (chkSaveLog.Checked && logs.Count > 0)
                {
                    LogManager.SaveLog(logs);
                }
            }
            catch (OperationCanceledException)
            {
                LogMessage("[INFO] 작업이 취소되었습니다.");
                UpdateStatus("작업 취소됨");
                throw; // Re-throw to be handled by caller
            }
            catch (Exception ex)
            {
                LogMessage($"[ERROR] 처리 실패: {ex.Message}");
                UpdateStatus("오류 발생");
                throw;
            }
            finally
            {
                SetProgress(0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void SetProcessingState(bool processing)
        {
            isProcessing = processing;
            
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetProcessingState(processing)));
                return;
            }

            btnSelectFolder.Enabled = !processing;
            btnCancel.Enabled = processing;
            chkBackup.Enabled = !processing;
            chkSaveLog.Enabled = !processing;
            
            btnSelectFolder.Text = processing ? "처리 중..." : "폴더 선택 및 정규화 시작";
            
            if (!processing)
            {
                UpdateStatus("준비됨 - 폴더를 선택해주세요");
            }
        }

        private void LogMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => LogMessage(message)));
                return;
            }

            logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            logBox.SelectionStart = logBox.Text.Length;
            logBox.ScrollToCaret();
        }

        private void UpdateStatus(string status)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateStatus(status)));
                return;
            }

            lblStatus.Text = status;
        }

        private void SetProgress(int value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetProgress(value)));
                return;
            }

            progressBar.Value = Math.Max(0, Math.Min(100, value));
        }
    }
}
