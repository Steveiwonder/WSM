using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz.Spi;
using System;
using System.IO;
using Topshelf;
using WSM.Client.Configuration;
using WSM.Client.Jobs;

namespace WSM.Client
{
    class Program
    {
        private static IConfiguration config { get; set; }
        static void Main(string[] args)
        {
            config = LoadConfiguration();
            var services = ConfigureServices();
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
               .AddJsonFile("appsettings.json", optional: true,
                            reloadOnChange: true);
            return builder.Build();
        }
        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(config);
            services.Configure<ServiceConfig>(config.GetSection(ServiceConfig.serviceConfig));
            services.Configure<QuartzConfig>(config.GetSection(QuartzConfig.quartzConfig));
            services.AddSingleton<IJobFactory>(provider =>
            {
                var jobFactory = new JobsFactory(provider);
                return jobFactory;
            });
            services.AddSingleton<WindowServiceJob>();
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
