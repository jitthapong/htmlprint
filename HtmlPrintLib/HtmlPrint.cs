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
                var bytesData = htmlToImageConv.FromHtmlString(Html, width: (int)printableArea.Width);

                File.WriteAllBytes("image.jpg", bytesData);

                Process.Start("image.jpg");
            }
            catch (Exception ex)
            {

            }

            //doc.PrintPage += new PrintPageEventHandler((sender, e) =>
            //{
            //    //var visibleBound = e.Graphics.VisibleClipBounds;
            //    //var scale = Math.Min(visibleBound.Width / img.Width, visibleBound.Height / img.Height);
            //    //var newWidth = (int)(img.Width * scale);
            //    //var newHeight = (int)(img.Height * scale);
            //    //var rect = new Rectangle(0, 0, newWidth, newHeight);

            //    var img = Image.FromFile("image.jpg");
            //    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            //    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            //    e.Graphics.DrawImage(img, printableArea.X, 0);
            //});
            //doc.Print();
        }
    }
}
