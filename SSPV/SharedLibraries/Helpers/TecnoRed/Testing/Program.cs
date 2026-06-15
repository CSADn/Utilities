using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TecnoRed;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //var centros = TecnoRedHelper.Client.ListaDeCentros();
                //centros.ForEach(f => Console.WriteLine(string.Concat("(", f.Uid, ") ", f.Direccion, " - ", f.Horario)));

                //TecnoRedHelper.Client.Localidades(1407);

                //var turnos = TecnoRedHelper.Client.Turnos(1198);
                //turnos.ForEach(f => Console.WriteLine(string.Concat("(", f.Codigo, ") ", f.Fecha.ToShortDateString(), " ", f.HoraDesde, "hs")));

                //var codigoReserva = TecnoRedHelper.Client.ReservarTurno(new TecnoRed.Commands.ReservarTurno
                //{
                //    CodTurno = "1510",
                //    CodLocalidad = "1198",
                //    Fecha = DateTime.ParseExact("2016-03-16", "yyyy-MM-dd", null),
                //    HoraDesde = "9"
                //});
                //Console.WriteLine("Reserva: " + codigoReserva);

                //var ok = TecnoRedHelper.Client.CancelarReserva(814940);

                //var trd = TecnoRedHelper.Client.ConfirmarReserva(new TecnoRed.Commands.ConfirmarReserva
                //{
                //    NumeroDeReserva = "814942",
                //    Nombre = "Nombre",
                //    Apellido = "Apellido",
                //    Calle = "Calle",
                //    EntreCalles = "Calle y Calle",
                //    CalleNro = "1234",
                //    Piso = "",
                //    Departamento = "",
                //    Telefono = "011 12341234",
                //    Celular = "15 12341234",
                //    Dominio = "ASD123",
                //    Marca = "FIAT",
                //    Modelo = "PALIO",
                //    Año = "2016",
                //    CodAseguradora = "123"
                //});
                //Console.WriteLine("TRD: " + trd); // 7518632

                //var trd = TecnoRedHelper.Client.InspeccionEnCentro(new TecnoRed.Commands.InspeccionEnCentro
                //{
                //    UidCentro = "1",
                //    Nombre = "Nombre",
                //    Apellido = "Apellido",
                //    Fecha = DateTime.Parse("2016-03-16"),
                //    Telefono = "011 12341234",
                //    Celular = "15 12341234",
                //    Dominio = "ASD123",
                //    Marca = "FIAT",
                //    Modelo = "PALIO",
                //    Año = "2016",
                //    CodAseguradora = "123",
                //    Referencia = "Referencia"
                //});
                //Console.WriteLine("TRD: " + trd); // 20167848

                var trd = TecnoRedHelper.Client.InspeccionEnDomicilio(new TecnoRed.Commands.InspeccionEnDomicilio
                {
                    Nombre = "Nombre",
                    Apellido = "Apellido",
                    Provincia = "Capital Federal",
                    Localidad = "Localidad",
                    Calle = "Calle",
                    EntreCalles = "Calle y Calle",
                    CalleNro = "1234",
                    Piso = "",
                    Departamento = "",
                    Telefono = "011 12341234",
                    Celular = "15 12341234",
                    Dominio = "ASD123",
                    Marca = "FIAT",
                    Modelo = "PALIO",
                    Año = "2016",
                    CodAseguradora = "123"
                });
                Console.WriteLine("TRD: " + trd); // 20167848

                var ok = TecnoRedHelper.Client.DejarSinEfecto(trd);
            }
            catch (TecnoRedException ex)
            {
                Console.WriteLine(string.Concat(ex.Code, " - ", ex.Message));
            }

            Console.Write("FIN.");
            Console.ReadKey();
        }
    }
}
