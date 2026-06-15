using PlanesMultilinea.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Helpers;

using PlanesMultilinea.Entities;

namespace PlanesMultilinea
{
    public static class Config
    {
        public static Endpoint ReadEndpoint(string name)
        {
            var section = (ConfigSection)ConfigurationManager.GetSection("planesMultilinea");

            if (section == null)
                throw new ArgumentNullException("Seccion 'planesMultilinea' no definida en web.config");

            if (section.Instances == null || section.Instances.Count == 0)
                throw new ArgumentOutOfRangeException("La seccion 'planesMultilinea' no contiene instancias definidas");

            var instance = section.Instances.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (instance == null)
                throw new KeyNotFoundException($"La instancia con nombre '{name}' no existe en la seccion 'planesMultilinea'");

            return new Endpoint(instance.Endpoint, instance.AppUser, instance.AppKey);
        }
    }
}
