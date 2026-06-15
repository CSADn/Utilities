using FileHelpers;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileIO
{
    public class FileManager
    {
        #region Privados

        private Logger _log;

        private string _filePath { get; set; }

        private string _headerText { get; set; }

        private TextReader _fileText { get; set; }

        #endregion

        #region Constructores

        public FileManager()
        {
            _log = LogManager.GetCurrentClassLogger();
        }

        public FileManager(string path)
            : this()
        {
            this._filePath = path;
        }

        public FileManager(TextReader fileText)
            : this()
        {
            this._fileText = fileText;
        }

        #endregion
        
        #region Metodos Públicos

        public T[] Parse<T>() where T : class, new()
        {
            try
            {
                var engine = new FileHelperEngine<T>();

                var result = engine.ReadFile(this._filePath);

                this._headerText = engine.HeaderText;

                return result;
            }
            catch (ConvertException ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message + " - Linea: " + ex.LineNumber + " - Columna: " + ex.ColumnNumber);
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message);
            }
        }

        public List<T> ParseAsList<T>() where T : class, new()
        {
            try
            {
                var engine = new FileHelperEngine<T>();

                var result = engine.ReadFileAsList(this._filePath);

                this._headerText = engine.HeaderText;

                return result;
            }
            catch (ConvertException ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message + " - Linea: " + ex.LineNumber + " - Columna: " + ex.ColumnNumber);
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message);
            }
        }

        public T[] ParseStream<T>() where T : class, new()
        {
            try
            {
                var engine = new FileHelperEngine<T>();

                var result = engine.ReadStream(this._fileText, -1);

                this._headerText = engine.HeaderText;

                return result;
            }
            catch (ConvertException ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message + " - Linea: " + ex.LineNumber + " - Columna: " + ex.ColumnNumber);
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message);
            }
        }

        public List<T> ParseStreamAsList<T>() where T : class, new()
        {
            try
            {
                var engine = new FileHelperEngine<T>();

                var result = engine.ReadStreamAsList(this._fileText, -1);

                this._headerText = engine.HeaderText;

                return result;
            }
            catch (ConvertException ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message + " - Linea: " + ex.LineNumber + " - Columna: " + ex.ColumnNumber);
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());
                throw new Exception("Formato de archivo inválido." + ex.Message);
            }
        }

        public string GetHeaderText()
        {
            return this._headerText;
        }

        public string GetFileName()
        {
            return Path.GetFileName(this._filePath);
        }

        #endregion
    }
}
