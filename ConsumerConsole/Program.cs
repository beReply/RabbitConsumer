using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";
            factory.UserName = "guest";
            factory.Password = "guest";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("Queue",
                        false, false, false, null);

                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume("Queue", false, consumer);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.Span;
                        var message = Encoding.UTF8.GetString(body);
                        Thread.Sleep(1000);
                        Console.WriteLine("已接收： {0}", message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };
                    Console.ReadLine();
                }
            }
        }
    }
}
