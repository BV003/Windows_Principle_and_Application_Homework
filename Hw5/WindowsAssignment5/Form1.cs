using System.Runtime.InteropServices;

namespace WindowsAssignment5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport(@"C:\Users\lwq_0\Desktop\WindowsCode\Hw5\x64\Release\CreateDLL.dll",
            EntryPoint = "test01",
            SetLastError = true,
            CharSet = CharSet.Ansi,
            ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int test01(int a);
        [DllImport(@"C:\Users\lwq_0\Desktop\WindowsCode\Hw5\x64\Release\CreateDLL.dll",
            EntryPoint = "test02",
            SetLastError = true,
            CharSet = CharSet.Ansi,
            ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int test02(int a, int b);
        private void button1_Click(object sender, EventArgs e)
        {
            int a;
            a = int.Parse(textBox1.Text);
            int ans = test01(a);
            if (ans == 0)
            {
                MessageBox.Show("输入数字无法计算");
            }
            else
            {
                MessageBox.Show("阶乘为 " + ans.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int a, b;
            a = int.Parse(textBox2.Text);
            b = int.Parse(textBox3.Text);
            int ans = test02(a, b);
            MessageBox.Show("a - b = " + ans.ToString());
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
