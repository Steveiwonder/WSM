using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSM.Server.Dtos;
using WSM.Server.Services;

namespace WSM.Server.Controllers
{

    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly WSMHealthCheckService _healthCheckService;

        public HealthCheckController(WSMHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [HttpPost]
        public void Post(HealthCheckReportDto healthCheckReport)
        {

            _healthCheckService.CheckIn(healthCheckReport.HealthCheckName);
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_healthCheckService.GetStatus());
        }
    }
}
