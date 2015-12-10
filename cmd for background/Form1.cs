using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cmd_for_background
{
    public partial class Form1 : Form
    {
        string filename;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cfbpe.Properties.Settings.Default.Reload();
            filepass.Text = cfbpe.Properties.Settings.Default.filepass;
            workspece.Text = cfbpe.Properties.Settings.Default.workspece;

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            panel1.Parent = pictureBox1;
            panel1.Location = new Point(0, 0);
        }

        //ファイル or コマンド
        private void button1_Click(object sender, EventArgs e)
        {

            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //「ファイル名」で表示される文字列を指定する
            ofd.FileName = filename;
            //[ファイルの種類]に表示される選択肢を指定する
            ofd.Filter = "バッチファイル(.bat .cmd)|*.bat;*.cmd|すべてのファイル(*.*)|*.*";
            //「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 1;
            //タイトルを設定する
            ofd.Title = "ファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;


            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepass.Text = ofd.FileName;
                filename = Path.GetFileName(@ofd.FileName);
                cfbpe.Properties.Settings.Default.filepass = ofd.FileName;
            }
            else
            {
                //メッセージボックスを表示する
                MessageBox.Show("ファイルが選択されていません",
                    "cmd for background",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        //作業フォルダ
        private void button2_Click(object sender, EventArgs e)
        {
            var Dialog = new CommonOpenFileDialog();
            // フォルダーを開く設定に
            Dialog.IsFolderPicker = true;
            // 読み取り専用フォルダ/コントロールパネルは開かない
            Dialog.EnsureReadOnly = false;
            Dialog.AllowNonFileSystemItems = false;
            // パス指定
            Dialog.DefaultDirectory = Application.StartupPath;
            // 開く
            var Result = Dialog.ShowDialog();
            // もし開かれているなら
            if (Result == CommonFileDialogResult.Ok)
            {
                // ここでいろいろする（開いたフォルダはDialog.FileNameで取得）
                Directory.SetCurrentDirectory(Dialog.FileName);
                workspece.Text = Directory.GetCurrentDirectory();
                cfbpe.Properties.Settings.Default.workspece = Directory.GetCurrentDirectory();
            }
            else
            {
                //メッセージボックスを表示する
                MessageBox.Show("作業フォルダが選択されていません",
                    "cmd for background",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        //実行
        public void run_Click(object sender, EventArgs e)
        {
            if (filepass.Text == null)
            {
                MessageBox.Show("バッチファイルが選択されていません",
                    "cmd for background",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                //Processオブジェクトを作成
                Process p = new System.Diagnostics.Process();

                //出力をストリームに書き込むようにする
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                //OutputDataReceivedイベントハンドラを追加
                p.OutputDataReceived += p_OutputDataReceived;

                p.StartInfo.FileName = System.Environment.GetEnvironmentVariable("ComSpec");
                p.StartInfo.RedirectStandardInput = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = @"/c " + filepass.Text;
                //起動
                p.Start();

                //非同期で出力の読み取りを開始
                p.BeginOutputReadLine();
            }
        }

        //OutputDataReceivedイベントハンドラ
        //行が出力されるたびに呼び出される
        private void p_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //出力された文字列を表示する
            Console.WriteLine(e.Data);

            // Delegateを作成
            Action act = () =>
            {
                if (string.IsNullOrEmpty(e.Data) == false)
                {
                    cw.Text += (e.Data);
                }

                cw.Text += (Environment.NewLine);
            };

            BeginInvoke(act);
        }

        //ErrorDataReceivedイベントハンドラ
        //行が出力されるたびに呼び出される
        private void p_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //出力された文字列を表示する
            Console.WriteLine("ERR>{0}", e.Data);

            // Delegateを作成
            Action act = () =>
            {
                if (string.IsNullOrEmpty(e.Data) == false)
                {
                    cw.Text += (e.Data);
                }

                cw.Text += (Environment.NewLine);
            };

            BeginInvoke(act);
        }

        private void p_Exited(object sender, EventArgs e)
        {
            System.Diagnostics.Process p = new Process();
            p.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                e.Cancel = true; // フォームが閉じるのをキャンセル
                this.Visible = false; // フォームの非表示
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true; // フォームの表示
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal; // 最小化をやめる
            }
            this.Activate(); // フォームをアクティブにする
        }

        private void exit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false; // アイコンをトレイから取り除く
            Application.Exit(); // アプリケーションの終了
        }
    }
}
