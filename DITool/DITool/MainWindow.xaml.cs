using DITool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using WindowsForms = System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using DITool.Helpers;
using System.Xml.XPath;
using System.IO;
using System.Text.RegularExpressions;
using DITool.Models;
using System.ComponentModel;
using System.Threading;
using System.Reflection;
using DITool.Enums;
using System.Runtime.Remoting.Channels;

namespace DITool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConsoleService _consoleService;

        private Dictionary<int, string> _conversionTypes;
        private bool _disabledUI;
        private bool _conversionInProgress;
        private BackgroundWorker _conversion;

        public Observable<string> SourcePath;


        public MainWindow()
        {
            InitializeComponent();
            InitializeDatasets();

            _consoleService = new ConsoleService(tbConsole);
            _disabledUI = false;
            _conversionInProgress = false;
            _conversion = new BackgroundWorker();

            Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BuildControls();
            BindControlEvents();

            _consoleService.Clear();
            _consoleService.LogLine("Tool started.");
            _consoleService.LogLine($"Conversion Mode: '{cbConversionType.Text}'");
        }


        private void InitializeDatasets()
        {
            _conversionTypes = new Dictionary<int, string>
            {
                { 1, "Convert Folder" },
                { 2, "Convert Single File" }
            };

            SourcePath = new Observable<string>("...");
        }

        private void BuildControls()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            Title += $" v{version.Major}.{version.Minor}";

            _conversion.WorkerReportsProgress = true;
            _conversion.WorkerSupportsCancellation = true;

            cbConversionType.Items.Clear();
            cbConversionType.ItemsSource = _conversionTypes;
            cbConversionType.SelectedValuePath = "Key";
            cbConversionType.DisplayMemberPath = "Value";
            cbConversionType.SelectedIndex = 0;

            tbPath.DataContext = SourcePath;
            tbPath.SetBinding(TextBox.TextProperty, "Value");

            btStop.Visibility = Visibility.Hidden;

            pbProgress.Value = 0;
            pbProgress.Minimum = 0;
            pbProgress.Maximum = 100;
        }

        private void BindControlEvents()
        {
            cbConversionType.SelectionChanged += (s, e) =>
            {
                var item = e.AddedItems
                    .Cast<KeyValuePair<int, string>>()
                    .First();

                SourcePath.Value = "...";
                _consoleService.LogLine($"Conversion Mode: '{item.Value}'");
                CheckUI();
            };

            btBrowse.Click += (s, e) =>
            {
                _consoleService.Log("Browse");

                var path = "...";

                switch (cbConversionType.SelectedValue)
                {
                    case 1:
                        _consoleService.AppendLine("'Folder'...");
                        path = BrowseFolder();
                        break;
                    case 2:
                        _consoleService.AppendLine("'File'...");
                        path = BrowseFile();
                        break;
                }

                SourcePath.Value = path;

                if (!string.IsNullOrWhiteSpace(path) && !path.Equals("..."))
                    CheckPath(path);

                CheckUI();
            };

            chExtBin.Checked += (s, e) => CheckUI();
            chExtBin.Unchecked += (s, e) => CheckUI();
            chExtDmp.Checked += (s, e) => CheckUI();
            chExtDmp.Unchecked += (s, e) => CheckUI();
            chExtMct.Checked += (s, e) => CheckUI();
            chExtMct.Unchecked += (s, e) => CheckUI();
            chExtMfd.Checked += (s, e) => CheckUI();
            chExtMfd.Unchecked += (s, e) => CheckUI();

            btConvert.Click += (s, e) => _conversion.RunWorkerAsync(SourcePath.Value);
            btStop.Click += (s, e) =>
            {
                if (_conversion.IsBusy)
                {
                    btStop.IsEnabled = false;
                    _conversion.CancelAsync();
                    _consoleService.LogLine("Cancellation requested...");
                }
            };

            btExportLog.Click += (s, e)
                => ExportLog();

            btClearConsole.Click += (s, e) =>
            {
                _consoleService.Clear();
                _consoleService.LogLine("Console cleared.");
            };

            btAbout.Click += (s, e) =>
            {
                About.Visibility = Visibility.Visible;
                About.AboutFadeIn();
            };

            btExit.Click += (s, e) =>
            {
                if (_conversionInProgress)
                {
                    var msg = "Convesion in progress\nSure?";
                    var result = MessageBox.Show(msg, this.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                Application.Current.Shutdown();
            };

            svConsole.ScrollChanged += (s, e) =>
            {
                var control = (ScrollViewer)s;

                if (e.ExtentHeightChange > 0)
                    control.ScrollToEnd();
            };

            About.OnFadeOutCompleted += (s)
                => About.Visibility = Visibility.Hidden;

            _conversion.DoWork += Convert;
            _conversion.ProgressChanged += ConversionProgress;
            _conversion.RunWorkerCompleted += ConversionDone;
        }


        private string BrowseFolder()
        {
            var dialog = new FolderBrowserControl.FolderBrowserDialog();
            dialog.Title = "Folder lookup...";
            dialog.AllowMultiSelect = false;

            var result = dialog.ShowDialog();

            if (result != WindowsForms.DialogResult.OK)
            {
                _consoleService.LogLine("Selection cancelled.");
                return "...";
            }

            _consoleService.LogLine($"Folder selected: '{dialog.SelectedFolder}'");
            return dialog.SelectedFolder;
        }

        private string BrowseFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Binary dump lookup...",
                DefaultExt = ".bin",
                RestoreDirectory = true,
                CheckFileExists = true,
                Filter = "Binary Dumps|*.bin;*.mfd;*.dmp"
            };

            var result = dialog.ShowDialog();

            if (!result.HasValue || result.Value != true)
            {
                _consoleService.LogLine("Selection cancelled.");
                return "...";
            }

            _consoleService.LogLine($"File selected: '{dialog.FileName}'");
            return dialog.FileName;
        }

        private void ExportLog()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export to...",
                DefaultExt = ".log",
                RestoreDirectory = true,
                CheckFileExists = false,
                OverwritePrompt= true,
                Filter = "Console Log|*.log"
            };

            var result = dialog.ShowDialog();

            if (!result.HasValue || result.Value != true)
            {
                _consoleService.LogLine("Export log cancelled.");
                return;
            }

            File.WriteAllText(dialog.FileName, tbConsole.Text);
            _consoleService.LogLine($"Console log exported to: '{dialog.FileName}'");
        }


        private void CheckPath(string path)
        {
            if (IsDirectory(path))
            {
                var files = Directory.GetFiles(path, "*.bin", SearchOption.AllDirectories);
                _consoleService.LogLine($"Binaries files found: '{files.Length}'");

                var n = 0;

                foreach (var file in files)
                {
                    _consoleService.LogLine($"Checking '..{file.Replace(path, "")}': ");

                    ValidateFormat(file, out string result);

                    LogOperationResult(result);

                    if (result != null)
                    {
                        n++;
                        continue;
                    };
                }

                if (n > 0)
                {
                    _consoleService.LineBreak();
                    _consoleService.LogLine($"Invalid files: ({n}/{files.Length})");
                }
            }
            else
            {
                _consoleService.LogLine($"Checking '..\\{Path.GetFileName(path)}': ");
                ValidateFormat(path, out string result);
                LogOperationResult(result);
            }
        }

        private void CheckUI()
        {
            btConvert.IsEnabled = false;
            btStop.Visibility = Visibility.Hidden;
            btStop.IsEnabled = true;

            if (string.IsNullOrWhiteSpace(tbPath.Text) || tbPath.Text.Equals("..."))
                return;

            var anyExt = chExtBin.IsChecked | chExtDmp.IsChecked | chExtMct.IsChecked | chExtMfd.IsChecked;

            if (anyExt == null || anyExt.Value == false)
                return;

            btConvert.IsEnabled = true;
        }

        private MF1ICS20 ValidateFormat(string path, out string error)
        {
            var pattern = new Regex("^04[0-9a-f]{12}$", RegexOptions.IgnoreCase);
            var info = new FileInfo(path);

            // Extension
            if (Path.GetExtension(info.Name).ToLower() != ".bin")
            {
                error = $"Not supported extension ({Path.GetExtension(info.Name)})";
                return null;
            }

            // Size
            if (info.Length != 320)
            {
                error = $"Incorrect size ({info.Length})";
                return null;
            }

            // UiD
            var data = new MF1ICS20(File.ReadAllBytes(path));

            if (!pattern.IsMatch(data.UID))
            {
                error = $"Invalid UID ({data.UID})";
                return null;
            }

            // Sector 0 Access
            var access = data.GetSector(MF1ICS20Sectors.Sector0).GetAccess();
            if (access != "17878e00")
            {
                error = $"Sector 0 invalid access ({access})";
                return null;
            }
            // Sector 1 Access
            access = data.GetSector(MF1ICS20Sectors.Sector1).GetAccess();
            if (access != "77878800")
            {
                error = $"Sector 1 invalid access ({access})";
                return null;
            }
            // Sector 2 Access
            access = data.GetSector(MF1ICS20Sectors.Sector2).GetAccess();
            if (access != "77878800")
            {
                error = $"Sector 2 invalid access ({access})";
                return null;
            }
            // Sector 3 Access
            access = data.GetSector(MF1ICS20Sectors.Sector3).GetAccess();
            if (access != "77878800")
            {
                error = $"Sector 3 invalid access ({access})";
                return null;
            }
            // Sector 4 Access
            access = data.GetSector(MF1ICS20Sectors.Sector4).GetAccess();
            if (access != "77878800")
            {
                error = $"Sector 4 invalid access ({access})";
                return null;
            }

            error = null;
            return data;
        }

        private void DisableUI(bool flag)
        {
            _disabledUI = flag;
            gbConversion.IsEnabled = !flag;
        }


        private void Convert(object s, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)s;
            _conversionInProgress = true;

            Dispatcher.Invoke(() =>
            {
                _consoleService.LogLine("Conversion started...");
                btConvert.IsEnabled = false;
                btStop.Visibility = Visibility.Visible;
                DisableUI(true);
            });

            var path = (e.Argument as string);
            var files = new List<string>();

            if (string.IsNullOrEmpty(path) || path.Equals("..."))
            {
                e.Cancel = true;
                worker.ReportProgress(100);
                Dispatcher.Invoke(() => _consoleService.LogLine("No path selected, conversion terminated."));
                return;
            }

            if (IsDirectory(path))
            {
                files.AddRange(Directory.GetFiles(path, "*.bin", SearchOption.AllDirectories));
                Dispatcher.Invoke(() => _consoleService.LogLine($"Binaries files found: '{files.Count}'"));
            }
            else
                files.Add(path);

            var total = files.Count();
            var n = 1;

            foreach (var file in files)
            {
                Dispatcher.Invoke(() => _consoleService.LogLine($"Checking '{file}'..."));

                var data = ValidateFormat(file, out string result);
                LogOperationResult(result);

                if (result != null)
                {
                    e.Cancel = true;
                    worker.ReportProgress(100);
                    break;
                }

                Dispatcher.Invoke(() => _consoleService.LogLine($"Converting ({n++}/{total})..."));

                var rootPath = Path.GetDirectoryName(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var convertedPath = Path.Combine(rootPath, $"{fileName}_converted");

                if (Dispatcher.Invoke(() => chComputeKeyA.IsChecked ?? false))
                {
                    data.ComputeKeys();
                    Dispatcher.Invoke(() => _consoleService.LogLine($"  =>   Computed KeyA [{data.KeyA.ToUpper()}]"));
                }

                if (Dispatcher.Invoke(() => chExtBin.IsChecked ?? false))
                {
                    data.ToBIN(convertedPath);
                    Dispatcher.Invoke(() => _consoleService.LogLine("  =>   To 'BIN' [OK]"));
                }

                if (Dispatcher.Invoke(() => chExtMfd.IsChecked ?? false))
                {
                    data.ToMFD(convertedPath);
                    Dispatcher.Invoke(() => _consoleService.LogLine("  =>   To 'MFD' [OK]"));
                }

                if (Dispatcher.Invoke(() => chExtDmp.IsChecked ?? false))
                {
                    data.ToDMP(convertedPath);
                    Dispatcher.Invoke(() => _consoleService.LogLine("  =>   To 'DMP' [OK]"));
                }

                if (Dispatcher.Invoke(() => chExtMct.IsChecked ?? false))
                {
                    data.ToMCT(convertedPath);
                    Dispatcher.Invoke(() => _consoleService.LogLine("  =>   To 'MCT' [OK]"));
                }

                if (Dispatcher.Invoke(() => chGenerateKeyFile.IsChecked ?? false))
                {
                    data.GenerateKeysFile(Path.Combine(rootPath, $"{fileName}_keys"));
                    Dispatcher.Invoke(() => _consoleService.LogLine("  =>   To 'nfc-tools keys' [OK]"));
                }

                var parcial = (n / 100) * total;
                worker.ReportProgress(parcial);

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    worker.ReportProgress(100);
                    break;
                }
            }
        }

        private void ConversionProgress(object s, ProgressChangedEventArgs e)
        {
            var worker = (BackgroundWorker)s;

            Dispatcher.Invoke(() =>
            {
                pbProgress.Value = e.ProgressPercentage;
            });
        }

        private void ConversionDone(object s, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)s;
            _conversionInProgress = false;

            Dispatcher.Invoke(() =>
            {
                var msg = e.Cancelled ? "Conversion has been cancelled." : "Conversion done.";
                _consoleService.LogLine(msg);
                btConvert.IsEnabled = true;
                btStop.Visibility = Visibility.Hidden;
                pbProgress.Value = 0;

                DisableUI(false);
                CheckUI();
            });
        }


        private bool IsDirectory(string path)
        {
            var pathAttrs = File.GetAttributes(path);
            return pathAttrs.HasFlag(FileAttributes.Directory);
        }

        private void LogOperationResult(string operationResult)
        {
            if (!string.IsNullOrWhiteSpace(operationResult))
            {
                Dispatcher.Invoke(() => _consoleService.LogLine($"  =>  [ERROR]:{operationResult}]"));
                return;
            };

            Dispatcher.Invoke(() => _consoleService.LogLine($"  =>  [OK]"));
        }
    }
}
