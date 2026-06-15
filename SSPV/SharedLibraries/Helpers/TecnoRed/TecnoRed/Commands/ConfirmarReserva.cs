using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnoRed.Commands
{
    public class ConfirmarReserva : Command
    {
        private const string _numeroDeReserva = "numeroDeReserva";
        private const string _nombre = "nombre";
        private const string _apellido = "apellido";
        private const string _calle = "calle";
        private const string _entreCalles = "entreCalles";
        private const string _telefono = "telefonoFijo";
        private const string _celular = "celular";
        private const string _dominio = "dominio";
        private const string _codAseguradora = "codigoAseguradora";

        private const string _calleNro = "numeroEnCalle";
        private const string _piso = "piso";
        private const string _depto = "departamento";
        private const string _marca = "marca";
        private const string _modelo = "modelo";
        private const string _año = "anioUnidad";
        private const string _referencia = "referencia";
        private const string _aseguradora = "aseguradora";


        public string NumeroDeReserva
        {
            get { return Parameters[_numeroDeReserva]; }
            set { SetParameter(_numeroDeReserva, value); }
        }

        public string Nombre
        {
            get { return Parameters[_nombre]; }
            set { SetParameter(_nombre, value); }
        }

        public string Apellido
        {
            get { return Parameters[_apellido]; }
            set { SetParameter(_apellido, value); }
        }

        public string Calle
        {
            get { return Parameters[_calle]; }
            set { SetParameter(_calle, value); }
        }

        public string EntreCalles
        {
            get { return Parameters[_entreCalles]; }
            set { SetParameter(_entreCalles, value); }
        }

        public string Telefono
        {
            get { return Parameters[_telefono]; }
            set { SetParameter(_telefono, value); }
        }

        public string Celular
        {
            get { return Parameters[_celular]; }
            set { SetParameter(_celular, value); }
        }

        public string Dominio
        {
            get { return Parameters[_dominio]; }
            set { SetParameter(_dominio, value); }
        }

        public string CodAseguradora
        {
            get { return Parameters[_codAseguradora]; }
            set { SetParameter(_codAseguradora, value); }
        }

        //Solo para brokers
        public string Aseguradora
        {
            get { return Parameters[_aseguradora]; }
            set { SetParameter(_aseguradora, value); }
        }

        public string CalleNro
        {
            get { return Parameters[_calleNro]; }
            set { SetParameter(_calleNro, value); }
        }

        public string Piso
        {
            get { return Parameters[_piso]; }
            set { SetParameter(_piso, value); }
        }

        public string Departamento
        {
            get { return Parameters[_depto]; }
            set { SetParameter(_depto, value); }
        }

        public string Marca
        {
            get { return Parameters[_marca]; }
            set { SetParameter(_marca, value); }
        }

        public string Modelo
        {
            get { return Parameters[_modelo]; }
            set { SetParameter(_modelo, value); }
        }

        public string Año
        {
            get { return Parameters[_año]; }
            set { SetParameter(_año, value); }
        }

        public string Referencia
        {
            get { return Parameters[_referencia]; }
            set { SetParameter(_referencia, value); }
        }


        public ConfirmarReserva()
        {
            Name = "confirmarReservaPendiente";
        }

        public override void Validate()
        {
            return;
        }
    }
}
