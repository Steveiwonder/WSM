using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net.Http;

namespace WSM.Client.Models
{
    public class HttpRequestHealthCheckDefinition : HealthCheckDefinitionBase
    {
        public string Url { get; set; }
        public HttpMethod Method = HttpMethod.Options;
        public int ExpectedStatusCode { get; set; }
        public TimeSpan? MaxResponseDuration { get; set; }
        public string RequestBody { get; set; }
        public string ExpectedResponseBody { get; set; }



    }
}
