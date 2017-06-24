using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.WPF;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MessageBox = System.Windows.MessageBox;

namespace Licencjat_new.CustomClasses
{
    static class ImageHelper
    {
        #region Methods
        public static BitmapImage UriToImageSource(Uri uri)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uri;
            image.EndInit();

            return image;
        }

        public static BitmapSource GetHtmlImagePreview(string html, Size minSize, Size maxSize)
        {
            return HtmlRender.RenderToImage(html, minSize, maxSize);
        }
        #endregion
    }

    static class ColorScheme
    {
        #region Variables
        public static Color MenuLight = (Color) ColorConverter.ConvertFromString("#fdfdfd");
        public static Color MenuDarker = (Color)ColorConverter.ConvertFromString("#ededed");
        public static Color MenuDark = (Color) ColorConverter.ConvertFromString("#c2c2c2");
        public static Color GlobalBlue = (Color) ColorConverter.ConvertFromString("#1c9ad2");
        public static Color GlobalWhite = (Color)ColorConverter.ConvertFromString("#ffffff");
        public static Color GlobalLightRed = (Color) ColorConverter.ConvertFromString("#ffe5e5");
        public static Color GlobalDarkRed = (Color) ColorConverter.ConvertFromString("#e50000");
        public static Color GlobalDarkText = (Color) ColorConverter.ConvertFromString("#1a2428");
        #endregion
    }

    public static class StringExtension
    {
        #region Methods
        public static String Replace(this string self,
                                          string oldString, string newString,
                                          bool firstOccurrenceOnly = false)
        {
            if (!firstOccurrenceOnly)
                return self.Replace(oldString, newString);

            int pos = self.IndexOf(oldString);
            if (pos < 0)
                return self;

            return self.Substring(0, pos) + newString
                   + self.Substring(pos + oldString.Length);
        }
        #endregion
    }

    public static class DelegateExpansion
    {
        public static object CrossInvoke(this Delegate delgt, object sender, EventArgs e)
        {
            if (delgt.Target is Control && ((Control)delgt.Target).InvokeRequired)
            {
                return ((Control)delgt.Target).Invoke(delgt, new object[] { sender, e });
            }
            return delgt.Method.Invoke(delgt.Target, new object[] { sender, e });
        }
    }

    public static class DateHelper
    {
        #region
        public static string DateToDateContraction(DateTime date)
        {
            DateTime dateNow = DateTime.Now;

            TimeSpan dateDifference = dateNow - date;

            if (dateDifference.TotalSeconds < 60)
            {
                return "Przed chwilą";
            }

            if (dateDifference.TotalMinutes <= 60)
            {
                return dateDifference.Minutes + " minut" + NumberLastLetter(dateDifference.Minutes) + " temu";
            }

            if (date.DayOfYear == dateNow.DayOfYear)
            {
                return dateDifference.Hours + " godzin" + NumberLastLetter(dateDifference.Hours) + " temu";
            }

            if (date.DayOfYear == dateNow.DayOfYear - 1)
            {
                return "Wczoraj o " + date.ToString("HH:mm");
            }

            return date.ToString("dd.MM.yyyy") + " o " + date.ToString("HH:mm");
        }

        public static string NumberLastLetter(double number)
        {
            string numberString = number.ToString();

            if (number == 1)
                return "ę";

            char almostLastNumber = '0';

            if (numberString.Length > 1)
                almostLastNumber = numberString[numberString.Count() - 2];

            char lastNumber = numberString.Last();

            if (almostLastNumber != '1' && new[]{'2', '3', '4'}.Contains(lastNumber))
            {
                return "y";
            }
            else
            {
                return "";
            }
        }
        #endregion
    }

    public static class DownloadHelper
    {
        public static MemoryStream ZipFiles(List<FileModel> files)
        {
            if (!files.Any()) return null;

            var ms = new MemoryStream();       
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var entry = archive.CreateEntry(file.Name);
                        using (var stream = entry.Open())
                        {
                            stream.Write(file.Data, 0, file.Data.Length);
                            stream.Close();
                        }
                    }
                }
                ms.Position = 0;
                return ms;
        }

        public static void DownloadFile(FileModel file)
        {
            if (file.Data.Length > 0)
            {
                using (var fs = new FileStream("C://Users/Marcin/Desktop/Zalaczniki/" + file.Name, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(file.Data, 0, file.Data.Length);
                }
            }
        }
    }

    public static class ErrorHelper
    {
        private static string _filePath = "";
        public static bool CreateErrorLogFile()
        {
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                string dir = System.IO.Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().Location);

                string filePath = dir + "/ErrorLog/" + version + ".txt";

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Version: " + version);
                    writer.WriteLine("Started: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                    writer.WriteLine("-----------------------------------------------------------------------------");
                }
                _filePath = filePath;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool LogError(Exception exception)
        {
            try
            {
                if (_filePath != "")
                {
                    using (StreamWriter writer = new StreamWriter(_filePath, true))
                    {
                        writer.WriteLine("Date: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                        writer.WriteLine("Exception: " + exception.ToString());
                        writer.WriteLine("--------------------------------------");
                    }
                    return true;
                }

                MessageBox.Show("ErrorLogFile not created");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Message: " + ex.Message + Environment.NewLine + "StackTrace: " + ex.StackTrace);
                return false;
            }
        }
    }

    public static class FileHelper
    {
        private static Dictionary<string, string> _iconDictionary = new Dictionary<string, string>()
        {
            {"application/x-zip-compressed", "zip"},
            {"application/x-7z-compressed", "zip"},
            {"application/pdf", "pdf"},
            {"video/x-msvideo", "avi"},
            {"image/jpeg", "jpg"},
            {"image/jpg", "jpg"},
            {"application/x-msdownload", "exe"},
            {"application/vnd.ms-excel", "xls"},
            {"application/vnd.ms-excel.sheet.macroenabled.12", "xls"},
            {"application/vnd.openxmlformats-officedocument.presentationml.presentation", "ppt"},
            {"application/vnd.openxmlformats-officedocument.presentationml.slideshow", "ppt"},
            {"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xls"},
            {"application/vnd.openxmlformats-officedocument.wordprocessingml.document", "doc"},
            {"application/vnd.ms-powerpoint", "ppt"},
            {"application/msword", "doc"},
            {"application/vnd.ms-word.document.macroenabled.12", "doc"},
            {"video/mp4", "mp4"},
            {"application/mp4", "mp4"},
            {"image/x-png", "png"},
            {"image/x-citrix-png", "png"},
            {"image/png", "png"},
            {"application/x-rar-compressed", "zip"},
            {"text/richtext", "rtf"},
            {"image/svg+xml", "svg"},
            {"text/plain", "txt"},
            {"application/xml", "xml"},
            {"text/css", "css"},
            {"text/csv", "csv"},
            {"image/vnd.dwg", "dwg"},
            {"application/javascript", "js"},
            {"application/json", "json"},
            {"audio/mpeg", "mp3"},
            {"application/octet-stream", "psd"},
            {"application/rtf", "rft"},
        };

        public static ImageSource GetFileIcon(ContentType contentType)
        {
            if (!_iconDictionary.ContainsKey(contentType.MediaType))
            {
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/FileTypeIcons/file.png"));
            }

            string iconFileName = _iconDictionary[contentType.MediaType] + ".png";

            ImageSource iconImage = ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/FileTypeIcons/" + iconFileName));

            iconImage.Freeze();

            return iconImage;
        }

        public static ImageSource GetFileIcon(string extension)
        {
            if (!_iconDictionary.ContainsValue(extension))
            {
                return ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/FileTypeIcons/file.png"));
            }

            string iconFileName = extension + ".png";

            ImageSource iconImage = ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/FileTypeIcons/" + iconFileName));

            iconImage.Freeze();

            return iconImage;
        }
    }
}
