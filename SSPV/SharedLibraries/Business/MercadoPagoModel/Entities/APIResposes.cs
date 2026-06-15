using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercadoPagoModel.Entities
{
    public class APIRespose
    {
        public double status { get; set; }
        public object response { get; set; }
    }

    public class IdentificationTypes
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public double min_length { get; set; }
        public double max_length { get; set; }
    }

    [Serializable]
    public class Error
    {
        public double status { get; set; }
        public string error { get; set; }
        public List<ErrorCause> cause { get; set; }
        public string message { get; set; }
    }

    [Serializable]
    public class ErrorCause
    {
        public string description { get; set; }
        public string code { get; set; }
    }
    
    [Serializable]
    public class CustomerSearchResult
    {
        public List<Customer> results { get; set; }
        public object paging { get; set; }
    }
}
