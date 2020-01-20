using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ExchangeRatesWebApp.CronJobServices
{
    public class InterBankUpdater : CronJobService
    {
        private readonly string url = "https://kurs.com.ua/ajax/getChart?size=big&type=interbank&currencies_from=usd&currencies_to=&organizations=&limit=&optimal=";
        private readonly ILogger<InterBankUpdater> _logger;

        public InterBankUpdater(IScheduleConfig<InterBankUpdater> config, ILogger<InterBankUpdater> logger)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 1 starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} CronJob 1 is working.");
            HttpClient client = new HttpClient();
            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var responseStream = response.Content.ReadAsStringAsync().Result;
            _logger.LogInformation(responseStream);
            //JsonSerializer.DeserializeAsync <IEnumerable<GitHubIssue>>(responseStream);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 1 is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}