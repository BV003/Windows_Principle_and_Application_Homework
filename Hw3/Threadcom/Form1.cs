using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Threadcom
{
    public partial class demo : Form
    {
        public const int WM_COPYDATA = 0x004A;
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_COPYDATA:
                    COPYDATASTRUCT cds = new COPYDATASTRUCT();
                    Type t = cds.GetType();
                    cds = (COPYDATASTRUCT)m.GetLParam(t);
                    string strResult = cds.lpData;
                    textBox1.Text = textBox1.Text + Environment.NewLine + strResult;//将每次接收到的结果分行输出
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }


        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        private void strOutputHandler(object sendingProcess,DataReceivedEventArgs outLine)
        {
            //通过FindWindow API的方式找到目标进程句柄，然后发送消息
            IntPtr WINDOW_HANDLER = FindWindow(null,"demo");
            if (WINDOW_HANDLER != IntPtr.Zero)
            {
                COPYDATASTRUCT mystr = new COPYDATASTRUCT();
                mystr.dwData = (IntPtr)0;
                if (string.IsNullOrEmpty(outLine.Data))
                {
                    mystr.cbData = 0;
                    mystr.lpData = ""
                    ;
                }
                else
                {
                    byte[] sarr = System.Text.Encoding.Unicode.GetBytes(outLine.Data);
                    mystr.cbData = sarr.Length + 1;
                    mystr.lpData = outLine.Data;
                }
                SendMessage(WINDOW_HANDLER, WM_COPYDATA, 0, ref mystr);
            }
        }
        public demo()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            // 是否使用外壳程序
            process.StartInfo.UseShellExecute = false;
            // 是否在新窗口中启动该进程的值
            process.StartInfo.CreateNoWindow = true;
            // 重定向输入流
            process.StartInfo.RedirectStandardInput = true;
            // 重定向输出流
            process.StartInfo.RedirectStandardOutput = true;
            //使ping命令执行九次
            // 假设 textBox2 是您的输入框控件
            string strCmd;
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                strCmd = "ping www.sohu.com -n 10";
            }
            else
            {
                strCmd = $"ping {textBox2.Text.Trim()} -n 10";
            }
            process.Start();
            process.StandardInput.WriteLine(strCmd);
            process.StandardInput.WriteLine("exit");
            // 获取输出信息
            textBox1.Text = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            // 是否使用外壳程序
            process.StartInfo.UseShellExecute = false;
            // 是否在新窗口中启动该进程的值
            process.StartInfo.CreateNoWindow = true;
            // 重定向输入流
            process.StartInfo.RedirectStandardInput = true;
            // 重定向输出流
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += new DataReceivedEventHandler(strOutputHandler);
            //使ping命令执行九次
            string strCmd;
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                strCmd = "ping www.sohu.com -n 10";
            }
            else
            {
                strCmd = $"ping {textBox2.Text.Trim()} -n 10";
            }
            process.Start();
            process.BeginOutputReadLine();
            process.StandardInput.WriteLine(strCmd);
            process.StandardInput.WriteLine("exit");

        }
    }
}
