using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz.Spi;
using System;
using System.IO;
using System.Threading.Tasks;
using WSM.Client.Configuration;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.Client
{
    class Program
    {

        static async Task Main(string[] args)
        {
            try
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                IHost host = Host.CreateDefaultBuilder(args)
                    .UseWindowsService()
                    .UseSystemd()
                    .ConfigureServices((context, services) =>
                    {
                        ConfigureServices(services);
                    }).Build();

                try
                {
                    // test server connection
                    var apiClient = host.Services.GetRequiredService<WSMApiClient>();
                    await apiClient.Ping();
                }
                catch (Exception ex)
                {
                    host.Services.GetRequiredService<ILogger<Program>>().LogError(ex, "Could not connect to server connection, is it running?");
                }
                await host.RunAsync();
            }
            catch(Exception ex)
            {
                File.WriteAllText("log.txt",ex.Message);
            }


        }
        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var config = LoadConfiguration();
            services.AddSingleton(config);
            services.DiscoverHealthChecks(config);

            var appConfig = AppConfigurationLoader.Load();
            services.AddSingleton(appConfig);


            services.AddSingleton<IJobFactory>(provider =>
            {
                var jobFactory = new JobsFactory(provider);
                return jobFactory;
            });

            services.AddTransient((sp) =>
            {
                return new WSMApiClient(appConfig.Server.Url, appConfig.Server.ApiKey);
            });
            services.AddHostedService<WindowService>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddNLog();
            });
        }
    }
}
