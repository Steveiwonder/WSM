using Microsoft.Extensions.Logging;
using Quartz;
using WSM.Client.Jobs;
using WSM.Shared;
using WSM.PluginExample;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WSM.HealthChecks.FreeMemoryHealthCheck
{
    [DisallowConcurrentExecution]
    public class FreeMemoryHealthCheckJob : HealthCheckJobBase
    {
        private readonly ILogger<FreeMemoryHealthCheckJob> _logger;
        private const string MemoryLowStatus = "Free memory low";
        public FreeMemoryHealthCheckJob(ILogger<FreeMemoryHealthCheckJob> logger, WSMApiClient apiClient) : base(apiClient, logger)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            var healthCheckConfiguration = GetConfiguration<FreeMemoryHealthCheckConfiguration>(context);
            try
            {
                var freeSystemMemory = GetFreeSystemMemory();

                if (freeSystemMemory < healthCheckConfiguration.MinimumFreeMemory)
                {
                    string mbAvailable = FormatUtils.SizeSuffix(freeSystemMemory);
                    await CheckIn(healthCheckConfiguration, $"{MemoryLowStatus}, {mbAvailable} available");
                    return;
                }

                await CheckIn(healthCheckConfiguration, Constants.AvailableStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }

        private static long GetFreeSystemMemory()
        {            
            return MemoryMetricsClient.GetMetrics().Free;
        }

        public class MemoryMetrics
        {
            public long Total;
            public long Used;
            public long Free;
        }

        public static class MemoryMetricsClient
        {
            public static MemoryMetrics GetMetrics()
            {
                if (IsUnix())
                {
                    return GetUnixMetrics();
                }

                return GetWindowsMetrics();
            }

            private static bool IsUnix()
            {
                var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                             RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

                return isUnix;
            }

            private static MemoryMetrics GetWindowsMetrics()
            {
                var output = "";

                var info = new ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                }

                var lines = output.Trim().Split("\n");
                var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
                var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics
                {
                    Total = long.Parse(totalMemoryParts[1]) * 1000,
                    Free = long.Parse(freeMemoryParts[1]) * 1000
                };
                metrics.Used = metrics.Total - metrics.Free;

                return metrics;
            }

            private static MemoryMetrics GetUnixMetrics()
            {
                var output = "";

                var info = new ProcessStartInfo("free -m")
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"free -m\"",
                    RedirectStandardOutput = true
                };

                using (var process = Process.Start(info))
                {
                    output = process.StandardOutput.ReadToEnd();
                    Console.WriteLine(output);
                }

                var lines = output.Split("\n");
                var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

                var metrics = new MemoryMetrics
                {
                    Total = long.Parse(memory[1]),
                    Used = long.Parse(memory[2]),
                    Free = long.Parse(memory[3])
                };

                return metrics;
            }
        }
    }
}
