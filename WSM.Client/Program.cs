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
using WSM.Shared;

namespace WSM.Client
{
    class Program
    {

        static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            
            var services = Configure();
       
            var serviceProvider = services.BuildServiceProvider();
            var serviceData = serviceProvider.GetRequiredService<IConfiguration>().GetSection(ServiceConfig.serviceConfig).Get<ServiceConfig>();

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

        private static IServiceCollection Configure()
        {
            IServiceCollection services = new ServiceCollection();
            var config = LoadConfiguration();
            services.AddSingleton(config);
            services.DiscoverHealthChecks(config);

            var appConfig = AppConfigurationLoader.Load();
            services.AddSingleton(appConfig);
            
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
