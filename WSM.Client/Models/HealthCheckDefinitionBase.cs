namespace WSM.Client.Models
{
    public abstract class HealthCheckDefinitionBase
    {
        public string Name { get; set; }
        public HealthCheckType Type { get; set; }
    }
}
