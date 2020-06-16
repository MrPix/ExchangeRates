using System;
namespace ExchangeRatesWebApp.Models
{
    public class ExchangeRate
    {
        public Guid Id { get; set; }
        public string Source { get; set; }
        public DateTime Date { get; set; }
        public double SellRate { get; set; }
        public double BuyRate { get; set; }
    }
}
