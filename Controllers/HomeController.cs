using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Img2Txt.Models;
using System.Net;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNet.Hosting;
using System.Text.RegularExpressions;
using System.Text;

namespace Img2Txt.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _env;
        public HomeController(IHostingEnvironment env)
        {
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConvertImage()
        {
            var result = string.Empty;
            using (var memoryStream = new MemoryStream())
            {
                await HttpContext.Request.Body.CopyToAsync(memoryStream);

                MultipartParser parser = new MultipartParser(memoryStream.ToArray());
                if (parser.Success)
                {
                    result = ConvertToText(parser.FileContents, parser.ContentType);
                }
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Convert(ImageToConvert imageToConvert)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(imageToConvert.UrlOfTheImage))
            {
                WebClient webClient = new WebClient();
                var imageBytes = await webClient.DownloadDataTaskAsync(new Uri(imageToConvert.UrlOfTheImage));
                result = ConvertToText(imageBytes);
            }
            return Json(result);
        }

        private string ConvertToText(byte[] imageBytes, string format = null)
        {
            var imageFormat = GetImageFormat(imageBytes);
            var imageFormatLiteral = format ?? GetImageFormatForOutput(imageFormat);

            string base64String = System.Convert.ToBase64String(imageBytes);
            return string.Format("data:{0};base64,{1}", imageFormatLiteral, base64String);
        }

        private string GetImageFormatForOutput(ImageFormat imageFormat)
        {
            var imageFormatLiteral = string.Empty;
            if (ImageFormat.Jpeg.Equals(imageFormat))
            {
                imageFormatLiteral = "image/jpg";
            }
            if (ImageFormat.Gif.Equals(imageFormat))
            {
                imageFormatLiteral = "image/gif";
            }
            if (ImageFormat.Png.Equals(imageFormat))
            {
                imageFormatLiteral = "image/png";
            }
            return imageFormatLiteral;
        }

        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (var image = Image.FromStream(ms))
                    {
                        return image.RawFormat;
                    }
                }
            }
            catch
            {
                return default(ImageFormat);
            }
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }

    //http://multipartparser.codeplex.com/
    public class MultipartParser
    {
        public MultipartParser(Stream stream)
        {
            this.Parse(stream, Encoding.UTF8);
        }

        public MultipartParser(byte[] data)
        {
            this.Parse(data);
        }

        public MultipartParser(Stream stream, Encoding encoding)
        {
            this.Parse(stream, encoding);
        }

        private void Parse(byte[] data)
        {
            // Copy to a string for header parsing
            string content = Encoding.UTF8.GetString(data);

            // The first line should contain the delimiter
            int delimiterEndIndex = content.IndexOf("\r\n");

            if (delimiterEndIndex > -1)
            {
                string delimiter = content.Substring(0, content.IndexOf("\r\n"));

                // Look for Content-Type
                Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                Match contentTypeMatch = re.Match(content);

                // Look for filename
                re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                Match filenameMatch = re.Match(content);

                // Did we find the required values?
                if (contentTypeMatch.Success && filenameMatch.Success)
                {
                    // Set properties
                    this.ContentType = contentTypeMatch.Value.Trim();
                    this.Filename = filenameMatch.Value.Trim();

                    // Get the start & end indexes of the file contents
                    int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                    byte[] delimiterBytes = Encoding.UTF8.GetBytes("\r\n" + delimiter);
                    int endIndex = IndexOf(data, delimiterBytes, startIndex);

                    int contentLength = endIndex - startIndex;

                    // Extract the file contents from the byte array
                    byte[] fileData = new byte[contentLength];

                    Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                    this.FileContents = fileData;
                    this.Success = true;
                }
            }
        }

        private void Parse(Stream stream, Encoding encoding)
        {
            this.Success = false;

            // Read the stream into a byte array
            byte[] data = ToByteArray(stream);

            // Copy to a string for header parsing
            string content = encoding.GetString(data);

            // The first line should contain the delimiter
            int delimiterEndIndex = content.IndexOf("\r\n");

            if (delimiterEndIndex > -1)
            {
                string delimiter = content.Substring(0, content.IndexOf("\r\n"));

                // Look for Content-Type
                Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                Match contentTypeMatch = re.Match(content);

                // Look for filename
                re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                Match filenameMatch = re.Match(content);

                // Did we find the required values?
                if (contentTypeMatch.Success && filenameMatch.Success)
                {
                    // Set properties
                    this.ContentType = contentTypeMatch.Value.Trim();
                    this.Filename = filenameMatch.Value.Trim();

                    // Get the start & end indexes of the file contents
                    int startIndex = contentTypeMatch.Index + contentTypeMatch.Length + "\r\n\r\n".Length;

                    byte[] delimiterBytes = encoding.GetBytes("\r\n" + delimiter);
                    int endIndex = IndexOf(data, delimiterBytes, startIndex);

                    int contentLength = endIndex - startIndex;

                    // Extract the file contents from the byte array
                    byte[] fileData = new byte[contentLength];

                    Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

                    this.FileContents = fileData;
                    this.Success = true;
                }
            }
        }

        private int IndexOf(byte[] searchWithin, byte[] serachFor, int startIndex)
        {
            int index = 0;
            int startPos = Array.IndexOf(searchWithin, serachFor[0], startIndex);

            if (startPos != -1)
            {
                while ((startPos + index) < searchWithin.Length)
                {
                    if (searchWithin[startPos + index] == serachFor[index])
                    {
                        index++;
                        if (index == serachFor.Length)
                        {
                            return startPos;
                        }
                    }
                    else
                    {
                        startPos = Array.IndexOf<byte>(searchWithin, serachFor[0], startPos + index);
                        if (startPos == -1)
                        {
                            return -1;
                        }
                        index = 0;
                    }
                }
            }

            return -1;
        }

        private byte[] ToByteArray(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public bool Success
        {
            get;
            private set;
        }

        public string ContentType
        {
            get;
            private set;
        }

        public string Filename
        {
            get;
            private set;
        }

        public byte[] FileContents
        {
            get;
            private set;
        }
    }
}
