using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WSM.Client.Models
{

    public enum HealthCheckType
    {
        HeartBeat,
        Process,
        Port,
        DockerContainer
    }
}
