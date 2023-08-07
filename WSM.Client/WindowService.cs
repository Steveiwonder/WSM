using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using WSM.Client.Configuration;
using WSM.Client.Jobs;

namespace WSM.Client
{
    public class WindowService
    {
        private readonly IJobFactory _jobFactory;
        private readonly ILogger _logger;
        private IScheduler scheduler;
        private readonly QuartzConfig _quartzConfig;

        public WindowService(ILogger<WindowService> logger, IJobFactory jobfactory, IOptions<QuartzConfig> options)
        {
            _logger = logger;
            _jobFactory = jobfactory;
            _quartzConfig = options.Value;
        }
        public void Start()
        {
            _logger.LogInformation("Service Started");
            StartScheduler();
            StartJobs();
        }
        public void Stop()
        {
            _logger.LogInformation("Service Stopped");
        }
        private void StartScheduler()
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
            scheduler.Start().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        private void StartJobs()
        {
            IJobDetail job1 = JobBuilder.Create<WindowServiceJob>()
                .WithIdentity("job1", "group1")
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(_quartzConfig.IntervalInMinute)
                    .RepeatForever())
                .Build();

            scheduler.ScheduleJob(job1, trigger1).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
