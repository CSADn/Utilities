using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.AccessControl;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Core;
using System.Linq;
using System.Text.RegularExpressions;

namespace SQLScheduleBKP
{
    class Program
    {
        private static string m_logDir;
        private static string m_backupDir;
        private static string m_connstring;
        private static bool m_stampFiles;
        private static bool m_deleteOldBackups;
        private static int m_deleteAfter;
        private static int m_waitBeforeClose;
        private static bool m_zipBackup;
        private static List<string> m_databasesToBackup;
        private static List<string> m_zipFolders;
        private static int m_compressionLevel;
        private static int m_bufferSize;
        private static int m_cmdTimeout;

        static void Main(string[] args)
        {
            Initialise();

            Console.WriteLine( "Federico Pisarello Copyright 2017 - fpisarello@gmail.com\n" );
            Console.WriteLine( "SQL Server Backup is starting...\n" );

            Output( "Starting SQL Database Backup...\n");


            var iSomeError = 0;
             foreach(var dataBase in m_databasesToBackup)
            {
                Output($"Starting SQL Database Backup for {dataBase}...");
                bool success = DoBackups(dataBase, m_backupDir, m_stampFiles);
                if (!success)
                    iSomeError++;
            }

            if (iSomeError == 0)
                Output("Backup of SQL Server Databases run with no errors.\n");
            else
                Output("Backup of SQL Server Databases run with errors. See Log File.\n");

            if (m_deleteOldBackups)
            {
                Output("Delete Olds Backup from Directory.\n");
                DeleteOldBackups();
            }

            if (m_zipBackup)
            {
                Output("Make a Zips File for All Backups Files.\n");
                MakeZIPFiles();
            }

            Console.WriteLine( "" );

            var counter = m_waitBeforeClose;

            while (counter > 0)
            {
                Thread.Sleep(1000); // Sleep to allow for 1 second timer ticks

                Console.WriteLine("The application will close in {0} seconds.", counter);

                Console.CursorLeft = 0;

                Console.CursorTop = Console.CursorTop - 1;

                counter--;
            }
        }


        private static void Initialise()
        {
            m_connstring = "DBConnectionString".FromAppSettings<string>(notFoundException: true);
            m_backupDir = "SQLBackupLocation".FromAppSettings<string>(notFoundException: true);
            m_logDir = "LoggingPath".FromAppSettings<string>(notFoundException: true);
            m_stampFiles = "DateStampBackupFiles".FromAppSettings(true);
            m_deleteOldBackups = "DeleteOldBackups".FromAppSettings(false);
            m_deleteAfter = "DeleteBackupsAfterDays".FromAppSettings(30);
            m_waitBeforeClose = "ConsoleWaitBeforeCloseSeconds".FromAppSettings(60);
            m_databasesToBackup = "DatabaseToBackup".FromAppSettings<List<string>>(notFoundException: true);
            m_zipBackup = "MakeZipFile".FromAppSettings(false);
            m_zipFolders = "FoldersToZip".FromAppSettings<List<string>>(notFoundException: true);
            m_compressionLevel = "CompressionLevel".FromAppSettings(3);
            m_bufferSize = "BufferSize".FromAppSettings(4) * 1048576;
            m_cmdTimeout = "CMDTimeout".FromAppSettings(5) * 60;

            if (!Directory.Exists(m_backupDir))
                Directory.CreateDirectory(m_backupDir);

            if (!Directory.Exists(m_logDir))
                Directory.CreateDirectory(m_logDir);

            LogSettings();
        }


        /// <summary>
        /// Backs up non system SQL Server databases to the configured directory.
        /// </summary>
        /// <param name="backupDir"></param>
        /// <param name="dateStamp"></param>
        private static bool DoBackups(string dataBaseName, string backupDir, bool dateStamp)
        {
            var allBackupsSuccessful = false;

            var sb = new StringBuilder();

            // Build the TSQL statement to run against your databases.
            // SQL is coded inline for portability, and to allow the dynamic
            // appending of datestrings to file names where configured.

            sb.AppendLine($@"DECLARE @name VARCHAR(50) -- database name  ");
            sb.AppendLine($@"DECLARE @path VARCHAR(256) -- path for backup files  ");
            sb.AppendLine($@"DECLARE @fileName VARCHAR(256) -- filename for backup ");
            sb.AppendLine($@"DECLARE @fileDate VARCHAR(20) -- used for file name ");
            sb.AppendLine($@"SET @path = '{backupDir}'  ");
            sb.AppendLine($@"SELECT @fileDate = CONVERT(VARCHAR(20),GETDATE(),112) ");
            sb.AppendLine($@"DECLARE db_cursor CURSOR FOR  ");
            sb.AppendLine($@"SELECT name ");
            sb.AppendLine($@"FROM master.dbo.sysdatabases ");
            sb.AppendLine($@"WHERE name NOT IN ('master','model','msdb','tempdb') AND name IN ('{dataBaseName}')");
            sb.AppendLine($@"OPEN db_cursor   ");
            sb.AppendLine($@"FETCH NEXT FROM db_cursor INTO @name   ");
            sb.AppendLine($@"WHILE @@FETCH_STATUS = 0   ");
            sb.AppendLine($@"BEGIN   ");

            if (dateStamp)
                sb.AppendLine($@"SET @fileName = @path + @name + '_' + @fileDate + '.BAK'  ");
            else
                sb.AppendLine($@"SET @fileName = @path + @name + '.BAK'  ");

            sb.AppendLine($@"BACKUP DATABASE @name TO DISK = @fileName  ");
            sb.AppendLine($@"FETCH NEXT FROM db_cursor INTO @name   ");
            sb.AppendLine($@"END   ");
            sb.AppendLine($@"CLOSE db_cursor   ");
            sb.AppendLine($@"DEALLOCATE db_cursor; ");

            var conn = new SqlConnection(m_connstring);
            var command = new SqlCommand(sb.ToString(), conn);

            try
            {
                conn.Open();

                command.CommandTimeout = m_cmdTimeout;
                command.ExecuteNonQuery();

                allBackupsSuccessful = true;
            }
            catch (Exception ex)
            {
                Output("\nAn error occurred while running the backup query: See Log File to view de Error", ex.Message);
            }
            finally
            {
                try
                {
                    conn.Close();
                }
                catch (Exception ex)
                {
                    Output("\nAn error occurred while trying to close the database connection: See Log File to view the Error", ex.Message);
                }
            }

            return allBackupsSuccessful;
        }

        /// <summary>
        /// Delete back up files in configured directory older than configured days.
        /// </summary>
        private static void DeleteOldBackups()
        {
            var dirFiles = new DirectoryInfo(m_backupDir);

            var fileInfoArr = default(FileInfo[]);

            if (m_zipBackup)
                fileInfoArr = dirFiles.GetFiles("*.ZIP", SearchOption.AllDirectories);
            else
                fileInfoArr = dirFiles.GetFiles("*.BAK", SearchOption.AllDirectories);

            foreach (var fileInfo in fileInfoArr)
            {
                var fileIsOldBackUp = CheckIfFileIsOldBackup(fileInfo.FullName);

                if (fileIsOldBackUp)
                {
                    File.Delete(fileInfo.FullName);
                    Output($"Deleting old backup file: {fileInfo.Name}");
                }
            }
        }

        /// <summary>
        /// Parses file name and returns true if file is older than configured days.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool CheckIfFileIsOldBackup(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            fileName = fileInfo.Name; // Get the file name without the full path

            var backupIsOld = false;

            var fileNameCharsArray = fileName.ToCharArray();

            var dateString = string.Empty;

            var sb = new StringBuilder();

            for (var i = fileNameCharsArray.Length - 1; i >= 0; i--)
            {
                if (sb.Length == 8)
                    break;
                if (char.IsNumber(fileNameCharsArray[i]))
                    sb.Append(fileNameCharsArray[i]);
            }

            var sb1 = new StringBuilder();
            for (int i = sb.Length - 1; i >= 0; i--)
                sb1.Append(sb[i]);

            dateString = sb1.ToString();

            if (!string.IsNullOrEmpty(dateString))
            {
                // Delete only if we have exactly 8 digits
                if (dateString.Length == 8)
                {
                    var year = String.Empty;
                    var month = String.Empty;
                    var day = String.Empty;

                    year = dateString.Substring(0, 4);
                    month = dateString.Substring(4, 2);
                    day = dateString.Substring(6, 2);

                    var backupDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

                    var backupConsideredOldAfterDays = m_deleteAfter;

                    // Compare backup date to test if this backup
                    // should be treated as an old backup.
                    var backupAge = DateTime.Now.Subtract(backupDate);

                    if (backupAge.Days > backupConsideredOldAfterDays)
                        backupIsOld = true;
                }
            }

            return backupIsOld;
        }

        /// <summary>
        /// Prints message to the console window and logs the same information in the log file
        /// </summary>
        /// <param name="message"></param>
        private static void Output(string message, string fileMessage = null)
        {
            Console.WriteLine($"{LogWriter.GetLogFileEntryDateString(DateTime.Now)} {message}");
            LogWriter.WriteLogToTextFile(m_logDir, (fileMessage.IsNull() ? message : fileMessage));
        }

        private static void MakeZIPFiles()
        {
            try
            {
                var dirBak = new DirectoryInfo(m_backupDir);
                var bakFile = dirBak.GetFiles("*.BAK", SearchOption.AllDirectories);

                foreach (var bkpFile in bakFile)
                {
                    Output($"Starting Ziping Files and Folders for {bkpFile.Name}");

                    var dbName = bkpFile.Name.Split('_').First();
                    var path = Path.GetDirectoryName(bkpFile.FullName);
                    var fileName = Path.GetFileNameWithoutExtension(bkpFile.FullName);
                    var zipFileName = Path.Combine(path, $"{fileName}.ZIP");

                    using (var zip = new ZipOutputStream(File.Create(zipFileName)))
                    {
                        zip.SetLevel(m_compressionLevel);
                        zip.UseZip64 = UseZip64.Off;

                        foreach (var folder in m_zipFolders)
                            CompressFolder(folder, zip, 0);

                        using (var file = File.OpenRead(bkpFile.FullName))
                        {
                            var buffer = new byte[m_bufferSize];

                            var entry = new ZipEntry(ZipEntry.CleanName(bkpFile.Name))
                            {
                                DateTime = DateTime.Now,
                                Comment = $"Backup SQL File ({dbName}) ({DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")})",
                                ZipFileIndex = 1,
                                Size = file.Length,
                                //Crc = CalculateFileCrc(file, buffer.Length) // No se necesita calcular a mano.
                            };

                            zip.PutNextEntry(entry);

                            var bytesReaded = file.Read(buffer, 0, buffer.Length);
                            while (bytesReaded > 0)
                            {
                                zip.Write(buffer, 0, bytesReaded);
                                bytesReaded = file.Read(buffer, 0, buffer.Length);
                            }

                            zip.Finish();
                            zip.Close();
                            file.Close();
                        }
                    }

                    bkpFile.Delete();
                }
            }
            catch (Exception ex)
            {
                Output("\nAn error occurred while trying to Zip Files: See Log File to view de Error", ex.Message);
            }
        }

        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            var files = Directory.GetFiles(path);

            foreach (var filename in files)
            {

                var fi = new FileInfo(filename);

                var entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                var newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                var buffer = new byte[4096];
                using (var streamReader = File.OpenRead(filename))
                    StreamUtils.Copy(streamReader, zipStream, buffer);

                zipStream.CloseEntry();
            }

            var folders = Directory.GetDirectories(path);

            foreach (var folder in folders)
                CompressFolder(folder, zipStream, folderOffset);
        }

        private static long CalculateFileCrc(FileStream fileStream, long bufferLength = 1048576 * 4)
        {
            if (!fileStream.CanRead)
                throw new Exception("File stream cannot be readed for CRC calculation.");

            if (fileStream.CanSeek)
                fileStream.Seek(0, SeekOrigin.Begin);

            var crc = new Crc32();
            crc.Reset();

            var bytesReaded = 0;
            var buffer = new byte[bufferLength];

            bytesReaded = fileStream.Read(buffer, 0, buffer.Length);

            while (bytesReaded > 0)
            {
                crc.Update(buffer, 0, bytesReaded);
                bytesReaded = fileStream.Read(buffer, 0, buffer.Length);
            }

            if (fileStream.CanSeek)
                fileStream.Seek(0, SeekOrigin.Begin);

            return crc.Value;
        }

        private static void LogSettings()
        {
            var r = Regex.Match(m_connstring, "Password=(.*);", RegexOptions.IgnoreCase);
            var conn = m_connstring.Substring(0, r.Groups[1].Index);
            conn += "****" + m_connstring.Substring(r.Groups[1].Index + r.Groups[1].Length);

            var log = "\r\n\t{ 'Settings': [\r\n";
            log += $"\t\t{{ 'DBConnectionString': '{conn}' }},\r\n";
            log += $"\t\t{{ 'SQLBackupLocation': '{m_backupDir}' }},\r\n";
            log += $"\t\t{{ 'LoggingPath': '{m_logDir}' }},\r\n";
            log += $"\t\t{{ 'DateStampBackupFiles': '{m_stampFiles}' }},\r\n";
            log += $"\t\t{{ 'DeleteOldBackups': '{m_deleteOldBackups}' }},\r\n";
            log += $"\t\t{{ 'DeleteBackupsAfterDays': '{m_deleteAfter}' }},\r\n";
            log += $"\t\t{{ 'ConsoleWaitBeforeCloseSeconds': '{m_waitBeforeClose}' }},\r\n";
            log += $"\t\t{{ 'DatabaseToBackup': '{string.Join(";", m_databasesToBackup.ToArray())}' }},\r\n";
            log += $"\t\t{{ 'MakeZipFile': '{m_zipBackup}' }},\r\n";
            log += $"\t\t{{ 'FoldersToZip': '{string.Join(";", m_zipFolders.ToArray())}' }},\r\n";
            log += $"\t\t{{ 'CompressionLevel': '{m_compressionLevel}' }},\r\n";
            log += $"\t\t{{ 'BufferSize': '{m_bufferSize}' }},\r\n";
            log += $"\t\t{{ 'CMDTimeout': '{m_cmdTimeout}' }},\r\n";
            log += "\t]}";

            Output(log);
        }
    }
}
