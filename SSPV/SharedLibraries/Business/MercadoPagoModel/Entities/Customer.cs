using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercadoPagoModel.Entities
{
    [Serializable]
    public class Customer
    {
        public string id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public Phone phone { get; set; }
        public Indetification identification { get; set; }
        public string default_address { get; set; }
        public Address address { get; set; }
        public DateTime? date_registered { get; set; }
        public string description { get; set; }
        public DateTime date_created { get; set; }
        public DateTime? date_last_updated { get; set; }
        public object metadata { get; set; }
        public string default_card { get; set; }
        public dynamic cards { get; set; }
        public bool live_mode { get; set; }
        public object addresses { get; set; }
    }

    [Serializable]
    public class Phone
    {
        public string area_code { get; set; }
        public string number { get; set; }
    }

    [Serializable]
    public class Indetification
    {
        public string type { get; set; }
        public string number { get; set; }
    }

    [Serializable]
    public class Address
    {
        public string id { get; set; }
        public string zip_code { get; set; }
        public string street_name { get; set; }
        public string street_number { get; set; }
    }

    //[Serializable]
    //public class Card
    //{
    //    public string id { get; set; }
    //    public string customer_id { get; set; }
    //    public int expiration_month { get; set; }
    //    public int expiration_year { get; set; }
    //    public string first_six_digits { get; set; }
    //    public string last_four_digits { get; set; }
    //    public PaymentMethod payment_method { get; set; }
    //    public SecurityCode security_code { get; set; }
    //    public Issuer issuer { get; set; }
    //    public CardHolder cardholder { get; set; }
    //    public DateTime? date_created { get; set; }
    //    public DateTime? date_last_updated { get; set; }
    //}

    [Serializable]
    public class PaymentMethod
    {
        public string id { get; set; }
        public string name { get; set; }
        public string payment_type_id { get; set; }
        public string thumbnail { get; set; }
        public string secure_thumbnail { get; set; }
    }

    [Serializable]
    public class SecurityCode
    {
        public int length { get; set; }
        public string card_location { get; set; }
    }

    [Serializable]
    public class Issuer
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    [Serializable]
    public class CardHolder
    {
        public string name { get; set; }
        public IdentificationCard identification { get; set; }
    }

    [Serializable]
    public class IdentificationCard
    {
        public int? number { get; set; }
        public string subtype { get; set; }
        public string type { get; set; }

    }
}
