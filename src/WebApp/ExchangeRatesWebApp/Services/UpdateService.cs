using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeRatesWebApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace ExchangeRatesWebApp.Services
{
    public class UpdateService : IUpdateService
    {
        public TelegramBotClient _client;
        private IServiceScopeFactory _serviceScopeFactory;

        public UpdateService(IBotService botService, ILogger<UpdateService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _client = botService.Client;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Update()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<DataContext>();
            List<Models.User> users = context.Users.Where(u => u.IsActive == true).ToList();
            List<Models.ForumComment> comments = context.ForumComments.OrderBy(c => c.Date).ToList();
            if (comments.Count == 0) return;
            foreach(var user in users)
            {
                if (user.LastForumMessageDate != DateTime.MinValue)
                {
                    var newComments = comments
                        .Where(c => c.Date > user.LastForumMessageDate)
                        .OrderBy(c => c.Date)
                        .ToList();
                    foreach(var comment in newComments)
                    {
                        SendMesssage(user.ChatId, comment);
                    }

                }
                else
                {
                    SendMesssage(user.ChatId, comments.Last());
                }
                user.LastForumMessageDate = comments.Last().Date;
            }
            context.SaveChanges();
        }

        private Telegram.Bot.Types.Message SendMesssage(long chatId, Models.ForumComment commment)
        {
            return _client.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"<b>{commment.Date:HH:mm}</b> {commment.Message}",
                    replyMarkup: new ReplyKeyboardRemove(),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    ).Result;
        }
    }
}
