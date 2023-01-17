using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WkHtmlToImageWrapper;

namespace HtmlPrint
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                comboPrinter.Items.Add(printer);
            }

            try
            {
                txtHtml.Text = File.ReadAllText("bill.html");
            }
            catch { }
        }

        private void button1_Click(object s, EventArgs a)
        {
            var printerName = comboPrinter.SelectedItem?.ToString() ?? "Summary";
            if (string.IsNullOrEmpty(printerName))
                return;

            var html = txtHtml.Text;
            if (string.IsNullOrEmpty(html))
                return;

            try
            {
                HtmlToImageConverter htmlToImage = new HtmlToImageConverter();

                PrintDocument doc = new PrintDocument();
                PrintController printController = new StandardPrintController();

                doc.PrintController = printController;
                doc.PrinterSettings.PrinterName = printerName;

                var pageSetting = doc.DefaultPageSettings;
                var printableArea = doc.DefaultPageSettings.PrintableArea;

                try
                {
                    var bytesData = htmlToImage.FromHtmlString(txtHtml.Text, width: 300);
                    var adj = int.Parse(txtAdj.Text);
                    using (var stream = new MemoryStream())
                    {
                        stream.Write(bytesData, 0, bytesData.Length);
                        var img = Image.FromStream(stream);
                        img.Save("image.bmp");

                        var targetSize = printableArea.Size;
                        var ratio = Math.Min((targetSize.Width + adj) / img.Width, (targetSize.Height + adj) / img.Height);
                        var newWidth = (int)(img.Width * ratio);
                        var newHeight = (int)(img.Height * ratio);
                        var destRect = new Rectangle(0, 0, newWidth, newHeight);

                        var attributes = new ImageAttributes();
                        attributes.SetColorMatrix(new ColorMatrix(
                         new float[][]
                         {
                             new float[] {.3f, .3f, .3f, 0, 0},
                             new float[] {.59f, .59f, .59f, 0, 0},
                             new float[] {.11f, .11f, .11f, 0, 0},
                             new float[] {0, 0, 0, 1, 0},
                             new float[] {0, 0, 0, 0, 1}
                         }));

                        //var res = new Bitmap(newWidth, newHeight);

                        //using (var graphics = Graphics.FromImage(res))
                        //{
                        //    graphics.CompositingMode = CompositingMode.SourceCopy;
                        //    graphics.CompositingQuality = CompositingQuality.HighQuality;
                        //    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        //    graphics.SmoothingMode = SmoothingMode.HighQuality;
                        //    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        //    graphics.DrawImage(img, rect);
                        //}

                        //res.Save("image_resize.png");
                        //Process.Start("image_resize.png");

                        doc.PrintPage += new PrintPageEventHandler((sender, e) =>
                        {
                            //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                            //e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                            //e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                            //e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            //e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                            //e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            e.Graphics.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);
                            e.HasMorePages = false;
                        });
                        doc.Print();
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                WkHtmlToImageWrapper.HtmlToImageConverter htmlToImage = new WkHtmlToImageWrapper.HtmlToImageConverter();
                var bytes = htmlToImage.FromHtmlString(txtHtml.Text, width: 300, format: WkHtmlToImageWrapper.ImageFormat.Png, quality: 95, zoomLevel: 1);

                var testImgFilePath = Path.Combine(Path.GetTempPath(), "image.png");
                using (var stream = new FileStream(testImgFilePath, FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                Process.Start(testImgFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
