using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using WSM.Client.Jobs;

namespace WSM.Client.Models
{
    public abstract class HealthCheckDefinitionBase<TJob, TConfig> : IHealthCheckDefinition
    {
        public abstract string Type { get;  }
        public Type JobType => typeof(TJob);
        public Type ConfigType => typeof(TConfig);
        public virtual void Initialize(IServiceCollection serviceCollection, IConfiguration configuration)
        {

        }
    }

    public interface IHealthCheckDefinition
    {
        public string Type { get;  }
        public Type JobType { get;  }
        public Type ConfigType { get; }

        public void Initialize(IServiceCollection serviceCollection, IConfiguration configuration);
    }

}
