using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WkHtmlToImageWrapper
{
    public class HtmlConverter
    {
        private static string toolFilename = "wkhtmltoimage";
        private static string directory;
        private static string toolFilepath;

        static HtmlConverter()
        {
            directory = Path.GetTempPath();

                toolFilepath = Path.Combine(directory, toolFilename + ".exe");

                if (!File.Exists(toolFilepath))
                {
                    var assembly = typeof(HtmlConverter).GetTypeInfo().Assembly;
                    var type = typeof(HtmlConverter);
                    var ns = type.Namespace;

                    using (var resourceStream = assembly.GetManifestResourceStream($"{ns}.{toolFilename}.exe"))
                    using (var fileStream = File.OpenWrite(toolFilepath))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
        }

        public byte[] FromHtmlString(string html, int width = 1024, ImageFormat format = ImageFormat.Jpg, int quality = 100, int multiplier = 1)
        {
            var filename = Path.Combine(directory, $"{Guid.NewGuid()}.html");
            File.WriteAllText(filename, html);
            var bytes = FromUrl(filename, width, format, quality, multiplier);
            File.Delete(filename);
            return bytes;
        }

        public byte[] FromUrl(string url, int width = 1024, ImageFormat format = ImageFormat.Jpg, int quality = 100, int multiplier = 1)
        {
            var imageFormat = format.ToString().ToLower();
            var filename = Path.Combine(directory, $"{Guid.NewGuid()}.{imageFormat}");

            string args = $"--encoding utf-8 --width {(int)(width * multiplier)} --disable-smart-width --zoom {multiplier} --quality {quality} --format {imageFormat} {url} {filename}";

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

            if (File.Exists(filename))
            {
                var bytes = File.ReadAllBytes(filename);
                File.Delete(filename);
                return bytes;
            }

            throw new Exception("Something went wrong. Please check input parameters");
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new Exception(e.Data);
        }
    }
}