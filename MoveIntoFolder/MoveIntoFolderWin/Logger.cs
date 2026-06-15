using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveIntoFolderWin
{
    public enum LogLevel
    {
        Info, Warning, Error, Trace
    }

    public class Logger
    {
        private readonly string _file;


        public Logger()
        {
            _file = Path.GetDirectoryName(Path.GetTempFileName());
            _file = Path.Combine(_file, $"MoveIntoFolder_{DateTime.Now.Date:yyyyMMdd}.log");
        }


        public async Task Log(LogLevel level, string message)
        {
            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(File.Open(_file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {
                    writer.Write($"[{DateTime.Now:HH:mm:ss}] ({level}) >> ");
                    writer.WriteLine(message);

                    writer.Flush();
                    writer.Close();
                }
            });
        }

        public void LogInfo(string message) => Log(LogLevel.Info, message).Wait();
        
        public void LogWarning(string message) => Log(LogLevel.Warning, message).Wait();

        public void LogError(string message) => Log(LogLevel.Error, message).Wait();
        
        public void LogTrace(string message) => Log(LogLevel.Trace, message).Wait();

        public void LogException(Exception exception) => Log(LogLevel.Error, $"{exception}").Wait();
    }
}
