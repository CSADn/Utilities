using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TecnoRed.Commands
{
    public class InspeccionEnCentro : Command
    {
        private const string _uidCentro = "uidCentro";
        private const string _fecha = "fecha";
        private const string _nombre = "nombre";
        private const string _apellido = "apellido";
        private const string _telefono = "telefonoFijo";
        private const string _celular = "celular";
        private const string _dominio = "dominio";
        private const string _codAseguradora = "codigoAseguradora";

        private const string _marca = "marca";
        private const string _modelo = "modelo";
        private const string _año = "anioUnidad";
        private const string _referencia = "referencia";
        private const string _aseguradora = "aseguradora";

        public string UidCentro
        {
            get { return Parameters[_uidCentro]; }
            set { SetParameter(_uidCentro, value); }
        }

        public DateTime Fecha
        {
            get { return DateTime.ParseExact(Parameters[_fecha], "yyyy-MM-dd", null); }
            set { SetParameter(_fecha, value.ToString("yyyy-MM-dd")); }
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


        public InspeccionEnCentro()
        {
            Name = "agendarEnCentro";
        }

        public override void Validate()
        {
            return;
        }
    }
}
