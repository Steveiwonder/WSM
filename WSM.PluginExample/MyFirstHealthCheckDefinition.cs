using WSM.Client.Models;

namespace WSM.PluginExample
{
    public class MyFirstHealthCheckDefinition : HealthCheckDefinitionBase<MyFirstHealthCheckJob, MyFirstHealthCheckConfiguration>
    {
        public override string Type => "MyFirstHealthCheck";
    }
}
