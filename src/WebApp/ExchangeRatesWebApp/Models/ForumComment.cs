using System;
namespace ExchangeRatesWebApp.Models
{
    public class ForumComment
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string OriginalMessage { get; set; }
    }
}
