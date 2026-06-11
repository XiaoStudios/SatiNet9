using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTsocketAPI.MT5
{
	public class Rates
	{
		public string TIME { get; set; }
		public double OPEN { get; set; }
		public double HIGH { get; set; }
		public double LOW { get; set; }
		public double CLOSE { get; set; }
		public int TICK_VOLUME { get; set; }
		public int SPREAD { get; set; }
		public int REAL_VOLUME { get; set; }

		public string TIME_MTAPI
		{
			get
			{
				if (DateTime.TryParseExact(
						TIME,
						"yyyy.MM.dd HH:mm:ss",
						System.Globalization.CultureInfo.InvariantCulture,
						System.Globalization.DateTimeStyles.None,
						out var dt))
				{
					// Resta 6 horas
					var dtApi = dt.AddHours(-6);
					return dtApi.ToString("yyyy.MM.dd HH:mm:ss");
				}
				return TIME;
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		public double CalculatedWAM { get; set; }
		public double PercentageDifference { get; set; }
		
		// Algoritmo Operativo (nuevo)
		public double PMPn { get; set; }
		public double MaxP { get; set; }
		public double MinP { get; set; }
		public double? PreviousPMPn { get; set; }
		public double RPPlus { get; set; }
		public double RPMinus { get; set; }
		public string Tendencia { get; set; }
		public double Difn { get; set; }
		public double? PromDifn { get; set; }
		public double? SigmaDifn { get; set; }
		public string Signal { get; set; }
		public double? ImporteAcumulacion { get; set; }
    }

	public class MarketBook
	{
		public double PRICE { get; set; }
		public int VOLUME { get; set; }
		public double VOLUMEREAL { get; set; }
		public string TYPE { get; set; }
	}

	public class MarketDepth
	{
		//public string MSG { get; set; }
		public string SYMBOL { get; set; }
		public List<MarketBook> MARKET_BOOK { get; set; }
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
