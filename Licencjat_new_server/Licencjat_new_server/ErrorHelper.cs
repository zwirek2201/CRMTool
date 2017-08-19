using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
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

                if (!Directory.Exists(dir + "../../../ErrorLog"))
                    Directory.CreateDirectory(dir + "../../../ErrorLog");

                string filePath = dir + "../../../ErrorLog/" + version + ".txt";

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

                Console.WriteLine("ErrorLogFile not created");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message: " + ex.Message + Environment.NewLine + "StackTrace: " + ex.StackTrace);
                return false;
            }
        }
    }
}
