using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExchangeRatesWebApp.CronJobServices.KursComUa
{
    public class Comment
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }

    public class ForumComments
    {
        [JsonPropertyName("data")]
        public IList<Comment> Data { get; set; }
    }
}
