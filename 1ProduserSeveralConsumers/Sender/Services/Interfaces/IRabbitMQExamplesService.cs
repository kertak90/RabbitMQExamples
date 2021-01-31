using System.Threading.Tasks;

namespace Sender.Services.Interfaces
{
    public interface IRabbitMQExamplesService
    {
        Task<string> Example1(string message);
    }
}