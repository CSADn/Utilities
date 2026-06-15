using DatabaseModel;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crud
{
    public class Bin_Codes
    {
        #region Instancia

        private static Bin_Codes _instance = null;

        public static Bin_Codes Instance
        {
            get
            {
                if (null == _instance)
                    _instance = new Bin_Codes();

                return _instance;
            }
        }

        #endregion

        #region Miembros Privados

        private DataModel _dataModel = null;
        private string _dbName = null;

        #endregion

        #region Constructor

        protected Bin_Codes()
        {
            _dataModel = DataModel.Instance;

            string key = "WSLookups";
            _dbName = key.ConnectionExists() ? key : "DB".FromAppSettings<string>(null, true);
        }

        #endregion

        #region Metodos Privados

        private void ValidateBinCode(string binCode)
        {
            if (string.IsNullOrWhiteSpace(binCode))
                throw new Exception("Código de tarjeta inválido");

            if (binCode.Length != 6)
                throw new Exception("El código no tiene el largo correspondiente (6 caracteres)");
        }

        #endregion

        #region Metodos Publicos

        public List<Entities.Bin_Codes> GetAll()
        {
            string query = @"SELECT * FROM BIN_CODES";

            return _dataModel.Execute<Entities.Bin_Codes>(_dbName, query);
        }

        public Entities.Bin_Codes GetByCode(string binCode)
        {
            ValidateBinCode(binCode);

            string query = @"SELECT * FROM BIN_CODES WHERE binCode = ?";

            return _dataModel.Execute<Entities.Bin_Codes>(_dbName, query, new object[] { binCode }).FirstOrDefault();
        }

        public string Add(Entities.Bin_Codes entity, bool useTransaction = true)
        {
            string query = @"INSERT INTO BIN_CODES (binCode, brand, countryCode, country, bank, cardType, cardCategory, cardSubBrand) 
                             OUTPUT INSERTED.binCode 
                             VALUES (?,?,?,?,?,?,?,?);";

            var parameters = new object[] { entity.BinCode, entity.Brand, entity.CountryCode, entity.Country, entity.Bank, entity.CardType, entity.CardCategory, entity.CardSubBrand };
            var dt = _dataModel.Execute(_dbName, query, parameters);

            return Convert.ToString(dt.Rows[0][0]);
        }

        public bool Delete(string binCode)
        {
            return _dataModel.ExecuteNonQuery(_dbName, "DELETE FROM BIN_CODES WHERE binCode = ?", new object[] { binCode }) > 0;
        }

        #endregion
    }
}
