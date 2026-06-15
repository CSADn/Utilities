namespace Brightness
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class FileLogger
    {
        private static object _lock = new object();
        public static readonly string LogName = "w10_BS.log.txt";
        public static readonly string AppConfigFolderName = "Win10_BrightnessSlider";
        public static readonly string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static void Log(string msg, bool consoleWriteline = true, string caller = "", string file = "", int line = -1)
        {
            try
            {
                object obj2 = _lock;
                lock (obj2)
                {
                    string fileName = Path.GetFileName(file);
                    File.AppendAllText(LogPath, $"{DateTime.UtcNow} ==>  Caller(File,Member,Line): {fileName}, {caller}, {line} " + msg + " \r\n\r\n");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("exception @ filelogger: " + exception);
            }
            Console.WriteLine("filelogger: " + msg);
        }

        public static string LogPath =>
            Path.Combine(localAppData, AppConfigFolderName, LogName);
    }
}

