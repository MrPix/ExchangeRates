using System;
using Telegram.Bot;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using ExchangeRatesWebApp.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ExchangeRatesWebApp.Services
{
    public class BotReplyService : IHostedService, IDisposable
    {
        private readonly BotConfiguration _config;
        private readonly ILogger<BotReplyService> _logger;
        public static TelegramBotClient _client;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BotReplyService(IOptions<BotConfiguration> config, IBotService botService, ILogger<BotReplyService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _config = config.Value;
            _logger = logger;
            _client = botService.Client;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new TelegramBotClient(_config.BotToken);
            var me = _client.GetMeAsync().Result;
            _logger.LogInformation($"BotService: started with bot {me.Username}");
            _client.OnMessage += Client_OnMessage;
            _client.StartReceiving(Array.Empty<UpdateType>());
            return Task.CompletedTask;
        }

        private async void Client_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;
            User user = message.From;
            FindOrCreateUserByChatId(user, message.Chat.Id);

            await Usage(message);

            static async Task Usage(Message message)
            {
                const string usage = "Usage:\n" +
                                        "/inline   - send inline keyboard\n" +
                                        "/keyboard - send custom keyboard\n" +
                                        "/photo    - send a photo\n" +
                                        "/request  - request location or contact";
                await _client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: usage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.StopReceiving();
            return Task.CompletedTask;
        }

        private Guid FindOrCreateUserByChatId(User telegramUser, long chatId)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<DataContext>();
            Models.User user = context.Users.FirstOrDefault(u => u.ChatId == chatId);

            if (user == null)
            {
                user = new Models.User()
                {
                    FirstName = telegramUser.FirstName,
                    LastName = telegramUser.LastName,
                    Username = telegramUser.Username,
                    TelegramId = telegramUser.Id,
                    ChatId = chatId,
                    IsActive = true
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
            return user.Id;
        }
    }
}
