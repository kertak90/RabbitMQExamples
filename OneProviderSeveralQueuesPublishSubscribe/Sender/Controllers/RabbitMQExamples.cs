using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sender.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Sender.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class RabbitMQExamples : Controller
    {
        private readonly IRabbitMQExamplesService _rabbitMQExamplesService;
        public RabbitMQExamples(IRabbitMQExamplesService rabbitMQExamplesService)
        {
            _rabbitMQExamplesService = rabbitMQExamplesService;
        }

        [HttpPost("[action]")]
        [SwaggerOperation(Description = "example with one sender and several reciever of one type")]
        public async Task<string> Example1([FromQuery] string message) =>
            await _rabbitMQExamplesService.Example1(message);
    }
}