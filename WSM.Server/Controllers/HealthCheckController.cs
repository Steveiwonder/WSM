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

        [HttpPost("CheckIn")]
        public IActionResult CheckIn(HealthCheckReportDto healthCheckReport)
        {
            if (!_healthCheckService.CheckIn(healthCheckReport))
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_healthCheckService.GetStatus().Select(hc =>
            {
                return new HealthCheckStatusDto()
                {
                    BadStatusCount = hc.BadStatusCount,
                    BadStatusLimit = hc.HealthCheck.BadStatusLimit,
                    CheckInInterval = hc.HealthCheck.CheckInInterval,
                    LastBadStatusAlertSent = hc.LastBadStatusAlertSent,
                    LastMissedCheckInAlertSent = hc.LastMissedCheckInAlertSent,
                    LastCheckInTime = hc.LastCheckInTime,
                    LastStatus = hc.LastStatus,
                    MissedCheckInCount = hc.MissedCheckInCount,
                    MissedCheckInLimit = hc.HealthCheck.BadStatusLimit,
                    Name = hc.Name,
                    NextCheckInTime = hc.NextCheckInTime,
                    NextStatusCheckTime = hc.NextStatusCheckTime,
                };
            }));
        }

        [HttpDelete("clear")]
        public IActionResult Clear()
        {
            _healthCheckService.ClearHealthChecks();
            return Ok();
        }

        [HttpPost("Register")]
        public IActionResult Register(HealthCheckRegistrationDto healthCheckRegistration)
        {
            _healthCheckService.Register(healthCheckRegistration);
            return Ok();
        }
    }
}
