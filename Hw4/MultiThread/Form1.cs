using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiThread
{
    public partial class Form1 : Form
    {
        private SemaphoreSlim _empty;
        private SemaphoreSlim _full;
        private Mutex _mutex;
        private Queue<int> _buffer;
        private int _bufferSize = 10;
        private Random _random = new Random();
        public Form1()
        {
            InitializeComponent();
            _buffer = new Queue<int>(_bufferSize);
            _empty = new SemaphoreSlim(_bufferSize);
            _full = new SemaphoreSlim(0);
            _mutex = new Mutex();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int producerCount = int.Parse(textBox2.Text);
            for (int i = 0; i < producerCount; i++)
            {
                int producerId = i + 1; // 生产者编号
                Thread producerThread = new Thread(() => Producer(producerId));
                producerThread.Start();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int consumerCount = int.Parse(textBox3.Text);
            for (int i = 0; i < consumerCount; i++)
            {
                int consumerId = i + 1; // 消费者编号
                Thread consumerThread = new Thread(() => Consumer(consumerId));
                consumerThread.Start();
            }
        }
        private void Producer(int producerId)
        {
            while (true)
            {
                int item = _random.Next(100);
                _empty.Wait();
                _mutex.WaitOne();

                _buffer.Enqueue(item);
                UpdateTextBox($"生产者 {producerId}: 生产 {item}");

                _mutex.ReleaseMutex();
                _full.Release();

                Thread.Sleep(500); // 模拟工作
            }
        }

        private void Consumer(int consumerId)
        {
            while (true)
            {
                _full.Wait();
                _mutex.WaitOne();

                int item = _buffer.Dequeue();
                UpdateTextBox($"消费者 {consumerId}: 消费 {item}");

                _mutex.ReleaseMutex();
                _empty.Release();

                Thread.Sleep(1000); // 模拟工作
            }
        }

        private void UpdateTextBox(string message)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() => textBox1.AppendText(message + Environment.NewLine)));
            }
            else
            {
                textBox1.AppendText(message + Environment.NewLine);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }
}
