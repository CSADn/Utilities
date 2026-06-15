using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MoveIntoFolderWin
{
    static class Program
    {
        private static readonly Logger _logger;
        private static readonly Regex _rxLang;


        static Program()
        {
            _logger = new Logger();
            _rxLang = new Regex(@"(\.\w{2}(\w)?)$", RegexOptions.Compiled);
        }


        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _logger.LogInfo($"Args: [{string.Join(" ", Environment.GetCommandLineArgs())}]");

            if (args.Length < 1)
                return;

            var app = $"\"{Environment.GetCommandLineArgs().First()}\"";
            var cmd = Environment.CommandLine.Replace(app, string.Empty).Trim();

            DoMagic(cmd);
        }

        private static void DoMagic(string fullPath)
        {
            _logger.LogInfo($"DoMagic({fullPath})");

            if (string.IsNullOrEmpty(fullPath))
                return;

            if (!File.Exists(fullPath))
            {
                _logger.LogError("File not found.");
                return;
            }

            var path = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);
            var fileNoExt = Path.GetFileNameWithoutExtension(fullPath);
            var folderName = fileNoExt;

            if (_rxLang.IsMatch(fileNoExt))
                folderName = _rxLang.Replace(fileNoExt, string.Empty);

            var targetPath = path + "\\" + folderName;

            var from = string.Empty;
            var to = string.Empty;

            _logger.LogInfo($"  -path: [{path}]");
            _logger.LogInfo($"  -fileName: [{fileName}]");
            _logger.LogInfo($"  -fileNoExt: [{fileNoExt}]");
            _logger.LogInfo($"  -folderName: [{folderName}]");
            _logger.LogInfo($"  -targetPath: [{targetPath}]");


            try
            {
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                var files = Directory.GetFiles(path).Where(w => Path.GetFileName(w).StartsWith(folderName));

                if (!files.Any())
                {
                    _logger.LogInfo("No files found.");
                    return;
                }

                foreach (var file in files)
                {
                    from = $"{file}";
                    to = $"{targetPath}\\{Path.GetFileName(file)}";

                    _logger.LogInfo($"  -from: [{from}]");
                    _logger.LogInfo($"  -to: [{to}]");

                    File.Move(from, to);

                    _logger.LogInfo("File moved.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);

                MessageBox.Show(
                    ex.Message + "\n" +
                    from + "\n" +
                    to,
                    "MoveIntoFolder Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            _logger.LogInfo(new String('-', 160));
        }
    }
}
