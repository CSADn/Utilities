using Helpers;

namespace Entities
{
    public class Bin_Codes : EntityBase<Bin_Codes>
    {
        #region Propiedades

        public string BinCode { get; set; }

        public string Brand { get; set; }

        public string CountryCode { get; set; }

        public string Country { get; set; }

        public string Bank { get; set; }

        public string CardType { get; set; }

        public string CardCategory { get; set; }

        public string CardSubBrand { get; set; }

        #endregion
    }
}
