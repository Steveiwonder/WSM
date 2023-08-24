using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
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

        public async Task ClearHealthChecks()
        {
            await Delete("healthcheck/clear");
        }

        public async Task Ping()
        {
            await Get("ping");
        }
        private async Task<HttpStatusCode> Delete(string path)
        {
            var requestMessage = CreateHttpRequestMessage(HttpMethod.Delete, path);
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
            return httpResponse.StatusCode;
        }

        private async Task<HttpStatusCode> Post(object any, string path)
        {
            var requestMessage = CreateHttpRequestMessage(HttpMethod.Post, path);

            var content = JsonContent.Create(any);
            requestMessage.Content = content;
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
            return httpResponse.StatusCode;
        }
        private async Task<T> Get<T>(string path)
        {
            var requestMessage = CreateHttpRequestMessage(HttpMethod.Get, path);
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(requestMessage);
            return await httpResponse.Content.ReadFromJsonAsync<T>();
        }

        public async Task Get(string path)
        {
            var requestMessage = CreateHttpRequestMessage(HttpMethod.Get, path);
            await _httpClient.SendAsync(requestMessage);
            
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string path)
        {
            string uri = BuildUri(path);
            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Headers.Add("Authorization", _apiKey);
            return requestMessage;
        }

        private string BuildUri(string path)
        {
            var uriBuilder = new UriBuilder(_url);
            uriBuilder.Path = path;
            return uriBuilder.ToString();
        }
    }
}
