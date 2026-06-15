using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Helpers
{
    public static class IEnumerableExtensions
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        #region Métodos Privados

        /// <summary>
        /// Crear un Workbook de excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private static XSSFWorkbook CreateExcel<T>(IEnumerable<T> enumerable, string sheetName) where T : class
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            
            return AddToWorkBook(enumerable, workbook, sheetName);
        }

        /// <summary>
        /// Agregar un Enumerable al libro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        private static XSSFWorkbook AddToWorkBook<T>(IEnumerable<T> enumerable, XSSFWorkbook workbook, string sheetName) where T : class
        {
            SheetAttribute sheetAttrib = typeof(T).GetCustomAttribute<SheetAttribute>();
            string sheetname = sheetName ?? sheetAttrib.SheetName;

            ISheet sheet = workbook.CreateSheet(sheetname);
            IDataFormat dataFormat = workbook.CreateDataFormat();

            Type t = typeof(T);
            if (t != null)
            {
                //Armo header
                PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (pi != null)
                {
                    Dictionary<PropertyInfo, HeaderCustomAttribute> dicAttributes = new Dictionary<PropertyInfo, HeaderCustomAttribute>();
                    Dictionary<int, ICellStyle> dicStyles = new Dictionary<int, ICellStyle>();
                    Dictionary<int, Action<object, ICell>> dicConvert = new Dictionary<int, Action<object, ICell>>();

                    int rowCount = 0;

                    IRow row = sheet.CreateRow(rowCount++);
                    ICellStyle HeadercellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();

                    font.Boldweight = (short)FontBoldWeight.Bold;
                    font.Color = HSSFColor.White.Index;
                    font.FontHeightInPoints = 11;

                    IColor color = new HSSFColor.BlueGrey();
                    HeadercellStyle.FillForegroundColor = HSSFColor.Grey50Percent.Index;
                    HeadercellStyle.BorderBottom = BorderStyle.Medium;
                    HeadercellStyle.FillPattern = FillPattern.SolidForeground;
                    HeadercellStyle.SetFont(font);

                    //Inicializacion de header y celdas.
                    for (int i = 0; i < pi.Count(); i++)
                    {
                        //HeaderCustomAttribute[] attributes = (HeaderCustomAttribute[])pi[i].GetCustomAttributes(typeof(HeaderCustomAttribute), false);
                        HeaderCustomAttribute attrib = pi[i].GetCustomAttribute<HeaderCustomAttribute>();
                        if (attrib != null)
                        {
                            //HeaderCustomAttribute attrib = attributes[0];
                            dicAttributes.Add(pi[i], attrib);

                            ICell cell = row.CreateCell(attrib.Index, CellType.String);
                            cell.CellStyle = HeadercellStyle;
                            cell.SetCellValue(attrib.Header);

                            ICellStyle cellStyle = workbook.CreateCellStyle();

                            if (!string.IsNullOrWhiteSpace(attrib.Format))
                            {
                                short format = HSSFDataFormat.GetBuiltinFormat(attrib.Format);
                                if(format == -1)
                                   format = dataFormat.GetFormat(attrib.Format);
                                cellStyle.DataFormat = format;
                            }
                            else
                                cellStyle.DataFormat = -1;

                            dicStyles.Add(attrib.Index, cellStyle);

                            switch (Type.GetTypeCode(pi[i].PropertyType))
                            {
                                case TypeCode.Empty:
                                case TypeCode.Object:
                                case TypeCode.DBNull:
                                    throw new NotSupportedException();
                                case TypeCode.Boolean:
                                    dicConvert.Add(attrib.Index, (s, c) => c.SetCellValue(Convert.ToBoolean(s)));
                                    break;
                                case TypeCode.SByte:
                                case TypeCode.Byte:
                                case TypeCode.Int16:
                                case TypeCode.UInt16:
                                case TypeCode.Int32:
                                case TypeCode.UInt32:
                                case TypeCode.Int64:
                                case TypeCode.UInt64:
                                case TypeCode.Single:
                                case TypeCode.Double:
                                case TypeCode.Decimal:
                                    dicConvert.Add(attrib.Index, (s, c) => c.SetCellValue(Convert.ToDouble(s)));
                                    break;
                                case TypeCode.DateTime:
                                    dicConvert.Add(attrib.Index, (s, c) => c.SetCellValue(Convert.ToDateTime(s)));
                                    break;
                                case TypeCode.Char:
                                case TypeCode.String:
                                    dicConvert.Add(attrib.Index, (s, c) => c.SetCellValue(Convert.ToString(s)));
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                    }

                    List<T> lista = enumerable.ToList();
                    for (int i = 0; i < lista.Count; i++)
                    {
                        T item = lista[i] as T;
                        row = sheet.CreateRow(rowCount++);
                        for (int j = 0; j < pi.Count(); j++)
                        {
                            PropertyInfo prop = pi[j];
                            object value = prop.GetValue(item, null);
                            HeaderCustomAttribute attrib = dicAttributes[prop];
                            ICell cell = row.CreateCell(attrib.Index, (CellType)attrib.CellType);

                            dicConvert[attrib.Index].Invoke(value, cell);
                            cell.CellStyle = dicStyles[attrib.Index];
                        }
                    }

                    if (dicAttributes.Values.Any(a => !string.IsNullOrWhiteSpace(a.Formula)))
                    {
                        row = sheet.CreateRow(rowCount);

                        for (int j = 0; j < pi.Count(); j++)
                        {
                            PropertyInfo prop = pi[j];
                            HeaderCustomAttribute attrib = dicAttributes[prop];
                            ICellStyle cellStyle = workbook.CreateCellStyle();
                            ICell cellTotal = row.CreateCell(attrib.Index, CellType.Blank);

                            if (!string.IsNullOrWhiteSpace(attrib.Formula))
                            {
                                cellTotal.SetCellType(CellType.Formula);
                                AreaReference range = new AreaReference(new CellReference(1, attrib.Index), new CellReference(rowCount - 1, attrib.Index));
                                cellTotal.CellFormula = string.Format(attrib.Formula, range.FormatAsString());
                                if (!string.IsNullOrWhiteSpace(attrib.Format))
                                    cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat(attrib.Format);
                                else
                                    cellStyle.DataFormat = -1;

                            }

                            cellStyle.BorderTop = BorderStyle.Thin;
                            cellStyle.BorderBottom = BorderStyle.Thin;
                            cellTotal.CellStyle = cellStyle;
                        }
                    }

                    //Ajuste de columnas
                    foreach (var pair in dicAttributes)
                        sheet.AutoSizeColumn(pair.Value.Index);
                }
            }

            return workbook;
        }

        private static List<T> GetListFromSheet<T>(XSSFWorkbook workbook, string sheetName) where T : class, new()
        {
            List<T> list = new List<T>();
            SheetAttribute sheetAttrib = typeof(T).GetCustomAttribute<SheetAttribute>();

            string sheetname = sheetName ?? sheetAttrib.SheetName;
            ISheet sheet = workbook.GetSheet(sheetname);
            try
            {

                if (sheet != null)
                {
                    List<CellType> validCells = new List<CellType>
                    {
                        CellType.Numeric,
                        CellType.Boolean,
                        CellType.String,
                        CellType.Blank
                    };

                    Type t = typeof(T);
                    PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    if (pi != null && pi.Count() > 0)
                    {
                        Dictionary<PropertyInfo, HeaderCustomAttribute> dicAttributes = new Dictionary<PropertyInfo, HeaderCustomAttribute>();
                        Dictionary<int, PropertyInfo> dicProperties = new Dictionary<int, PropertyInfo>();
                        Dictionary<int, Action<ICell, PropertyInfo, T>> dicValues = new Dictionary<int, Action<ICell, PropertyInfo, T>>();

                        for (int i = 0; i < pi.Count(); i++)
                        {
                            HeaderCustomAttribute attrib = pi[i].GetCustomAttribute<HeaderCustomAttribute>();

                            if (attrib != null)
                            {
                                dicAttributes.Add(pi[i], attrib);
                                dicProperties.Add(attrib.Index, pi[i]);

                                switch (attrib.CellType)
                                {
                                    case Enums.CellType.Numeric:
                                        dicValues.Add(attrib.Index, (cell, prop, obj) =>
                                        {
                                            try
                                            {
                                                if (prop.PropertyType == typeof(DateTime))
                                                    prop.SetValue(obj, Convert.ChangeType(cell.DateCellValue, prop.PropertyType));
                                                else
                                                {
                                                    HeaderCustomAttribute hA = dicAttributes[prop];
                                                    if (!string.IsNullOrWhiteSpace(hA.Format) && hA.Format.Contains("%"))
                                                        prop.SetValue(obj, Convert.ChangeType(cell.NumericCellValue * 100.0, prop.PropertyType));
                                                    else
                                                        prop.SetValue(obj, Convert.ChangeType(cell.NumericCellValue, prop.PropertyType));
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                throw new InvalidCastException($"No se pudo convertir el valor {cell.ToString()} a un valor numérico.\nVerifique la planilla excel.");
                                            }
                                        });
                                        break;
                                    case Enums.CellType.String:
                                        dicValues.Add(attrib.Index, (cell, prop, obj) =>
                                        {
                                            try
                                            {
                                                if (cell.CellType == CellType.Numeric) //Necesito string y esta como numeric
                                                    prop.SetValue(obj, Convert.ChangeType(cell.NumericCellValue, prop.PropertyType));
                                                else
                                                    prop.SetValue(obj, Convert.ChangeType(cell.StringCellValue, prop.PropertyType));
                                            }
                                            catch (Exception)   
                                            {
                                                throw new InvalidCastException($"No se pudo convertir el valor {cell.ToString()} a un tipo cadena.\nVerifique la planilla excel.");
                                            }
                                        });
                                        break;
                                    case Enums.CellType.Boolean:
                                        dicValues.Add(attrib.Index, (cell, prop, obj) =>
                                        {
                                            try
                                            {
                                                prop.SetValue(obj, Convert.ChangeType(cell.BooleanCellValue, prop.PropertyType));
                                            }
                                            catch (Exception)
                                            {
                                                throw new InvalidCastException($"No se pudo convertir el valor {cell.ToString()} al tipo booleano.\nVerifique la planilla excel.");
                                            }
                                        });
                                        break;
                                    case Enums.CellType.Blank:
                                    case Enums.CellType.Formula:
                                    case Enums.CellType.Unknown:
                                    case Enums.CellType.Error:
                                        throw new InvalidCastException("El tipo de celda no es válido.\nVerifique la planilla excel.");
                                }
                            }
                        }

                        IEnumerator rows = sheet.GetRowEnumerator();

                        //Excluir Header
                        int excluir = 0;
                        while (excluir++ < sheetAttrib.ExcludedRows)
                            rows.MoveNext();
                                                
                        //Recorro filas
                        while (rows.MoveNext())
                        {
                            T item = new T();
                            IRow row = (XSSFRow)rows.Current;

                            if (row.Cells.All(d => d.CellType == CellType.Blank)) //null is when the row only contains empty cells 
                                continue;

                            //Recorro Columnas
                            for (int i = 0; i < row.LastCellNum; i++)
                            {
                                ICell cell = row.GetCell(i);

                                if (cell != null)
                                {
                                    PropertyInfo prop = null;
                                    if (dicProperties.TryGetValue(cell.ColumnIndex, out prop))
                                    {
                                        if (cell != null && validCells.Any(a => cell.CellType == a))
                                            dicValues[cell.ColumnIndex].Invoke(cell, prop, item);
                                        else if (cell.CellType == CellType.Formula)
                                        {
                                            HeaderCustomAttribute attrib = prop.GetCustomAttribute<HeaderCustomAttribute>();
                                            if(attrib.AllowImportFormula)
                                                dicValues[cell.ColumnIndex].Invoke(cell, prop, item);
                                            else
                                                throw new InvalidCastException($"La celda {prop.Name} de la planilla {sheetname} no puede contener formulas");
                                        }
                                        else
                                            throw new InvalidCastException($"La Planilla {sheetname} contiene un tipo de celda ({row.RowNum + 1}, {cell.ColumnIndex + 1}) no válido");
                                    }

                                    if (prop == null)
                                        _logger.Log(NLog.LogLevel.Error, $"La columna {cell.ColumnIndex + 1} de la planilla {sheetname} no esta definida para importar");
                                }
                            }

                            list.Add(item);
                        }
                    }
                }
                else
                    throw new InvalidCastException($"La hoja {sheetname} no se encuentra en la planilla excel");
                
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "Error obtener los datos de la planilla excel");
                _logger.Log(NLog.LogLevel.Error, ex);

                if (ex is InvalidCastException)
                    throw;
                else
                    throw new Exception("Error obtener los datos de la planilla excel");
            }

            return list;
        }

        #endregion

        #region Métodos públicos

        public static XSSFWorkbook ToWorkbook(this byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);

            return new XSSFWorkbook(stream);
        }

        public static List<T> SheetToList<T>(this XSSFWorkbook workbook, string sheetName = null) where T:class, new()
        {
            return GetListFromSheet<T>(workbook, sheetName);
        }

        /// <summary>
        /// Genera un archivo excel desde una lista
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="sheetName"></param>
        /// <param name="fileName"></param>
        public static void SaveExcel<T>(this IEnumerable<T> enumerable, string sheetName, string fileName) where T : class
        {
            try
            {
                XSSFWorkbook workbook = CreateExcel(enumerable, sheetName);
                string fileSave = Path.Combine(Environment.CurrentDirectory, fileName + ".xlsx");

                FileStream sw = File.Create(fileSave);
                workbook.Write(sw);
                sw.Close();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "Error al generar la planilla excel");
                _logger.Log(NLog.LogLevel.Error, ex);
                throw;
            }
        }

        /// <summary>
        /// Devuelve una serie de bytes del un workbook
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="sheetName">Nombre de la hoja</param>
        /// <returns></returns>
        public static byte[] ToExcel<T>(this IEnumerable<T> enumerable, string sheetName) where T : class
        {
            try
            {
                XSSFWorkbook workbook = CreateExcel(enumerable, sheetName);

                return workbook.ToExcel();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "Error al generar la planilla excel");
                _logger.Log(NLog.LogLevel.Error, ex);
                throw;
            }
        }
        
        /// <summary>
        /// Obtener en bytes[] un libro excel
        /// </summary>
        /// <param name="workbook"></param>
        /// <returns></returns>
        public static byte[] ToExcel(this XSSFWorkbook workbook)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                workbook.Write(ms);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, "Error al generar la planilla excel");
                _logger.Log(NLog.LogLevel.Error, ex);
                throw;
            }
        }

        /// <summary>
        /// Crear un libro desde una lista
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static XSSFWorkbook ToWorkbook<T>(this IEnumerable<T> enumerable, string sheetName) where T : class
        {
            return CreateExcel(enumerable, sheetName);
        }

        /// <summary>
        /// Agregar una lista en una nueva hoja de un libro existente
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static XSSFWorkbook ToWorkbook<T>(this IEnumerable<T> enumerable, XSSFWorkbook workbook, string sheetName) where T : class
        {
            return AddToWorkBook(enumerable, workbook, sheetName);
        }
                
        #endregion
    }
}
