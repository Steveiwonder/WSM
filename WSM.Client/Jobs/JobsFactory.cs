using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;

namespace WSM.Client.Jobs
{
    public class JobsFactory : IJobFactory
    {
        private readonly IServiceProvider _provider;

        public JobsFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var job = _provider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            if (job == null)
            {
                throw new NotSupportedException($"{bundle.JobDetail.JobType.Name} is not supported!");
            }

            return job;
        }

        public void ReturnJob(IJob job)
        {
            if (job is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
