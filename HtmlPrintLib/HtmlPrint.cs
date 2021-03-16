using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;

namespace HtmlPrintLib
{
    public class HtmlPrint
    {
        public string Html { get; set; }
        public string PrinterName { get; set; }

        public HtmlPrint(string html, string printerName)
        {
            Html = html;
            PrinterName = printerName;
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
                var bytesData = htmlToImageConv.FromHtmlString(Html, width: (int)pageSetting.PaperSize.Width, format: ImageFormat.Png, multiplier: 4);

                File.WriteAllBytes("image.png", bytesData);
            }
            catch (Exception ex)
            {

            }

            doc.PrintPage += new PrintPageEventHandler((sender, e) =>
            {
                var img = Image.FromFile("image.png");
                var targetSize = printableArea.Size;
                var scale = Math.Min(targetSize.Width / img.Width, targetSize.Height / img.Height);
                var newWidth = (int)(img.Width * scale);
                var newHeight = (int)(img.Height * scale);
                var rect = new Rectangle(0, 0, newWidth, newHeight);
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawImage(img, rect);

                img.Save("image_resize.png");

                e.HasMorePages = false;
            });
            doc.Print();
        }
    }
}
