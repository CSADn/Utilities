using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using mercadopago;
using MercadoPagoModel.Entities;

namespace MercadoPagoManager
{
    public class MercadoPagoManager
    {
        private static MercadoPagoManager _instance;

        public static MercadoPagoManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MercadoPagoManager();
                return _instance;
            }
        }

        public List<IdentificationTypes> GetIdentificationTypes()
        {
            //MP mp = new MP("MP_CLIENT_ID".FromAppSettings(string.Empty), "MP_CLIENT_SECRET".FromAppSettings(string.Empty));
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.get("/v1/identification_types", true);

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(200))
                return JsonConvert.DeserializeObject<List<IdentificationTypes>>(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object RealizarCobro(Payment payment)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.post("/v1/payments", JsonConvert.SerializeObject(payment));

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(201))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object RealizarCobroAutomatico(AutomaticPayment payment)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.post("/v1/payments", JsonConvert.SerializeObject(payment));

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(201))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object ObtenerCustomer(string customerId)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.get("/v1/customers/" + customerId, true);

            var response = JsonConvert.SerializeObject(result["response"]);
            
            if (result["status"].Equals(200))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public Customer ObtenerCustomer(string email, out bool isNewCustomer)
        {
            isNewCustomer = false;

            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Dictionary<String, String> filters = new Dictionary<String, String>();
            filters.Add("email", email);

            Hashtable result = mp.get("/v1/customers/search", filters, true);

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(200))
            {
                var responseObject = JsonConvert.DeserializeObject<CustomerSearchResult>(response);
                if (responseObject.results.Count > 0)
                {
                    return responseObject.results.FirstOrDefault();
                }
                else //Si no existe, lo creo...
                {
                    isNewCustomer = true;
                    return CrearCustomer(email);
                }                    
            }
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public Customer CrearCustomer(string email)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Payer payer = new Payer { email = email };
            string sPayer = JsonConvert.SerializeObject(payer);
            //Hashtable hPayer = new Hashtable();
            //hPayer.Add("email", email);

            Hashtable result = mp.post("/v1/customers", sPayer);

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(201))
                return JsonConvert.DeserializeObject<Customer>(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object AsociarTarjeta(string customerId, string cardToken)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.post("/v1/customers/" + customerId + "/cards", "{\"token\" : \"" + cardToken + "\"}");

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(201))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object ObtenerTarjetas(string customerId)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.get("/v1/customers/" + customerId + "/cards", true);

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(200))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object ObtenerTarjeta(string cardId, string customerId)
        {
            dynamic cards = ObtenerTarjetas(customerId);

            int iFound = -1;
            if (cards != null && cards.Count > 0)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    if (cards[i].id == cardId)                        
                    {
                        iFound = i;
                        break;
                    }
                }

                if (iFound >= 0)
                    return cards[iFound];
            }

            return null;
        }

        public object ObtenerNuevoTokenTarjeta(string cardId)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.post("/v1/card_tokens", "{\"card_id\" : \"" + cardId + "\"}");

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(201))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object ObtenerCobro(string paymentId)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.get("/v1/payments/" + paymentId, true);            

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(200))
                return JsonConvert.DeserializeObject(response);            
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object ObtenerNotification(string notificationId)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.get("/v1/collections/notifications/" + notificationId, true);

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(200))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        public object ObtenerMerchantOrder(string merchantOrderId)
        {
            MP mp = new MP("MP_CLIENT_SECRET_PERSONALIZED".FromAppSettings(string.Empty));
            //mp.sandboxMode(true);

            Hashtable result = mp.get("/v1/merchant_orders/" + merchantOrderId, true);

            var response = JsonConvert.SerializeObject(result["response"]);

            if (result["status"].Equals(200))
                return JsonConvert.DeserializeObject(response);
            else
                throw new Exception(GetFormattedErrorMessage(response));
        }

        private string GetFormattedErrorMessage(string sResponse) {
            Error error = JsonConvert.DeserializeObject<Error>(sResponse);
            return string.Format("{0} - {1} - {2} - {3} - {4}", error.status, 
                                                                    error.error, 
                                                                    error.message,
                                                                    error.cause.FirstOrDefault().code ?? string.Empty,
                                                                    error.cause.FirstOrDefault().description ?? string.Empty);
        }
    }
}
