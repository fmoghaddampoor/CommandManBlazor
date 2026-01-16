using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CommandMan.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;

        public LogController(ILogger<LogController> logger)
        {
            _logger = logger;
        }

        public record LogEntry(string Level, string Message, object? Args);

        [HttpPost]
        public IActionResult Log([FromBody] LogEntry entry)
        {
            using (LogContext.PushProperty("LogSource", "Frontend"))
            {
                var message = $"{entry.Message}";
                
                switch (entry.Level?.ToLower())
                {
                    case "error":
                        _logger.LogError(message, entry.Args);
                        break;
                    case "warn":
                    case "warning":
                        _logger.LogWarning(message, entry.Args);
                        break;
                    case "info":
                    case "log":
                    default:
                        _logger.LogInformation(message, entry.Args);
                        break;
                }
            }
            return Ok();
        }
    }
}
