using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace JasoFixer
{
    public class Change : Form
    {
        private List list;
        private IContainer components;
        private Label labelCurrentFile;
        private ProgressBar progressBarSearching;
        private Button buttonCancel;
        private Background backgroundRename;
        private StringBuilder renameLog; // 로깅을 위한 StringBuilder

        public FormChange() 
        {
            this.InitializeComponent();
            renameLog = new StringBuilder(); // 로그 초기화
        }

        public void SetCriteria(List list ) => this.list  = list;

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.StopBackground();
            this.buttonCancel.Enabled = false;
        }

        private void StopBackground()
        {
            if (!this.backgroundRename.IsBusy)
                return;
            this.backgroundRename.CancelAsync();
        }

        private void FormChange(object sender, EventArgs e)
        {
            this.Enabled = false; // 작업 중에 UI 비활성화
            this.backgroundRename.RunWorkerAsync(new WorkerArgument(this.listview, this.labelCurrentFile));
        }

        private void BackgroundWorkerRename_DoWork(object sender, DoWorkEventArgs e)
        {
            Background worker = sender as Background;
            WorkerArgument workerArgument = e.Argument as WorkerArgument;
            Label fileDisplayLabel = workerArgument.CurrentFileDisplayLabel;
            List list = workerArgument.List;
            int count = list.CheckedItems.Count;

            this.ChangeFiles(worker, list.Items, count, fileDisplayLabel);
        }

        private void ChangeFiles(
            Background worker,
            List.ListItemCollection items,
            int totalFileCount,
            Label currentFileDisplayLabel)
        {
            int processedCount = 0;
            foreach (ListItem listItem in items)
            {
                if (worker.CancellationPending)
                    break;

                if (listItem.Checked)
                {
                    processedCount++;
                    worker.ReportProgress(processedCount * 100 / totalFileCount);

                    string beforeFileName = listItem.SubItems[0].Text;
                    string afterFileName = listItem.SubItems[1].Text;

                    // UI 스레드에서 안전하게 라벨 업데이트
                    this.Invoke((MethodInvoker)(() => currentFileDisplayLabel.Text = beforeFileName));

                    try
                    {
                        this.Rename(beforeFileName, afterFileName);
                        // 성공 시 아이템을 리스트에서 제거
                        this.Invoke((MethodInvoker)(() => items.Remove(listItem)));
                        renameLog.AppendLine($"수행 성공한 작업 기록: {beforeFileName} -> {afterFileName}");
                    }
                    catch (Exception ex)
                    {
                        // 실패 메시지 출력
                        this.Invoke((MethodInvoker)(() => listItem.SubItems[2].Text = ex.Message));
                        renameLog.AppendLine($"수행 실패한 작업 기록: {beforeFileName} -> {afterFileName}, Reason: {ex.Message}");
                    }
                }
            }
        }

        private void Rename(string before, string after, bool isOverwrite = false)
        {
            if (isOverwrite)
            {
                FileInfo fileInfo = new FileInfo(after);
                if (fileInfo.Exists)
                    fileInfo.Delete();
            }

            FileInfo newfileInfo = new FileInfo(before);
            try
            {
                newfileInfo.MoveTo(after);
            }
            catch (IOException ex)
            {
                if (new FileInfo(after).Exists)
                {
                    if (MessageBox.Show("바뀔 파일과 동일한 이름의 파일이 존재합니다. 대체하겠습니까?", "경고", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        throw new IOException("바뀔 파일명과 동일한 파일이 존재합니다.");
                    this.Rename(before, after, true);
                }
                throw;
            }
        }

        private void BackgroundRenameChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBarSearching.Value = e.ProgressPercentage;
        }

        private void BackgroundRenameRunCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Enabled = true; // 작업 완료 시 UI 다시 활성화

            if (e.Cancelled)
            {
                MessageBox.Show("요청에 의해 작업이 취소되었습니다.", "취소됨", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.Error != null)
            {
                MessageBox.Show($"오류가 발생하였습니다. 다음을 참조하세요. : {e.Error.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("성공적으로 수행되었습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 로그 파일 저장
            File.WriteAllText("operationlog.txt", renameLog.ToString());

            this.Close();
        }

        private void FormChangingClosing(object sender, FormClosingEventArgs e)
        {
            this.StopBackground();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.labelCurrentFile = new Label();
            this.progressBarSearching = new ProgressBar();
            this.buttonCancel = new Button();
            this.backgroundRename = new Background();
            this.SuspendLayout();

            this.labelCurrentFile.AutoSize = true;
            this.labelCurrentFile.Location = new Point(72, 19);
            this.labelCurrentFile.Name = "labelCurrentFile";
            this.labelCurrentFile.Size = new Size(153, 12);
            this.labelCurrentFile.TabIndex = 5;
            this.labelCurrentFile.Text = "{current file display zone}";

            this.progressBarSearching.Location = new Point(74, 52);
            this.progressBarSearching.Name = "progressBarSearching";
            this.progressBarSearching.Size = new Size(369, 18);
            this.progressBarSearching.Style = ProgressBarStyle.Continuous;
            this.progressBarSearching.TabIndex = 4;

            this.buttonCancel.Location = new Point(396, 76);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "취소(&C)";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new EventHandler(this.ButtonCancel_Click);

            this.backgroundRename.WorkerReportsProgress = true;
            this.backgroundRename.WorkerSupportsCancellation = true;
            this.backgroundRename.DoWork += new DoWorkEventHandler(this.BackgroundWorkerRename_DoWork);
            this.backgroundRename.ProgressChanged += new ProgressChangedEventHandler(this.BackgroundWorkerRename_ProgressChanged);
            this.backgroundRename.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorkerRename_RunWorkerCompleted);

            this.AcceptButton = this.buttonCancel;
            this.AutoScaleDimensions = new SizeF(7f, 12f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(484, 111);
            this.Controls.Add(this.labelCurrentFile);
            this.Controls.Add(this.progressBarSearching);
            this.Controls.Add(this.buttonCancel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormChanging";
            this.ShowIcon = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "변경중...";
            this.FormClosing += new FormClosingEventHandler(this.FormChanging_FormClosing);
            this.Shown += new EventHandler(this.FormChanging_Shown);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private class WorkerArgument
        {
            public WorkerArgument(ListView listview, Label currentFileDisplayLabel)
            {
                this.Listview = listview;
                this.CurrentFileDisplayLabel = currentFileDisplayLabel;
            }

            public ListView Listview { get; }
            public Label CurrentFileDisplayLabel { get; }
        }
    }
}
