namespace CreditCardValidation
{
    public class ValidacionNumeroTarjeta
    {
        #region Propiedades

        public TipoTarjetaCredito TipoTarjetaSeleccionada { get; set; }

        public TipoTarjetaCredito TipoTarjetaIdentificada { get; set; }

        public string Mensaje { get; set; }

        public bool Valida { get; set; }

        #endregion

        #region Constructor

        public ValidacionNumeroTarjeta(int tipoTarjeta)
        {
            TipoTarjetaSeleccionada = (TipoTarjetaCredito)tipoTarjeta;
            TipoTarjetaIdentificada = TipoTarjetaCredito.InvalidCard;
            Valida = false;
            Mensaje = null;
        }

        #endregion
    }
}
