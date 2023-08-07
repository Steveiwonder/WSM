using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WSM.Client.Jobs
{
    public class WindowServiceJob : IJob
    {
        private readonly ILogger _logger;
        public WindowServiceJob(ILogger<WindowServiceJob> logger)
        {
            _logger = logger;
        }
        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Job Started");
            return Task.CompletedTask;
        }
    }
}
