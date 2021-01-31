using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
            var queueName = "Task_Queue";
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
            //Создание очереди, т.к. мы не уверены что очерердь уже существует.
            //также устанавливаем флаг durable в true
            channel.QueueDeclare(queueName, true, false, false, null);

            //Устанавливаем параметр не больше одной задачи  за раз
            //Qos - quality of service
            //PrefetchSize - сколько сообщений отправить заранее
            //PrefetchCOunt - не более 1 сооббщения на воркер
            //global = false - эта настройка распространяется на всех консьюмеров
            channel.BasicQos(0, 1, false);

            //Создаем получателя
            var consumer = new EventingBasicConsumer(channel);

            //подписываем получателя на лямбду, которая выводит сообщение в консоль.
            consumer.Received += OnReceived;
            
            void OnReceived(object o, BasicDeliverEventArgs ea)
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                System.Console.WriteLine($"{message}");
                DoHardWork(message);
                //Даем каналу знать что сообщение успешно отработало
                //multiple - возможность отметить несколько сообщеинй
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }

            //указываем название очереди к котрой будет привязан получатель
            //AutoAck = true - сообшение после получения сервисом автоматически помечается на удаление
            //AutoAck(Manual Ack) = false - когда работа выполнена, то сообщение нужно вызвать BasickAck
            //чтобы удалить вручную ссобщение из очереди
            channel.BasicConsume(queueName, false, consumer);
            System.Console.WriteLine($"press Enter");
            Console.ReadLine();
        }
        static void DoHardWork(string message)
        {
            int dots = message.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);
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
