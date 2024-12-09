﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MultiImageProcessor
{
    public partial class Form1 : Form
    {
        Queue<string> imageQueue = new Queue<string>();
        bool isCancelProcessing = false;
        Queue<ProgressInfo> progressQueue = new Queue<ProgressInfo>();
        private SemaphoreSlim listBoxAndProgressQueueSemaphore = new SemaphoreSlim(1, 1);

        private Bitmap ProcessImageGrayScale(string filePath)//灰度处理函数
        {
            Bitmap original = new Bitmap(filePath);
            Bitmap grayScale = new Bitmap(original.Width, original.Height);
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color originalColor = original.GetPixel(x, y);
                    int gray = (int)(originalColor.R * 0.299 + originalColor.G * 0.587 + originalColor.B * 0.114);
                    grayScale.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            return grayScale;
        }

        private Bitmap ProcessImageEnlarge(string filePath, double factor)//放大200%
        {
            Bitmap original = new Bitmap(filePath);
            int newWidth = (int)(original.Width * factor);
            int newHeight = (int)(original.Height * factor);
            Bitmap enlarged = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(enlarged))
            {
                g.DrawImage(original, 0, 0, newWidth, newHeight);
            }
            return enlarged;
        }

        private Bitmap ProcessImageReduce(string filePath, float factor)//缩小50%
        {
            Bitmap original = new Bitmap(filePath);
            int newWidth = (int)(original.Width * factor);
            int newHeight = (int)(original.Height * factor);
            Bitmap reduced = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(reduced))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, newWidth, newHeight);
            }
            return reduced;
        }

        private Bitmap ProcessImageRotateClockwise(string filePath)//顺时针90度
        {
            Bitmap original = new Bitmap(filePath);
            Bitmap rotated = new Bitmap(original.Height, original.Width);
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color color = original.GetPixel(x, y);
                    rotated.SetPixel(original.Height - 1 - y, x, color);
                }
            }
            return rotated;
        }

        private Bitmap ProcessImageRotateCounterclockwise(string filePath)//逆时针90度
        {
            Bitmap original = new Bitmap(filePath);
            Bitmap rotated = new Bitmap(original.Height, original.Width);
            for (int x = 0; x < original.Width; x++)
            {
                for (int y = 0; y < original.Height; y++)
                {
                    Color color = original.GetPixel(x, y);
                    rotated.SetPixel(y, original.Width - 1 - x, color);
                }
            }
            return rotated;
        }

        private async void ProcessImageInThread(string filePath, string processingOption)
        {

            if (isCancelProcessing)
            {
                return;
            }

            ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() => new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId));
            Random random = threadLocalRandom.Value;
            int delaySeconds = random.Next(4, 5);
            Thread.Sleep(delaySeconds * 1000);

            Bitmap processedImage = null;
            switch (processingOption)
            {
                case "灰度":
                    processedImage = ProcessImageGrayScale(filePath);
                    break;
                case "放大至200%":
                    processedImage = ProcessImageEnlarge(filePath, 2);
                    break;
                case "缩小至50%":
                    processedImage = ProcessImageReduce(filePath, 0.5f);
                    break;
                case "顺时针旋转90°":
                    processedImage = ProcessImageRotateClockwise(filePath);
                    break;
                case "逆时针旋转90°":
                    processedImage = ProcessImageRotateCounterclockwise(filePath);
                    break;
            }
            if (processedImage != null)
            {
                string fileNameWithoutExtension = Path.ChangeExtension(filePath, null);
                string fileExtension = Path.GetExtension(filePath);
                string processedFilePath = "";

                // 根据处理选项确定保存路径
                switch (processingOption)
                {
                    case "灰度":
                        processedFilePath = fileNameWithoutExtension + "_gray" + fileExtension;
                        break;
                    case "放大至200%":
                        processedFilePath = fileNameWithoutExtension + "_enlarged" + fileExtension;
                        break;
                    case "缩小至50%":
                        processedFilePath = fileNameWithoutExtension + "_reduced" + fileExtension;
                        break;
                    case "顺时针旋转90°":
                        processedFilePath = fileNameWithoutExtension + "_rotated_clockwise" + fileExtension;
                        break;
                    case "逆时针旋转90°":
                        processedFilePath = fileNameWithoutExtension + "_rotated_counterclockwise" + fileExtension;
                        break;
                }

                // 保存处理后的图像
                if (!File.Exists(processedFilePath))
                {
                    processedImage.Save(processedFilePath);
                }

            }
            await listBoxAndProgressQueueSemaphore.WaitAsync();
            try
            {
                //处理完成后，找到对应的ProgressInfo对象并更新状态
                foreach (ProgressInfo progress in progressQueue)
                {
                    if (progress.FileName == filePath && progress.Status == "处理中")
                    {
                        progress.Status = "已处理";
                        // 更新ListBox中的状态
                        int index = listBox1.Items.IndexOf(filePath + " [处理中]");
                        if (index != -1)
                        {
                            listBox1.Items[index] = filePath + " [已处理]";
                        }
                        break;
                    }
                }
            }
            finally
            {
                listBoxAndProgressQueueSemaphore.Release();
            }

        }

        private System.Timers.Timer timer;

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
            timer = new System.Timers.Timer(500); // 3000毫秒即3秒，设置时间间隔
                                                  // 绑定计时器的Elapsed事件处理方法，当时间间隔到达时会触发这个方法
            timer.Elapsed += Timer_Elapsed;
            // 设置计时器为自动重启，即每次时间间隔到达并执行完事件处理方法后，会自动开始下一轮计时
            timer.AutoReset = true;
            // 启动计时器
            timer.Start();
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            isCancelProcessing = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedOption = comboBox1.SelectedItem.ToString();
            List<(string filePath, string status)> imageStatusList = new List<(string filePath, string status)>();
            foreach (string filePath in imageQueue)
            {
                string status = GetImageStatusFromFile(filePath, selectedOption);
                imageStatusList.Add((filePath, status));
            }
            listBox1.Items.Clear();
            foreach (var (filePath, status) in imageStatusList)
            {
                listBox1.Items.Add(filePath + $" [{status}]");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图像文件|*.jpg;*.png;*.jpeg";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                imageQueue.Enqueue(filePath);
                listBox1.Items.Add(filePath + " [待处理]");
            }
        }

        private void button2_Click(object sender, EventArgs e)//删除按钮
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请先选中要删除的文件！");
                return;
            }
            List<string> itemsToRemove = new List<string>();
            while (listBox1.SelectedItems.Count > 0)
            {
                string selectedItem = listBox1.SelectedItem.ToString();
                int startIndex = selectedItem.LastIndexOf(" [");
                string filePath = selectedItem.Substring(0, startIndex);
                itemsToRemove.Add(filePath);
                listBox1.Items.Remove(listBox1.SelectedItem);
            }

            Queue<string> newImageQueue = new Queue<string>();
            foreach (string filePath in imageQueue)
            {
                if (!itemsToRemove.Contains(filePath))
                {
                    newImageQueue.Enqueue(filePath);
                }
            }
            imageQueue = newImageQueue;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            isCancelProcessing = true;
            //重新更新ListBox
            string selectedOption = comboBox1.SelectedItem.ToString();
            List<(string filePath, string status)> imageStatusList = new List<(string filePath, string status)>();
            foreach (string filePath in imageQueue)
            {
                string status = GetImageStatusFromFile(filePath, selectedOption);
                imageStatusList.Add((filePath, status));
            }
            listBox1.Items.Clear();
            foreach (var (filePath, status) in imageStatusList)
            {
                listBox1.Items.Add(filePath + $" [{status}]");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string processingOption = comboBox1.SelectedItem.ToString();
            foreach (string filePath in imageQueue)
            {
                string tempOption = processingOption;
                // 创建ProgressInfo对象并添加到队列
                ProgressInfo progress = new ProgressInfo
                {
                    FileName = filePath,
                    Status = "处理中"
                };
                progressQueue.Enqueue(progress);
                // 更新ListBox中的状态
                int index = listBox1.Items.IndexOf(filePath + " [待处理]");
                if (index != -1)
                {
                    listBox1.Items[index] = filePath + " [处理中]";
                }
                Thread thread = new Thread(() => ProcessImageInThread(filePath, tempOption));
                thread.Start();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                string selectedItem = listBox1.SelectedItem.ToString();
                int startIndex = selectedItem.LastIndexOf(" [");
                string originalFilePath = selectedItem.Substring(0, startIndex);
                string fileNameWithoutExtension = Path.ChangeExtension(originalFilePath, null);
                string fileExtension = Path.GetExtension(originalFilePath);

                string selectedOption = comboBox1.SelectedItem.ToString();
                string processedFilePath = "";

                // 根据选择的不同处理选项，确定处理后图像的文件名
                switch (selectedOption)
                {
                    case "灰度":
                        processedFilePath = fileNameWithoutExtension + "_gray" + fileExtension;
                        break;
                    case "放大至200%":
                        processedFilePath = fileNameWithoutExtension + "_enlarged" + fileExtension;
                        break;
                    case "缩小至50%":
                        processedFilePath = fileNameWithoutExtension + "_reduced" + fileExtension;
                        break;
                    case "顺时针旋转90°":
                        processedFilePath = fileNameWithoutExtension + "_rotated_clockwise" + fileExtension;
                        break;
                    case "逆时针旋转90°":
                        processedFilePath = fileNameWithoutExtension + "_rotated_counterclockwise" + fileExtension;
                        break;
                }

                if (File.Exists(processedFilePath))
                {
                    try
                    {
                        Bitmap processedImage = new Bitmap(processedFilePath);
                        string selectedFilePath = GetFilePathFromSelectedItem(listBox1.SelectedItem.ToString());

                        ImageDisplayForm imageDisplayForm = new ImageDisplayForm();
                        // 假设你有获取原图和处理后图片的方法，这里简单示例获取方式，实际需根据你的具体代码调整

                        Image originalImage = Image.FromFile(selectedFilePath);
                        imageDisplayForm.OriginalImage = originalImage;
                        imageDisplayForm.ProcessedImage = processedImage;
                        imageDisplayForm.DisplayImages();

                        imageDisplayForm.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载处理后图像时出错: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("未找到对应的处理后图像文件，请先确保已成功处理该图像。");
                }
            }
            else
            {
                MessageBox.Show("请先选中要查看的文件！");
                return;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private string GetFilePathFromSelectedItem(string selectedItemText)
        {
            int index = selectedItemText.IndexOf(" [");
            if (index != -1)
            {
                return selectedItemText.Substring(0, index);
            }
            return selectedItemText;
        }
        private List<(string filePath, string status)> GetImageStatusList()
        {
            List<(string filePath, string status)> result = new List<(string filePath, string status)>();
            // 假设你有一个imageQueue来存储图片路径
            foreach (string filePath in imageQueue)
            {
                string status = GetImageStatus(filePath);
                result.Add((filePath, status));
            }
            return result;
        }
        private string GetImageStatus(string filePath)
        {
            // 假设你有一个progressQueue来存储处理进度信息
            foreach (ProgressInfo progress in progressQueue)
            {
                if (progress.FileName == filePath)
                {
                    return progress.Status;
                }
            }
            return "未处理";
        }
        private string GetImageStatusFromFile(string filePath, string processingOption)
        {
            //string[] processingOptions = { "灰度", "放大至200%", "缩小至50%", "顺时针旋转90°", "逆时针旋转90°" };
            //foreach (string option in processingOptions)
            //{
            string fileNameWithoutExtension = Path.ChangeExtension(filePath, null);
            string fileExtension = Path.GetExtension(filePath);
            string processedFileSuffix = "";
            switch (processingOption)
            {
                case "灰度":
                    processedFileSuffix = "_gray" + fileExtension;
                    break;
                case "放大至200%":
                    processedFileSuffix = "_enlarged" + fileExtension;
                    break;
                case "缩小至50%":
                    processedFileSuffix = "_reduced" + fileExtension;
                    break;
                case "顺时针旋转90°":
                    processedFileSuffix = "_rotated_clockwise" + fileExtension;
                    break;
                case "逆时针旋转90°":
                    processedFileSuffix = "_rotated_counterclockwise" + fileExtension;
                    break;
            }
            string processedFilePath = Path.Combine(Path.GetDirectoryName(filePath), fileNameWithoutExtension + processedFileSuffix);
            if (File.Exists(processedFilePath))
            {
                return "已处理";
            }

            return "待处理";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
class ProgressInfo//声明用于存储处理进度信息的队列（比如包含文件名、处理状态等内容）
{
    public string FileName;
    public string Status;
}


