using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExchangeRatesWebApp.CronJobServices.KursComUa
{
    public class Series
    {
        [JsonPropertyName("data")]
        public IList<IList<object>> Data { get; set; }
    }

    public class TableData
    {
        [JsonPropertyName("series")]
        public IList<Series> Series { get; set; }
    }


}
