using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
    public class Order
    {
        public string TIME_SETUP { get; set; } = string.Empty;
        public string SYMBOL { get; set; } = string.Empty;
        public long TICKET { get; set; }
        public string TYPE { get; set; } = string.Empty;
        public double VOLUME_INITIAL { get; set; }
        public double VOLUME_CURRENT { get; set; }
        public double PRICE { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }
        public string STATE { get; set; } = string.Empty;
        public int MAGIC { get; set; }
        public string COMMENT { get; set; } = string.Empty;
        public string TIME_DONE { get; set; } = string.Empty;
        public long POSITION { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class OrderEvent
    {
        public string MSG { get; set; } = string.Empty;
        public TradeTransaction? TRADE_TRANSACTION { get; set; }
        public TradeRequest? TRADE_REQUEST { get; set; }
        public TradeResult? TRADE_RESULT { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class TradeTransaction
    {
        public long DEAL { get; set; }
        public string DEAL_TYPE { get; set; } = string.Empty;
        public long ORDER { get; set; }
        public string ORDER_STATE { get; set; } = string.Empty;
        public string ORDER_TYPE { get; set; } = string.Empty;
        public long POSITION { get; set; }
        public long POSITION_BY { get; set; }
        public double PRICE { get; set; }
        public double PRICE_SL { get; set; }
        public double PRICE_TP { get; set; }
        public double PRICE_TRIGGER { get; set; }
        public string SYMBOL { get; set; } = string.Empty;
        public string TIME_EXPIRATION { get; set; } = string.Empty;
        public string TIME_TYPE { get; set; } = string.Empty;
        public string TYPE { get; set; } = string.Empty;
        public string REASON { get; set; } = string.Empty;
        public double VOLUME { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class TradeResult
    {
        public double ASK { get; set; }
        public double BID { get; set; }
        public string COMMENT { get; set; } = string.Empty;
        public long DEAL { get; set; }
        public long ORDER { get; set; }
        public double PRICE { get; set; }
        public long REQUEST_ID { get; set; }
        public long RETCODE { get; set; }
        public int RETCODE_EXTERNAL { get; set; }
        public double VOLUME { get; set; }
        public string TYPE { get; set; } = string.Empty; //ADDED BY MTSOCKETAPI TO VISUALLY ASSIGN FULLY CLOSED OR PARTIALLY CLOSED MESSAGE
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class TradeRequest
    {
        public string ACTION { get; set; } = string.Empty;
        public string COMMENT { get; set; } = string.Empty;
        public int DEVIATION { get; set; }
        public string EXPIRATION { get; set; } = string.Empty;
        public int MAGIC { get; set; }
        public long ORDER { get; set; }
        public long POSITION { get; set; }
        public long POSITION_BY { get; set; }
        public double PRICE { get; set; }
        public double SL { get; set; }
        public double STOPLIMIT { get; set; }
        public string SYMBOL { get; set; } = string.Empty;
        public double TP { get; set; }
        public string TYPE { get; set; } = string.Empty;
        public string TYPE_FILLING { get; set; } = string.Empty;
        public string TYPE_TIME { get; set; } = string.Empty;
        public double VOLUME { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
