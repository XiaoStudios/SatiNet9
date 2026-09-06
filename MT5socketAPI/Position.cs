using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
    public class Position
    {
        public long TICKET { get; set; }
        //public string TIME { get; set; } //<- REPLACED BY OPEN_TIME
        public string OPEN_TIME { get; set; } = string.Empty;
		public string TIME_UPDATE { get; set; } = string.Empty;
        public string SYMBOL { get; set; } = string.Empty;
        public string TYPE { get; set; } = string.Empty;
        public double VOLUME { get; set; }
        public double PRICE_OPEN { get; set; }
        public int MAGIC { get; set; }
        public string COMMENT { get; set; } = string.Empty;
        public string CLOSE_TIME { get; set; } = string.Empty;
        public double PRICE_CLOSE { get; set; }
        public double PROFIT { get; set; }
        public double COMMISSION { get; set; }
        public double SWAP { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }
        public long IDENTIFIER { get; set; }
        public long REASON { get; set; }
        public double PRICE_CURRENT { get; set; }
        public string EXTERNAL_ID { get; set; } = string.Empty;
        public double CHANGE { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
