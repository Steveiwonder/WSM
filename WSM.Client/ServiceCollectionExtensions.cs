using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WSM.Client.Configuration;
using WSM.Client.Models;

namespace WSM.Client
{
    internal static class ServiceCollectionExtensions
    {
        private static readonly string HealthCheckDirectory = Path.Combine(Directory.GetCurrentDirectory(), "HealthChecks");
        public static void DiscoverHealthChecks(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            string[] files = Directory.GetFiles(HealthCheckDirectory, "*.dll");
            foreach (var item in files)
            {
                var healthCheckTypes = TryGetHealthChecks(item);
                foreach (var healthCheckType in healthCheckTypes)
                {
                    var healthCheck = Activator.CreateInstance(healthCheckType) as IHealthCheckDefinition;
                    serviceCollection.AddSingleton<IHealthCheckDefinition>(healthCheck);
                    serviceCollection.AddSingleton(healthCheck.JobType);
                    AppConfigurationLoader.AddMapping(healthCheck.Type, healthCheck.ConfigType);
                    healthCheck.Initialize(serviceCollection, configuration);
                }
            }
        }

        private static IEnumerable<Type> TryGetHealthChecks(string dllFile)
        {
            try
            {
                var assembly = Assembly.LoadFile(dllFile);
                var types = assembly.GetTypes();
                return assembly.GetTypes().Where(t => t.BaseType != null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() == typeof(HealthCheckDefinitionBase<,>));
            }
            catch
            {

            }
            return Array.Empty<Type>();
        }
        
    }
}
