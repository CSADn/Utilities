using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercadoPagoModel.Entities
{
    [Serializable]
    public class Payment
    {
        public double transaction_amount { get; set; }
        public string token { get; set; }
        public string description { get; set; }
        public int installments { get; set; }
        public string payment_method_id { get; set; }
        public object payer { get; set; }
        public bool capture { get; set; }
        public int external_reference { get; set; }
    }

    [Serializable]
    public class Payer
    {
        public string email { get; set; }
    }

    [Serializable]
    public class PayerId
    {
        public string id { get; set; }
    }

    [Serializable]
    public class AutomaticPayment
    {
        public double transaction_amount { get; set; }
        public string token { get; set; }
        public string description { get; set; }
        public int installments { get; set; }
        public AutomaticPayer payer { get; set; }
        public int external_reference { get; set; }
    }

    [Serializable]
    public class AutomaticPayer
    {
        public string id { get; set; }
    }
}
