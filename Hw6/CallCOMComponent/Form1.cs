using System;
using System.Windows.Forms;

namespace CallCOMComponent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 创建COM对象实例
            var expressObj = new ExpressImpl();
            //// 调用minus方法并显示结果
            //string minusResult = expressObj.minus(23, 14);
            //MessageBox.Show(minusResult);

            //// 调用divide方法并显示结果
            //string divideResult = expressObj.divide(33, 8);
            //MessageBox.Show(divideResult);

            //// 测试除零情况
            //divideResult = expressObj.divide(5, 0);
            //MessageBox.Show(divideResult);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var expressObj = new ExpressImpl();
            if (int.TryParse(textBox1.Text, out int a) && int.TryParse(textBox2.Text, out int b))
            {
                string result = expressObj.minus(a, b);
                MessageBox.Show($"结果为：{result}");
            }
            else
            {
                MessageBox.Show("请输入有效的数字。");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var expressObj = new ExpressImpl();
            if (int.TryParse(textBox1.Text, out int a) && int.TryParse(textBox2.Text, out int b) && b != 0)
            {
                string result = expressObj.divide(a, b);
                MessageBox.Show($"结果为：{result}");
            }
            else
            {
                MessageBox.Show("请输入有效的数字，且除数不能为 0。");
            }
        }
    }
}
