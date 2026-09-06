using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
    public class Deal
    {
        public string TIME { get; set; } = string.Empty;
        public long DEAL { get; set; }
        public string SYMBOL { get; set; } = string.Empty;
        public long ORDER { get; set; }
        public long POSITION { get; set; }
        public string TYPE { get; set; } = string.Empty;
        public string REASON { get; set; } = string.Empty;
        public string DIRECTION { get; set; } = string.Empty;
        public double PRICE { get; set; }
        public double VOLUME { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }
        public double COMMISSION { get; set; }
        public double PROFIT { get; set; }
        public int MAGIC { get; set; }
        public string COMMENT { get; set; } = string.Empty;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class OrderDeal
    {
        public string TIME { get; set; } = string.Empty;
        public long TICKET { get; set; }
        public string SYMBOL { get; set; } = string.Empty;
        public string TYPE { get; set; } = string.Empty;
        public double VOLUME { get; set; }
        public double PRICE { get; set; }
        public int MAGIC { get; set; }
        public string COMMENT { get; set; } = string.Empty;
        public List<Deal>? DEALS { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
