using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using uniVerseSise.Configuracion;

namespace uniVerseSise.Manager.Helpers
{
    public class AtrtibuteInspectorManager
    {
        public Type Type { get; private set; }
        public PropertyInfo[] Propiedades { get; private set; }
        public NombreArchivoAttribute NombreArchivo { get; private set; }
        public List<KeyValuePair<DatoCampoAttribute, PropertyInfo>> DataCampoPropiedad { get; private set; }

        public AtrtibuteInspectorManager(Type typeT)
        {
            this.Type = typeT;

            this.NombreArchivo = (NombreArchivoAttribute)typeT.GetCustomAttributes(typeof(NombreArchivoAttribute), false).FirstOrDefault();
            if (this.NombreArchivo == null)
                throw new Exception($"La clase {typeT.Name} no posee el artibuto NombreArchivoAttribute");

            this.Propiedades = typeT.GetProperties();

            //Preparo los valores de los campos
            this.DataCampoPropiedad = new List<KeyValuePair<DatoCampoAttribute, PropertyInfo>>();

            //Por cada una de las propiedades
            foreach (var propInfo in this.Propiedades)
            {
                foreach (DatoCampoAttribute dataCampoAttr in propInfo.GetCustomAttributes(typeof(DatoCampoAttribute), false))
                {
                    if (dataCampoAttr != null)
                        this.DataCampoPropiedad.Add(new KeyValuePair<DatoCampoAttribute, PropertyInfo>(dataCampoAttr, propInfo));
                }
            }
        }
    }
}
