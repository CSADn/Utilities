using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercadoPagoModel.Entities
{
    [Serializable]
    public class Notification
    {
        public int id { get; set; }
        public string live_mode { get; set; }
        public string type { get; set; }
        public DateTime date_created { get; set; }
        public int user_id { get; set; }
        public string api_version { get; set; }
        public string action { get; set; }    
        public Data data { get; set; }
    }

    [Serializable]
    public class Data
    {
        public string id { get; set; }
    }
}
