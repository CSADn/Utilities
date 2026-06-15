using System;
using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Helpers;
using Arboles;
using Entities;

namespace historico
{
    public class GeneradorHistorico
    {
        private static GeneradorHistorico _instance;

        public List<Entities.ParametroHistoricoCabecera> _listaTablasCabeceras = new List<Entities.ParametroHistoricoCabecera>();
        public static GeneradorHistorico Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GeneradorHistorico();
                return _instance;
            }
        }

        #region proceso principal

        public void iniciarProceso()

        {
            try
            {
                _listaTablasCabeceras = Crud.ParametroHistoricoCabecera.Instance.GetAllParametroHistoricoCabecera();
                if (_listaTablasCabeceras == null)
                    return;
                foreach (var itemCabecera in _listaTablasCabeceras)
                {
                    List<object> listaTablaArbolProcesar = new List<object>();
                    List<string> listaTablaProcesar = new List<string>();
                    //Crear raiz arbol
                    Arbol raiz = new Arbol(itemCabecera.nombreTablaCabecera);
                    List<TablaFamilia> listaNodosArbol = new List<TablaFamilia>();
                    //Generar Arbol segun base de datos y sus relaciones. para poder procesarlo de manera que no de excepcion por fks
                    GenerarArbolDB(raiz,listaNodosArbol);
                    //Obtienen los identificadores para generar tablas temporales para filtrado
                    var listaTablasTemporales = Crud.TablaTemporal.Instance.Get_Data(listaNodosArbol);
                    //Metodo para establecer el orden del procesamiento de tablas para evitar errores por relaciones.
                    raiz.PostOrden(listaTablaArbolProcesar);
                    //Se comentaron las siguientes lineas por no acordarme que hacen.
                    foreach (var item in listaTablaArbolProcesar)
                    {
                        if (item.ToString() != itemCabecera.nombreTablaCabecera)
                            listaTablaProcesar.Add(item.ToString());
                    }
                //Proceso de consolidacion de estructuras tablas base vs historicas(incluye tanto cabeceras como detalles)
                // Revisa estructura, registros existentes y si hay cambios en estructura se realizan dejando las tablas historicas a las tablas base.
                TablaHistorico_ConsolidarEstructura.iniciarConsolidacion(itemCabecera.nombreTablaCabecera, listaTablaProcesar);
                    //Migracion de datos de tabla Base a Historico
                    TablaHistorico_MigrarDatos.Instance.iniciarProcesoTablaBaseToHistorico(itemCabecera.nombreTablaCabecera, listaTablaProcesar, listaTablasTemporales);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void GenerarArbolDB(Arbol nuevoNodo, List<TablaFamilia> listaNodos)
        {
            try
            {
            var tablasHijos = Crud.TablaFamilia.Instance.GetTablaFamiliaByClave(nuevoNodo.Valor.ToString());
            if (tablasHijos.Count() > 0)
            {
                foreach (var itemHijo in tablasHijos)
                {
                    Arbol nuevoNodoHijo = new Arbol(itemHijo.NombreTablaHijo);
                    nuevoNodo.Add(nuevoNodoHijo);
                    listaNodos.Add(itemHijo);
                    GenerarArbolDB(nuevoNodoHijo, listaNodos);
                }
            }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
