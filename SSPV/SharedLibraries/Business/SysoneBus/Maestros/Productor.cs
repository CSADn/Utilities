using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sysoneBus.Entities;

namespace sysoneBus.Maestros
{
    public class Productor:Persona
    {
        public string Codigo { get; set; }
        public Grupo Grupo { get; set; }
        public Orgaizador Organizador { get; set;}


    }
}
