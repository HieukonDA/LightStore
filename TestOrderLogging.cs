using Serilog;
using Microsoft.AspNetCore.Mvc;

namespace TheLightStore.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestLoggingController : ControllerBase
    {
        private static readonly Serilog.ILogger OrderLogger = Log.ForContext("OrderProcess", true);

        [HttpGet("order-log")]
        public IActionResult TestOrderLogging()
        {
            OrderLogger.Information("=== ORDER PROCESS TEST: File logging is working ===");
            OrderLogger.Information("Testing ORDER PROCESS logging to file at {Timestamp}", DateTime.UtcNow);
            
            return Ok(new { message = "ORDER PROCESS log written to file. Check logs/order-process-*.txt" });
        }
    }
}