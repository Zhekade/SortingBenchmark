using Microsoft.AspNetCore.Mvc;
using BenchmarkWeb.Services;

namespace BenchmarkWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BenchmarkController : ControllerBase
    {
        private readonly BenchmarkService _service;

        public BenchmarkController(BenchmarkService service)
        {
            _service = service;
        }

        [HttpPost("start")]
        public IActionResult Start([FromBody] BenchmarkConfig config)
        {
            if (_service.IsRunning) return BadRequest("Benchmark is already running");
            
            _service.StartBenchmark(config);
            return Ok();
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            _service.StopBenchmark();
            return Ok();
        }

        [HttpGet("stream")]
        public async Task Stream()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            var cancellationToken = HttpContext.RequestAborted;

            await foreach (var ev in _service.GetEventStream(cancellationToken))
            {
                await Response.WriteAsync($"data: {ev}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
    }
}
