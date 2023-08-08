using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSM.Server.Services;
using WSM.Shared.Dtos;

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
            _healthCheckService.CheckIn(healthCheckReport);
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_healthCheckService.GetStatus());
        }
    }
}
