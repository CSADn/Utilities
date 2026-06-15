using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crud
{
    public class TablaTemporal
    {
        private static TablaTemporal _instance;

        public static TablaTemporal Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TablaTemporal();

                return _instance;
            }
        }

        public TablaTemporal()
        {
            //
        }

        public List<Entities.TablaTemporal> Get_Data(List<Entities.TablaFamilia> listaFamiliaBase)
        {
            List<Entities.TablaTemporal> resultadoFinal = new List<Entities.TablaTemporal>();
            List<string> listaPadres = new List<string>();
            foreach (var item in listaFamiliaBase)
            {
                if (!listaPadres.Any(x => x == (string)item.NombreTablaPadre))
                    listaPadres.Add((string)item.NombreTablaPadre);
            }
            List<string> listaHijos = new List<string>();
            foreach (var item in listaFamiliaBase)
            {
                if (!listaPadres.Any(x => x == (string)item.NombreTablaHijo))
                    if (!listaHijos.Any(x => x == (string)item.NombreTablaHijo))
                        listaHijos.Add((string)item.NombreTablaHijo);
            }
            foreach (var itemNombreTablaPadre in listaPadres)
            {
                Entities.TablaTemporal itemTablaTemporal;
                var itemTablaPadreEsHijo = listaFamiliaBase
                                        .Where(
                                                x => listaFamiliaBase.Any(y => 
                                                                            (string)y.NombreTablaHijo == (string)itemNombreTablaPadre &&
                                                                            (string)y.NombreTablaHijo == (string)x.NombreTablaHijo
                                                                            )
                                                ).ToList();
                if (itemTablaPadreEsHijo.Count() == 0)
                {
                    //Busco dentro de los padres y obtengo el id Pk 
                    var itemTablaPadreSolamente = listaFamiliaBase.Where(x => (string)x.NombreTablaPadre == (string)itemNombreTablaPadre).FirstOrDefault();
                    itemTablaTemporal = new Entities.TablaTemporal
                    {
                        NombreTabla = (string)itemNombreTablaPadre,
                        Pk = itemTablaPadreSolamente.NombreColumnaPadre,
                        NombreTablaFiltro = (string)itemNombreTablaPadre,
                        ClaveFiltro = itemTablaPadreSolamente.NombreColumnaPadre,
                        bCrearTemporal = true
                    };
                }
                else
                {
                    //Busco dentro de los hijos y obtengo el id Pk que tiene como hijo.
                    var itemTablaPadre = listaFamiliaBase.Where(x => (string)x.NombreTablaPadre == (string)itemNombreTablaPadre).FirstOrDefault();
                    var itemTablaPadreHijo = listaFamiliaBase.Where(x => (string)x.NombreTablaHijo == (string)itemNombreTablaPadre).FirstOrDefault();
                    itemTablaTemporal = new Entities.TablaTemporal
                    {
                        NombreTabla = itemTablaPadreHijo.NombreTablaHijo,
                        Pk = itemTablaPadre.NombreColumnaPadre,
                        NombreTablaFiltro = itemTablaPadreHijo.NombreTablaPadre,
                        ClaveFiltro = itemTablaPadreHijo.NombreColumnaHijo,
                        bCrearTemporal = true
                    };

                }
                resultadoFinal.Add(itemTablaTemporal);
            }
            //Procesando a los hijos
            foreach (var itemNombreTablaHijo in listaHijos)
            {
                Entities.TablaTemporal itemTablaTemporal;
                //Busco dentro de los hijos y obtengo el id Pk que tiene como hijo.
                var listaTablaPadreHijo = listaFamiliaBase.Where(x => (string)x.NombreTablaHijo == (string)itemNombreTablaHijo).ToList();
                foreach (var item in listaTablaPadreHijo)
                {
                    itemTablaTemporal = new Entities.TablaTemporal
                    {
                        NombreTabla = (string)item.NombreTablaHijo,
                        Pk = item.NombreColumnaHijo,
                        NombreTablaFiltro = (string)item.NombreTablaPadre,
                        ClaveFiltro = item.NombreColumnaHijo,
                        bCrearTemporal = false
                    };
                    resultadoFinal.Add(itemTablaTemporal);
                }
            }
            var soloTablasTemporales = resultadoFinal.Where(x => x.bCrearTemporal).ToList();
            var resultadoFiltradoSoloClavesPrincipales = resultadoFinal.Where(y=> soloTablasTemporales.Any(x => x.ClaveFiltro==y.ClaveFiltro)).ToList();
            return resultadoFiltradoSoloClavesPrincipales;
        }
    }
}

