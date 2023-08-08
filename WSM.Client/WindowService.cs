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
using System.Threading.Tasks;
using WSM.Client.Configuration;
using WSM.Client.Jobs;
using WSM.Client.Models;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Client
{
    internal class WindowService
    {
        private readonly IJobFactory _jobFactory;
        private readonly AppConfiguration _appConfiguration;
        private readonly WSMApiClient _client;
        private readonly ILogger _logger;
        private IScheduler scheduler;

        public WindowService(ILogger<WindowService> logger, IJobFactory jobfactory, AppConfiguration appConfiguration, WSMApiClient client)
        {
            _logger = logger;
            _jobFactory = jobfactory;
            _appConfiguration = appConfiguration;
            _client = client;
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
                        RegisterJob<HeartbeatHealthCheckJob>(healthCheck, Constants.DefaultInterval);
                        break;

                    case HealthCheckType.Process:
                        RegisterJob<ProcessHealthCheckJob>(healthCheck, (healthCheck as ProcessHealthCheckDefinition).Interval);
                        break;

                    case HealthCheckType.Port:
                        RegisterJob<TcpPortHealthCheckJob>(healthCheck, (healthCheck as TcpPortHealthCheckDefinition).Interval);
                        break;

                    case HealthCheckType.DockerContainer:
                        RegisterJob<DockerHealthCheckJob>(healthCheck, (healthCheck as DockerContainerHealthCheckDefinition).Interval);
                        break;

                    case HealthCheckType.DiskSpace:
                        RegisterJob<DiskSpaceHealthCheckJob>(healthCheck, (healthCheck as DiskSpaceHealthCheckDefinition).Interval);
                        break;
                 

                }
            }
        }



        private void RegisterJob<T>(HealthCheckDefinitionBase healthCheckDefinition, TimeSpan interval) where T : HealthCheckJobBase
        {
            if (interval < TimeSpan.Zero)
            {
                interval = Constants.DefaultInterval;
            }
            IJobDetail job = JobBuilder.Create<T>()
                      .WithIdentity(healthCheckDefinition.Name, "group1")
                      .Build();
            job.JobDataMap.Add(HealthCheckJobBase.HealthCheckDataKey, healthCheckDefinition);

            ITrigger trigger = TriggerBuilder.Create()
           .StartNow()
           .WithSimpleSchedule(x => x
               .WithInterval(interval)
               .RepeatForever())
           .Build();

            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }

    }
}
