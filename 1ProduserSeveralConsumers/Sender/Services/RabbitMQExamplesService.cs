using System.Threading.Tasks;
using Sender.Services.Interfaces;
using Library;
using Library.CommonModels;
using RabbitMQ.Client;
using System.Text;

namespace Sender.Services
{
    public class RabbitMQExamplesService : IRabbitMQExamplesService
    {
        private readonly RabbitMQConnection _rabbitMQConnection;
        public RabbitMQExamplesService(RabbitMQConnection connection)
        {
            _rabbitMQConnection = connection;
        }
        public Task<string> Example1(string message)
        {
            return Task<string>.Run(() =>
            {
                var queueName = "Task_Queue";
                var factory = new ConnectionFactory()
                {
                    UserName = _rabbitMQConnection.UserName,
                    Password = _rabbitMQConnection.Password,
                    VirtualHost = _rabbitMQConnection.Vhost,
                    HostName = _rabbitMQConnection.HostName,
                    Port = _rabbitMQConnection.Port             
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                //создание очереди, устанавливаем флаг durable
                channel.QueueDeclare(queueName, true, false, false, null);

                var messageInBytes = Encoding.UTF8.GetBytes(message);

                //Для канала(сообщения) создали свойство 
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                //паблишим сообщение в очередь
                channel.BasicPublish("", queueName, properties, messageInBytes);
                System.Console.WriteLine($"message sended in queue");
                return "message Sended";
            });
        }
    }
}