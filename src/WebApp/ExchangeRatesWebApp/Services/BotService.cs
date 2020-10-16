using System;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace ExchangeRatesWebApp.Services
{
    public class BotService : IBotService
    {
        private readonly BotConfiguration _config;

        public BotService(IOptions<BotConfiguration> config)
        {
            _config = config.Value;
            if (!config.Value.Enabled) return;
            // use proxy if configured in appsettings.*.json
            Client = new TelegramBotClient(_config.BotToken);
        }

        public TelegramBotClient Client { get; }
    }
}
