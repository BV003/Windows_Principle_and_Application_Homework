using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MultiImageProcessor
{
    public partial class Form1 : Form
    {
        Queue<string> imageQueue = new Queue<string>();
        bool isCancelProcessing = false;
        Queue<ProgressInfo> progressQueue = new Queue<ProgressInfo>();

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

        private void ProcessImageInThread(string filePath, string processingOption)
        {
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
            // 这里后续可以添加代码将处理后的图像保存或者更新到界面展示等操作
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

        private void UpdateProgress()
        {
            if (progressQueue.Count > 0)
            {
                ProgressInfo progress = progressQueue.Dequeue();

                // 在ListBox中更新文件状态
                int listBoxIndex = listBox1.Items.IndexOf(progress.FileName + " [待处理]");
                if (listBoxIndex != -1)
                {
                    listBox1.Items[listBoxIndex] = progress.FileName + $" [ {progress.Status} ]";
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图像文件|*.jpg;*.png;*.jpeg";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                //imageQueue.Enqueue(filePath);
                listBox1.Items.Add(filePath + " [待处理]");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (listBox1.SelectedItems.Count > 0)
            {
                string selectedItem = listBox1.SelectedItem.ToString();
                //string filePath = selectedItem.Substring(0, selectedItem.IndexOf(" [待处理]"));
                int startIndex = selectedItem.LastIndexOf(" [");
                string filePath = selectedItem.Substring(0, startIndex);
                //imageQueue = new Queue<string>(imageQueue.Where(x => x != filePath));
                listBox1.Items.Remove(listBox1.SelectedItem);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            isCancelProcessing = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string processingOption = comboBox1.SelectedItem.ToString();
            string selectedfilePath = listBox1.SelectedItem.ToString();
            int startIndex = selectedfilePath.LastIndexOf(" [");
            string filePath = selectedfilePath.Substring(0, startIndex);

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
                        pictureBox1.Image = processedImage;
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
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
class ProgressInfo//声明用于存储处理进度信息的队列（比如包含文件名、处理状态等内容）
{
    public string FileName;
    public string Status;
}


