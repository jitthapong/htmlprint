using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            var printerName = comboPrinter.SelectedItem?.ToString() ?? "EPSON TM-T82 Receipt";
            if (string.IsNullOrEmpty(printerName))
                return;

            var html = txtHtml.Text;
            if (string.IsNullOrEmpty(html))
                return;

            try
            {
                HtmlToImage htmlToImage = new HtmlToImage();

                PrintDocument doc = new PrintDocument();
                PrintController printController = new StandardPrintController();

                doc.PrintController = printController;
                doc.PrinterSettings.PrinterName = printerName;

                var pageSetting = doc.DefaultPageSettings;
                var printableArea = doc.DefaultPageSettings.PrintableArea;

                try
                {
                    var bytesData = htmlToImage.ToImage(txtHtml.Text, width: 300);
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
                        var rect = new Rectangle(0, 0, newWidth, newHeight);

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
                            e.Graphics.CompositingMode = CompositingMode.SourceCopy;
                            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            e.Graphics.DrawImage(img, rect);
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
                WkHtmlToImageWrapper.HtmlToImage htmlToImage = new WkHtmlToImageWrapper.HtmlToImage();
                var bytes = htmlToImage.ToImage(txtHtml.Text, width: 300);

                var filePath = Path.Combine(Path.GetTempPath(), "image.png");
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                Process.Start(filePath);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
