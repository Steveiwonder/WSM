﻿using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
{
    [DisallowConcurrentExecution]
    public class HttpRequestHealthCheckJob : HealthCheckJobBase
    {
        private readonly ILogger<HttpRequestHealthCheckJob> _logger;

        public HttpRequestHealthCheckJob(ILogger<HttpRequestHealthCheckJob> logger, WSMApiClient apiClient) : base(apiClient)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var healthCheckDefinition = GetDefinition<HttpRequestHealthCheckDefinition>(context);
                var status = await ExecuteRequest(healthCheckDefinition);
                await CheckIn(healthCheckDefinition, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }

        public  async Task<string> ExecuteRequest(HttpRequestHealthCheckDefinition definition)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(definition.Method, definition.Url);
                using(var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(definition.RequestBody))
                    {
                        requestMessage.Content = new StringContent(definition.RequestBody);
                    }
                    if(definition.MaxResponseDuration != null)
                    {
                        client.Timeout = definition.MaxResponseDuration.Value;
                    }
                    var response = await client.SendAsync(requestMessage);
                    if((int)response.StatusCode != definition.ExpectedStatusCode)
                    {
                        return $"Invalid status code. Expected {definition.ExpectedStatusCode}, got {(int)response.StatusCode}";
                    }
                    if(!string.IsNullOrEmpty(definition.ExpectedResponseBody))
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (!responseBody.Equals(definition.ExpectedResponseBody))
                        {
                            return $"Invalid response, expected '{definition.ExpectedResponseBody}' got '{responseBody}'";
                        }
                    }
                    return Constants.AvailableStatus;
                }
            }
            catch(Exception ex) when (ex.InnerException is TimeoutException)
            {
                return $"Request timed out, {ex.Message}";
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "ExecuteRequest failed");
                return $"Unexpected error, {ex.Message}";
            }
        }
    }
}
