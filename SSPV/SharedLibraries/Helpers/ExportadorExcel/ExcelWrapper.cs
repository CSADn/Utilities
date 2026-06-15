using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Helpers
{
    public class ExcelWrapper
    {
        #region Privados

        XSSFWorkbook _workbook = null;

        #endregion
        
        #region Constructor

        public ExcelWrapper(Stream stream = null)
        {
            if (stream != null)
                _workbook = new XSSFWorkbook(stream);
        }

        #endregion

        #region Metodos publicos

        /// <summary>
        /// Agregar una lista en una hoja
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sheetName"></param>
        /// <param name="elements"></param>
        public void AddListToBook<T>(List<T> elements, string sheetName = null) where T : class
        {
            if (_workbook == null)
                _workbook = elements.ToWorkbook(sheetName);
            else
                _workbook = elements.ToWorkbook(_workbook, sheetName);
        }

        /// <summary>
        /// Obrtener los bytes del libro
        /// </summary>
        /// <returns></returns>
        public byte[] GetExcel()
        {
            if (_workbook == null)
                throw new NullReferenceException("Debe agregar elementos al libro");
                        
            return _workbook.ToExcel();
        } 

        /// <summary>
        /// Crear el libro desde el array de bytes
        /// </summary>
        /// <param name="bytes"></param>
        public void AddBytesToWorkbook(byte[] bytes)
        {
            _workbook = bytes.ToWorkbook();
        }

        public List<T> GetListFromSheet<T>(string sheetName = null) where T: class, new()
        {
            if (_workbook == null)
                throw new NullReferenceException("Debe agregar elementos al libro");

            return _workbook.SheetToList<T>(sheetName);
        }

        #endregion
    }
}
