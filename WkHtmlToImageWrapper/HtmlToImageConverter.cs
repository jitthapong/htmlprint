using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WkHtmlToImageWrapper
{
    public class HtmlToImageConverter
    {
        private static string toolFilename = "wkhtmltoimage";
        private static string directory;
        private static string toolFilepath;

        static HtmlToImageConverter()
        {
            directory = Path.GetTempPath();

            toolFilepath = Path.Combine(directory, toolFilename + ".exe");

            if (!File.Exists(toolFilepath))
            {
                var assembly = typeof(HtmlToImageConverter).GetTypeInfo().Assembly;
                var type = typeof(HtmlToImageConverter);
                var ns = type.Namespace;

                using (var resourceStream = assembly.GetManifestResourceStream($"{ns}.{toolFilename}.exe"))
                using (var fileStream = File.OpenWrite(toolFilepath))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        public byte[] FromHtmlString(string html, int width = 1024, ImageFormat format = ImageFormat.Jpg, int quality = 100, int zoomLevel = 1)
        {
            var htmlFileName = Path.Combine(directory, $"{Guid.NewGuid()}.html");
            File.WriteAllText(htmlFileName, html);

            try
            {
                var imageFormat = format.ToString().ToLower();
                var imgFileName = Path.Combine(directory, $"{Guid.NewGuid()}.{imageFormat}");

                string args = $"--encoding utf-8 --width {(int)(width * zoomLevel)} --disable-smart-width --zoom {zoomLevel} --quality {quality} --format {imageFormat} {htmlFileName} {imgFileName}";

                Process process = Process.Start(new ProcessStartInfo(toolFilepath, args)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = directory,
                    RedirectStandardError = true
                });

                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.WaitForExit();

                if (File.Exists(imgFileName))
                {
                    var bytes = File.ReadAllBytes(imgFileName);
                    File.Delete(imgFileName);
                    return bytes;
                }
            }
            finally
            {
                try
                {
                    File.Delete(htmlFileName);
                }
                catch { }
            }
            throw new Exception("Something went wrong. Please check input parameters");
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new Exception(e.Data);
        }
    }
}