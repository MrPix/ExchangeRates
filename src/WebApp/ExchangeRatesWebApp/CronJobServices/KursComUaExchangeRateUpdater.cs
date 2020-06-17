using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRatesWebApp.CronJobServices.KursComUa;
using ExchangeRatesWebApp.Data;
using ExchangeRatesWebApp.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExchangeRatesWebApp.CronJobServices
{
    public class KursComUaExchangeRateUpdater : CronJobService
    {
        private const string Url = "https://kurs.com.ua/ajax/getChart?size=big&type=interbank&currencies_from=usd&currencies_to=&organizations=&limit=&optimal=";
        private readonly ILogger<KursComUaExchangeRateUpdater> _logger;
        readonly IServiceScopeFactory _serviceScopeFactory;
        private const string SourceName = "kurs.com.ua";
        
        public KursComUaExchangeRateUpdater(IScheduleConfig<KursComUaExchangeRateUpdater> config, ILogger<KursComUaExchangeRateUpdater> logger, IServiceScopeFactory serviceScopeFactory)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KursComUaExchangeRateUpdater starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now:hh:mm:ss} KursComUaExchangeRateUpdater is working.");
                HttpClient client = new HttpClient();
                var response = client.GetAsync(Url, cancellationToken).Result;
                response.EnsureSuccessStatusCode();
                var responseStream = response.Content.ReadAsStringAsync().Result;
                ParseJson(responseStream);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now:hh:mm:ss} KursComUaExchangeRateUpdater something was wrong");
                _logger.LogError(ex.Message);
            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("KursComUaExchangeRateUpdater is stopping.");
            return base.StopAsync(cancellationToken);
        }

        private void ParseJson(string json)
        {
            string view = "";
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };
            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                view = document.RootElement.GetProperty("view").GetString();
            }
            if (string.IsNullOrEmpty(view))
            {
                return;
            }

            TableData td = JsonSerializer.Deserialize<TableData>(view);
            List<ExchangeRate> rates = new List<ExchangeRate>();
            for (int i = 0; i < td.Series[0].Data.Count; i++)
            {
                var sell = td.Series[0].Data[i];
                var buy = td.Series[1].Data[i];
                var stringDateSell = sell[0].ToString();
                var stringDateBuy = buy[0].ToString();
                if (stringDateBuy != stringDateSell)
                {
                    _logger.LogWarning("KursComUaExchangeRateUpdater: Dates are not equal");
                    continue;
                }
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(stringDateSell));
                DateTime dateTime = dateTimeOffset.LocalDateTime;

                if (!double.TryParse(sell[1]?.ToString(), out double sellRate))
                {
                    _logger.LogWarning("KursComUaExchangeRateUpdater: Cannot parse sell value");
                    continue;
                }
                if (!double.TryParse(buy[1]?.ToString(), out double buyRate))
                {
                    _logger.LogWarning("KursComUaExchangeRateUpdater: Cannot parse buy value");
                    continue;
                }
                rates.Add(new Models.ExchangeRate() { Date = dateTime, SellRate = sellRate, BuyRate = buyRate, Source = SourceName });
            }
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                int count = 0;
                foreach (var rate in rates)
                {
                    if (!context.ExchangeRates.Any(m => m.Date == rate.Date && m.Source == SourceName))
                    {
                        count++;
                        context.ExchangeRates.Add(rate);
                    }
                }
                if (count > 0)
                {
                    context.SaveChanges();
                    _logger.LogInformation("KursComUaExchangeRateUpdater: DataSaved");
                }
                _logger.LogInformation($"KursComUaExchangeRateUpdater: {count} records was added.");
            }
        }
    }
}