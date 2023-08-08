namespace WSM.Client.Models
{
    public class IISHealthCheckDefinition : HealthCheckDefinitionBase
    {
        public string SiteName { get; set; }

        public string Host { get; set; }
    }
}
