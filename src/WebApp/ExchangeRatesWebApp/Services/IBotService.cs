using System;
using Telegram.Bot;

namespace ExchangeRatesWebApp.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}
