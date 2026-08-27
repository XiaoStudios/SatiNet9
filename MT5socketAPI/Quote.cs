using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
    public class Quote
    {
        public string SYMBOL { get; set; } = string.Empty;
        public double ASK { get; set; }
        public double BID { get; set; }
        public int FLAGS { get; set; }
        public string TIME { get; set; } = string.Empty;
        public int VOLUME { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class OHLC_Msg
    {
        //public string MSG { get; set; }
        public string SYMBOL { get; set; } = string.Empty;
        public string PERIOD { get; set; } = string.Empty;
        public List<Rates>? OHLC { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

	public class OHLC_Req
	{
        //public string MSG { get; set; }
        [Required]
		public string SYMBOL { get; set; } = string.Empty;
		[Required]
		public TimeFrame TIMEFRAME { get; set; }
		public int? DEPTH { get; set; }
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
