using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        private Bitmap ProcessImageBlur(string filePath)//模糊处理
        {
            Bitmap originalImage = new Bitmap(filePath);
            Bitmap blurredImage = new Bitmap(originalImage.Width, originalImage.Height);

            // 定义模糊的半径
            int blurRadius = 5;
            int diameter = blurRadius * 2 + 1;

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = Color.Empty;
                    int r = 0, g = 0, b = 0, count = 0;

                    // 遍历模糊半径内的像素
                    for (int xx = -blurRadius; xx <= blurRadius; xx++)
                    {
                        for (int yy = -blurRadius; yy <= blurRadius; yy++)
                        {
                            int newX = x + xx;
                            int newY = y + yy;

                            // 检查边界
                            if (newX >= 0 && newX < originalImage.Width && newY >= 0 && newY < originalImage.Height)
                            {
                                pixelColor = originalImage.GetPixel(newX, newY);
                                r += pixelColor.R;
                                g += pixelColor.G;
                                b += pixelColor.B;
                                count++;
                            }
                        }
                    }

                    // 计算平均色值
                    if (count > 0)
                    {
                        r /= count;
                        g /= count;
                        b /= count;
                    }

                    // 设置模糊后图像的像素
                    blurredImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return blurredImage;
        }
        private Bitmap ProcessImageEdgeDetection(string filePath)//边缘检测
        {
            Bitmap originalImage = new Bitmap(filePath);
            Bitmap edgeImage = new Bitmap(originalImage.Width, originalImage.Height);

            // Sobel 算子
            int[,] gx = new int[,]
            {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
            };

            int[,] gy = new int[,]
            {
        { 1, 2, 1 },
        { 0, 0, 0 },
        { -1, -2, -1 }
            };

            for (int x = 1; x < originalImage.Width - 1; x++)
            {
                for (int y = 1; y < originalImage.Height - 1; y++)
                {
                    int sumX = 0;
                    int sumY = 0;

                    // 应用 Sobel 算子
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color pixelColor = originalImage.GetPixel(x + i, y + j);
                            int gray = (int)(pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114); // 转为灰度

                            sumX += gx[i + 1, j + 1] * gray;
                            sumY += gy[i + 1, j + 1] * gray;
                        }
                    }

                    // 计算最终强度
                    int magnitude = (int)Math.Sqrt(sumX * sumX + sumY * sumY);
                    magnitude = Math.Min(255, Math.Max(0, magnitude)); // 限制在[0, 255]范围内

                    // 设置边缘图像的像素
                    edgeImage.SetPixel(x, y, Color.FromArgb(magnitude, magnitude, magnitude));
                }
            }

            return edgeImage;
        }
        private Bitmap ProcessImageFlipHorizontal(string filePath)//水平翻转
        {
            Bitmap originalImage = new Bitmap(filePath);
            Bitmap flippedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    // 进行水平翻转
                    flippedImage.SetPixel(originalImage.Width - 1 - x, y, pixelColor);
                }
            }

            return flippedImage;
        }
        private Bitmap ProcessImageBrightness(string filePath)//增加亮度
        {
            int brightnessAdjustment = 30; // 默认增加亮度的值

            Bitmap originalImage = new Bitmap(filePath);
            Bitmap brightenedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);

                    // 调整每个颜色分量
                    int r = Math.Min(255, Math.Max(0, pixelColor.R + brightnessAdjustment));
                    int g = Math.Min(255, Math.Max(0, pixelColor.G + brightnessAdjustment));
                    int b = Math.Min(255, Math.Max(0, pixelColor.B + brightnessAdjustment));

                    // 设置调整后图像的像素
                    brightenedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return brightenedImage;
        }
        private async void ProcessImageInThread(string filePath, string processingOption, CancellationToken cancellationToken)
        {

            ThreadLocal<Random> threadLocalRandom = new ThreadLocal<Random>(() => new Random(Environment.TickCount ^ Thread.CurrentThread.ManagedThreadId));
            Random random = threadLocalRandom.Value;
            int delaySeconds = random.Next(4,6);
            Thread.Sleep(delaySeconds * 1000);

            // 检查是否请求取消处理
            if (cancellationToken.IsCancellationRequested)
            {
                // 处理取消请求，例如记录日志、清理资源等
                return;
            }

            // 执行图像处理操作

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
                case "模糊":
                    processedImage = ProcessImageBlur(filePath);
                    break;
                case "锐化":
                    processedImage = ProcessImageEdgeDetection(filePath);
                    break;
                case "水平翻转":
                    processedImage = ProcessImageFlipHorizontal(filePath);
                    break;
                case "增加亮度":
                    processedImage = ProcessImageBrightness(filePath);
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
                    case "模糊":
                        processedFilePath = fileNameWithoutExtension + "_blurred" + fileExtension;
                        break;
                    case "锐化":
                        processedFilePath = fileNameWithoutExtension + "_edge" + fileExtension;
                        break;
                    case "水平翻转":
                        processedFilePath = fileNameWithoutExtension + "_flipped" + fileExtension;
                        break;
                    case "增加亮度":
                        processedFilePath = fileNameWithoutExtension + "_brightened" + fileExtension;
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
        private CancellationTokenSource cts;
        private void button5_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
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
            cts = new CancellationTokenSource();
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
                
                Thread thread = new Thread(() => ProcessImageInThread(filePath, tempOption, cts.Token));
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
                    case "模糊":
                        processedFilePath = fileNameWithoutExtension + "_blurred" + fileExtension;
                        break;
                    case "锐化":
                        processedFilePath = fileNameWithoutExtension + "_edge" + fileExtension;
                        break;
                    case "水平翻转":
                        processedFilePath = fileNameWithoutExtension + "_flipped" + fileExtension;
                        break;
                    case "增加亮度":
                        processedFilePath = fileNameWithoutExtension + "_brightened" + fileExtension;
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
                case "模糊":
                    processedFileSuffix = "_blurred" + fileExtension;
                    break;
                case "锐化":
                    processedFileSuffix = "_edge" + fileExtension;
                    break;
                case "水平翻转":
                    processedFileSuffix = "_flipped" + fileExtension;
                    break;
                case "增加亮度":
                    processedFileSuffix= "_brightened"+fileExtension;
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


