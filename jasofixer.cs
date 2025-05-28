using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HangulJasofixer
{
    public class MainForm : Form
    {
        private Button btnSelectFiles;
        private Button btnNormalize;
        private CheckedListBox fileList;
        private CheckBox chkSelectAll;

        public MainForm()
        {
            this.Text = "한글 파일 자소 결합기";
            this.Width = 600;
            this.Height = 400;

            btnSelectFiles = new Button() { Text = "파일 선택", Left = 20, Top = 20, Width = 100 };
            btnSelectFiles.Click += BtnSelectFiles_Click;

            btnNormalize = new Button() { Text = "자소 결합 실행", Left = 130, Top = 20, Width = 150 };
            btnNormalize.Click += BtnNormalize_Click;

            chkSelectAll = new CheckBox() { Text = "전체 선택", Left = 300, Top = 23, Width = 100 };
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;

            fileList = new CheckedListBox() { Left = 20, Top = 60, Width = 540, Height = 280 };

            this.Controls.Add(btnSelectFiles);
            this.Controls.Add(btnNormalize);
            this.Controls.Add(chkSelectAll);
            this.Controls.Add(fileList);
        }

        private void BtnSelectFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Multiselect = true,
                Title = "자소 결합할 파일 선택",
                Filter = "모든 파일|*.*"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                fileList.Items.Clear();
                foreach (string file in dlg.FileNames)
                {
                    fileList.Items.Add(file, true);
                }
            }
        }

        private void BtnNormalize_Click(object sender, EventArgs e)
        {
            foreach (var item in fileList.CheckedItems)
            {
                string oldPath = item.ToString();
                string dir = Path.GetDirectoryName(oldPath);
                string fileName = Path.GetFileName(oldPath);
                string normalized = fileName.Normalize(NormalizationForm.FormC);

                if (fileName != normalized)
                {
                    string newPath = Path.Combine(dir, normalized);
                    try
                    {
                        File.Move(oldPath, newPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"변경 실패: {fileName}\n→ {ex.Message}");
                    }
                }
            }

            MessageBox.Show("선택한 파일 이름이 자소 결합되었습니다.");
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                fileList.SetItemChecked(i, chkSelectAll.Checked);
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}