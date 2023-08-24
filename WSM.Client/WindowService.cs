using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WSM.Client.Configuration;
using WSM.Client.Jobs;
using WSM.Client.Models;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Client
{
    internal class WindowService : BackgroundService
    {
        private readonly IJobFactory _jobFactory;
        private readonly AppConfiguration _appConfiguration;
        private readonly WSMApiClient _client;
        private readonly IEnumerable<IHealthCheckDefinition> _healthCheckDefinitions;
        private readonly ILogger _logger;
        private IScheduler scheduler;

        public WindowService(ILogger<WindowService> logger, IJobFactory jobfactory, AppConfiguration appConfiguration, WSMApiClient client,
            IEnumerable<IHealthCheckDefinition> healthCheckDefinitions)
        {
            _logger = logger;
            _jobFactory = jobfactory;
            _appConfiguration = appConfiguration;
            _client = client;
            _healthCheckDefinitions = healthCheckDefinitions;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Started");
            try
            {
                _client.ClearHealthChecks().Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to clear health checks");
            }
            await StartSchedulerAsync();
            StartJobs();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
        private async Task StartSchedulerAsync()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" },
                { "quartz.scheduler.instanceName", "MyScheduler" },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.threadPool.threadCount", "10" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            scheduler = factory.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
            scheduler.JobFactory = _jobFactory;
            await scheduler.Start();
        }
        private void StartJobs()
        {
            if(_appConfiguration.HealthChecks==null || !_appConfiguration.HealthChecks.Any())
            {
                _logger.LogWarning("No health checks defined");
                return;
            }
            foreach (var healthCheckConfig in _appConfiguration.HealthChecks)
            {
                var definition = _healthCheckDefinitions.FirstOrDefault(hcd => hcd.Type.Equals(healthCheckConfig.Type, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                {
                    continue;
                }
                RegisterJob(definition, healthCheckConfig);

            }
        }



        private void RegisterJob(IHealthCheckDefinition healthCheckDefinition, HealthCheckConfigurationBase healthCheckConfiguration)
        {
            var interval = healthCheckConfiguration.Interval;
            if (interval < TimeSpan.Zero)
            {
                interval = Constants.DefaultInterval;
            }
            IJobDetail job = JobBuilder.Create(healthCheckDefinition.JobType)
                      .WithIdentity(healthCheckConfiguration.Name, "group1")
                      .Build();
            job.JobDataMap.Add(HealthCheckJobBase.HealthCheckDataKey, healthCheckConfiguration);

            ITrigger trigger = TriggerBuilder.Create()
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithInterval(interval)
               .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {          
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
