﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WSM.Shared.Dtos;

namespace WSM.Shared
{
    public class WSMApiClient
    {
        private readonly string _url;
        private readonly string _apiKey;
        private HttpClient _httpClient = new HttpClient();
        public WSMApiClient(string url, string apiKey)
        {
            _url = url;
            _apiKey = apiKey;
        }

        public async Task<bool> CheckIn(string name, string status)
        {
            var dto = new HealthCheckReportDto()
            {
                Name = name,
                Status = status
            };

            var statusCode = await Post(dto, "healthcheck/checkin");
            if (statusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            return true;
        }   

        public async Task RegisterHealthCheck(HealthCheckRegistrationDto healthCheckRegistration)
        {
            await Post(healthCheckRegistration, "healthcheck/register");
        }

        private async Task<HttpStatusCode> Post(object any, string path)
        {
            var uri = BuildUri(path);
            var content = JsonContent.Create(any);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Content = content;
            requestMessage.Headers.Add("Authorization", _apiKey);
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
            return httpResponse.StatusCode;
        }

        private string BuildUri(string path)
        {
            var uriBuilder = new UriBuilder(_url);
            uriBuilder.Path = path;
            return uriBuilder.ToString();
        }
    }
}
