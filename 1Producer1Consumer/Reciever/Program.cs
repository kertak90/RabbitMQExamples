using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Library.CommonModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reciever
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine($"started");
            var queueName = "TestQueue";
            var settings = GetSettings("appsettings.json");
            var _rabbitMQJson = settings["RabbitMqConnection"].ToString();

            var _rabbitMQConnection = JsonConvert.DeserializeObject<RabbitMQConnection>(_rabbitMQJson);
            if(_rabbitMQConnection is null)
                throw new DataMisalignedException($"RabbitMqConnection data is null");

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

            //Создание очереди, т.к. мы не уверены что очерердь уже существует
            channel.QueueDeclare(queueName, false, false, false, null);

            //Создаем получателя
            var consumer = new EventingBasicConsumer(channel);

            //подписываем получателя на лямбду, которая выводит сообщение в консоль.
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                System.Console.WriteLine($"{message}");
            };

            //указываем название очереди к котрой будет привязан получатель
            channel.BasicConsume(queueName, true, consumer);
            System.Console.WriteLine($"press Enter");
            Console.ReadLine();
        }
        static Dictionary<string, object> GetSettings(string fileName)
        {
            if(!File.Exists(fileName))
                throw new FileNotFoundException($"file not found: {fileName}");
            var json = File.ReadAllText(fileName);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dictionary;
        }
    }
}
