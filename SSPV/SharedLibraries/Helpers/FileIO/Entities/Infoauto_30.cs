using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace EntitiesIO
{
    [DelimitedRecord(";")]
    [IgnoreFirst(1)]
    public class Infoauto_30
    {
        public string Marca;

        public string ModGrupo;

        public string Modelo;

        public string Cod;

        public double Usados;

        public double Anno01;

        public double Anno02;

        public double Anno03;

        public double Anno04;

        public double Anno05;

        public double Anno06;

        public double Anno07;

        public double Anno08;

        public double Anno09;

        public double Anno10;

        public double Anno11;

        public double Anno12;

        public double Anno13;

        public double Anno14;

        public double Anno15;

        public double Anno16;

        public double Anno17;

        public double Anno18;

        public double Anno19;

        public double Anno20;

        public double Anno21;

        public double Anno22;

        public double Anno23;

        public double Anno24;

        public double Anno25;

        public double Anno26;

        public double Anno27;

        public double Anno28;

        public double Anno29;

        public double Anno30;

        public string Combustible;

        public string Alimentacion;

        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal Cilindrada;

        public string CanPuertas;

        public string Tipo;

        public string TipoAIG;

        public string SuscribeCasco;

        public string Cabina;

        public string Carga;

        public double PesoKgs;

        public double Kms;

        public double Potencia;

        public string Direccion;

        public string AireAcond;

        public string Traccion;

        public string Importado;

        public string Caja;

        public string ABS;

        public string Airbag;

        public string CodCategoria;
    }
}
