using System;

namespace Library.CommonModels
{
    public class RabbitMQConnection
    {
        public String UserName { get; set; }
        public String Password { get; set; }
        public String Vhost { get; set; }
        public String HostName { get; set; }
        public Int16 Port { get; set; }
    }
}