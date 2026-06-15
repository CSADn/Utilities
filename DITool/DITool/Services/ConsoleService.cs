using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DITool.Services
{
    public class ConsoleService
    {
        private readonly TextBlock _consoleControl;


        public ConsoleService(TextBlock consoleControl)
        {
            _consoleControl = consoleControl;
        }


        public void Clear()
        {
            _consoleControl.Text = string.Empty;
        }

        public void Log(string message)
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _consoleControl.Text += msg;
        }

        public void LogLine(string message)
            => Log($"{message}{Environment.NewLine}");

        public void Append(string message)
            => _consoleControl.Text += $" {message}";

        public void AppendLine(string message)
            => Append($"{message}{Environment.NewLine}");

        public void LineBreak()
            => _consoleControl.Text += Environment.NewLine;
    }
}
