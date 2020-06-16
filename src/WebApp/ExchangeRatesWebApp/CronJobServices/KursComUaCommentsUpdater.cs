using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRatesWebApp.CronJobServices.KursComUa;
using ExchangeRatesWebApp.Data;
using ExchangeRatesWebApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExchangeRatesWebApp.CronJobServices
{
    public class KursComUaCommentsUpdater : CronJobService
    {
        private readonly string url = "https://kurs.com.ua/ajax/getForum?limit=15&type=mb_comments&current_page=interbank";
        private readonly ILogger<KursComUaCommentsUpdater> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KursComUaCommentsUpdater(IScheduleConfig<KursComUaCommentsUpdater> config, ILogger<KursComUaCommentsUpdater> logger, IServiceScopeFactory serviceScopeFactory)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KursComUaCommentsUpdater starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} KursComUaCommentsUpdater is working.");
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(url, cancellationToken).Result;
            response.EnsureSuccessStatusCode();
            string responseStream = response.Content.ReadAsStringAsync().Result;
            ParseJson(responseStream);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KursComUaCommentsUpdater is stopping.");
            return base.StopAsync(cancellationToken);
        }

        private void ParseJson(string json)
        {
            ForumComments td = JsonSerializer.Deserialize<ForumComments>(json);
            List<ForumComment> comments = new List<ForumComment>();
            foreach(var comment in td.Data)
            {
                string content = comment.Content;
                comments.Add(new ForumComment() {
                    Date = comment.Date.ToLocalTime(),
                    Message = content.StripHtml(),
                    OriginalMessage = content
                });
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetService<DataContext>();
            int count = 0;
            foreach (ForumComment comment in comments)
            {
                if (!context.ForumComments.Any(c => c.Date == comment.Date))
                {
                    count++;
                    context.ForumComments.Add(comment);
                }
            }
            if (count > 0)
            {
                context.SaveChanges();
                _logger.LogInformation("KursComUaCommentsUpdater: Forum comments saved");
            }
            _logger.LogInformation($"KursComUaCommentsUpdater: {count} comments was added.");
        }
    }

    public static class StringExt
    {
        public static string StripHtml(this string input)
        {
            string result = Regex.Replace(input, "<.*?>", String.Empty);
            result = result.Replace('\n', ' ');
            result = result.Replace('\r', ' ');
            result = result.Replace('\t', ' ');
            return result;
        }
    }
}