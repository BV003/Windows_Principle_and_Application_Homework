using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiImageProcessor
{
    public partial class ImageDisplayForm : Form
    {
        public ImageDisplayForm()
        {
            InitializeComponent();
        }
        public Image OriginalImage { get; set; }
        public Image ProcessedImage { get; set; }

        public void DisplayImages()
        {
            if (OriginalImage != null)
            {
                pictureBox1.Image = new Bitmap(OriginalImage);
            }
            if (ProcessedImage != null)
            {
                pictureBox2.Image = new Bitmap(ProcessedImage);
            }
        }
        private void ImageDisplayForm_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
