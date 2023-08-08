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
using WSM.Client.Models;

namespace WSM.Client
{
    internal class WindowService
    {
        private readonly IJobFactory _jobFactory;
        private readonly AppConfiguration _appConfiguration;
        private readonly ILogger _logger;
        private IScheduler scheduler;

        public WindowService(ILogger<WindowService> logger, IJobFactory jobfactory, AppConfiguration appConfiguration)
        {
            _logger = logger;
            _jobFactory = jobfactory;
            _appConfiguration = appConfiguration;
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
            foreach (var healthCheck in _appConfiguration.HealthChecks)
            {
                switch (healthCheck.Type)
                {

                    case HealthCheckType.HeartBeat:
                        RegisterHeatbeatJob(healthCheck as HeartBeatHealthCheckDefinition);
                        break;

                    case HealthCheckType.Process:
                        RegisterProcessJob(healthCheck as ProcessHealthCheckDefinition);
                        break;

                    case HealthCheckType.Port:
                        RegisterPortJob(healthCheck as TcpPortHealthCheckDefinition);
                        break;
                    case HealthCheckType.DockerContainer:
                        RegisterDockerContainerJob(healthCheck as DockerContainerHealthCheckDefinition);
                        break;

                }
            }
        }

        private void RegisterHeatbeatJob(HeartBeatHealthCheckDefinition healthCheck)
        {
            IJobDetail job = JobBuilder.Create<HeartbeatHealthCheckJob>()
                      .WithIdentity("Heatbeat", "group1")
                      .Build();
            job.JobDataMap.Add(HealthCheckJobBase.HealthCheckDataKey, healthCheck);

            ITrigger trigger = TriggerBuilder.Create()
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithInterval(TimeSpan.FromSeconds(5))
               .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void RegisterProcessJob(ProcessHealthCheckDefinition healthCheck)
        {
            IJobDetail job = JobBuilder.Create<ProcessHealthCheckJob>()
                      .WithIdentity($"Process Health Check {healthCheck.Name}", "group2")
                      .Build();
            job.JobDataMap.Add(HealthCheckJobBase.HealthCheckDataKey, healthCheck);
            ITrigger trigger = TriggerBuilder.Create()           
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithInterval(healthCheck.Interval)
               .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void RegisterPortJob(TcpPortHealthCheckDefinition healthCheck)
        {
            IJobDetail job = JobBuilder.Create<TcpPortHealthCheckJob>()
                      .WithIdentity($"Port Health Check {healthCheck.Name}", "group2")
                      .Build();
            job.JobDataMap.Add(HealthCheckJobBase.HealthCheckDataKey, healthCheck);
            ITrigger trigger = TriggerBuilder.Create()
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithInterval(healthCheck.Interval)
               .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void RegisterDockerContainerJob(DockerContainerHealthCheckDefinition healthCheck)
        {
            IJobDetail job = JobBuilder.Create<DockerHealthCheckJob>()
                      .WithIdentity($"Docker Container Health Check {healthCheck.Name}", "group2")
                      .Build();
            job.JobDataMap.Add(HealthCheckJobBase.HealthCheckDataKey, healthCheck);
            ITrigger trigger = TriggerBuilder.Create()
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithInterval(healthCheck.Interval)
               .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
    }
}
