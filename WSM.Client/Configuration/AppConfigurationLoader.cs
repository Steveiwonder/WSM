
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WSM.Client.Models;

namespace WSM.Client.Configuration
{
    internal static class AppConfigurationLoader
    {
        internal static AppConfiguration Load()
        {
            
            using (Stream stream = File.OpenRead("appsettings.json"))
            {
                using (TextReader textReader = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(textReader))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Converters.Add(new TypeSelectorConverter<HealthCheckDefinitionBase>(new Dictionary<string, Type>()
                        {
                            {"Heartbeat", typeof(HeartBeatHealthCheckDefinition) },
                            {"Process", typeof(ProcessHealthCheckDefinition) },
                            {"Port", typeof(TcpPortHealthCheckDefinition) },
                            {"DockerContainer", typeof(DockerContainerHealthCheckDefinition) },
                        }));


                        var root = serializer.Deserialize<ConfigurationRoot>(jsonTextReader);



                        return root.App;
                    }
                }
            }
        }

        private class ConfigurationRoot
        {
            public AppConfiguration App { get; set; }
        }
    }
}
