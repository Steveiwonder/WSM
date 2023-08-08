using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz.Spi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Topshelf;
using WSM.Client.Configuration;
using WSM.Client.Jobs;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client
{
    class Program
    {
        private static IConfiguration config { get; set; }
        private static AppConfiguration appConfig { get; set; }
        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            config = LoadConfiguration();
            appConfig = AppConfigurationLoader.Load();
            if (appConfig.HealthChecks.Count(d => d.Type == HealthCheckType.HeartBeat) != 1)
            {
                throw new Exception("Must have a single heartbeat check defined");
            }
            var services = ConfigureServices();

            services.AddSingleton(appConfig);
            var serviceProvider = services.BuildServiceProvider();
            var serviceData = config.GetSection(ServiceConfig.serviceConfig).Get<ServiceConfig>();

            HostFactory.Run(serviceconfig =>
            {
                serviceconfig.Service<WindowService>(serviceInstance =>
                {
                    serviceInstance.ConstructUsing(() => serviceProvider.GetService<WindowService>());
                    serviceInstance.WhenStarted(s => s.Start());
                    serviceInstance.WhenStopped(s => s.Stop());
                }
                );
                serviceconfig.SetServiceName(serviceData.ServiceName);
                serviceconfig.SetDisplayName(serviceData.DisplayName);
                serviceconfig.SetDescription(serviceData.Description);
                serviceconfig.StartAutomatically();
                serviceconfig.EnableServiceRecovery(recoveryOption =>
                {
                    recoveryOption.RestartService(TimeSpan.FromSeconds(10));

                });
            });

        }
        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(config);
            services.Configure<ServiceConfig>(config.GetSection(ServiceConfig.serviceConfig));
            services.AddSingleton<IJobFactory>(provider =>
            {
                var jobFactory = new JobsFactory(provider);
                return jobFactory;
            });

            services.AddTransient<WSMApiClient>((sp) =>
            {

                return new WSMApiClient(appConfig.Server.Url, appConfig.Server.ApiKey);
            });
            services.AddSingleton<HeartbeatHealthCheckJob>();
            services.AddSingleton<ProcessHealthCheckJob>();
            services.AddSingleton<TcpPortHealthCheckJob>();
            services.AddSingleton<DockerHealthCheckJob>();
            services.AddSingleton<DiskSpaceHealthCheckJob>();
            services.AddSingleton<WindowService>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddNLog();
            });
            return services;
        }
    }
}
