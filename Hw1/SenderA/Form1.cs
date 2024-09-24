using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CopyDataStruct;

namespace SenderA
{
    public partial class Form1 : Form
    {
        public const int WM_COPYDATA = 0x004A;

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr WINDOW_HANDLER = FindWindow(null, "FormA");
            if (WINDOW_HANDLER != IntPtr.Zero)
            {
                string text = this.textBox1.Text;
                byte[] sarr = System.Text.Encoding.Default.GetBytes(text);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)100;
                cds.lpData = text;
                cds.cbData = len + 1;
                SendMessage(WINDOW_HANDLER, WM_COPYDATA, 0, ref cds);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IntPtr WINDOW_HANDLER = FindWindow(null, "FormB");
            if (WINDOW_HANDLER != IntPtr.Zero)
            {
                string text = this.textBox1.Text;
                byte[] sarr = System.Text.Encoding.Default.GetBytes(text);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)100;
                cds.lpData = text;
                cds.cbData = len + 1;
                SendMessage(WINDOW_HANDLER, WM_COPYDATA, 0, ref cds);
            }
        }
    }
}
