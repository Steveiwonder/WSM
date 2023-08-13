
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
        private static Dictionary<string, Type> mappings = new Dictionary<string, Type>();
        internal static AppConfiguration Load()
        {
            
            using (Stream stream = File.OpenRead("appsettings.json"))
            {
                using (TextReader textReader = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(textReader))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Converters.Add(new HttpMethodConverter());
                        serializer.Converters.Add(new TypeSelectorConverter<HealthCheckConfigurationBase>(mappings));

                        var root = serializer.Deserialize<ConfigurationRoot>(jsonTextReader);

                        return root.App;
                    }
                }
            }
        }

        public static void AddMapping(string type, Type configurationType) 
        {
            mappings.Add(type, configurationType);
        }


        private class ConfigurationRoot
        {
            public AppConfiguration App { get; set; }
        }
    }
}
