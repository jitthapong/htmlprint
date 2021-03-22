using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;

namespace WkHtmlToImageWrapper
{
    public class HtmlToImage
    {
        private int _resizeRatio;

        public string Html { get; set; }
        public string PrinterName { get; set; }
        public int ResizeRatio
        {
            get => _resizeRatio;
            set => _resizeRatio = value * 10;
        }

        public HtmlToImage(string html, string printerName = "")
        {
            Html = html;
            PrinterName = printerName;
        }

        public byte[] Export(int width=213, ImageFormat format = ImageFormat.Png)
        {
            var htmlToImageConv = new HtmlConverter();
            var bytesData = htmlToImageConv.FromHtmlString(Html, width: width, format: format);
            return bytesData;
        }

        public void Print()
        {
            PrintDocument doc = new PrintDocument();
            PrintController printController = new StandardPrintController();

            doc.PrintController = printController;
            doc.PrinterSettings.PrinterName = PrinterName;

            var pageSetting = doc.DefaultPageSettings;
            var printableArea = doc.DefaultPageSettings.PrintableArea;

            try
            {
                var htmlToImageConv = new HtmlConverter();
                var bytesData = htmlToImageConv.FromHtmlString(Html, width: (int)pageSetting.PaperSize.Width, format: ImageFormat.Bmp, multiplier: 4);

                using (var stream = new MemoryStream())
                {
                    stream.Write(bytesData, 0, bytesData.Length);
                    var img = Image.FromStream(stream);
                    img.Save("image.bmp");

                    var targetSize = printableArea.Size;
                    var ratio = Math.Min((targetSize.Width + ResizeRatio) / img.Width, (targetSize.Height + ResizeRatio) / img.Height);
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
    }
}
