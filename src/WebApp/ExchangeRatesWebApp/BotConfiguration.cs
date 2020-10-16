using System;
namespace ExchangeRatesWebApp
{
    public class BotConfiguration
    {
        public string BotToken { get; set; }

        public string Socks5Host { get; set; }

        public int Socks5Port { get; set; }

        public bool Enabled { get; set; }
    }
}
