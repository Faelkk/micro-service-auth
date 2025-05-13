using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Notification.Service.Models;
using Notification.Service.Service;
using System.Threading.Tasks;
using System.Threading;

namespace Notification.Service
{
    public class Program
    {
        private static AutoResetEvent waitHandle = new AutoResetEvent(false);

        public static async Task Main(string[] args)
        {
        Inicio:
            try
            {
                var MessageBrokerHost = Environment.GetEnvironmentVariable("MESSAGE_BROKER_HOST");
                if (string.IsNullOrEmpty(MessageBrokerHost))
                {
                    Console.WriteLine("Error: MESSAGE_BROKER_HOST environment variable is not set.");
                    return;
                }
                var factory = new ConnectionFactory { HostName = MessageBrokerHost };
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: "notification",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                Console.WriteLine("Waiting for new notifications.");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    Message message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(body));
                    if (message == null)
                    {
                        Console.WriteLine("Error: Could not deserialize message.");
                        return;
                    }
                    try
                    {
                        EmailService.Send(message);
                        Console.WriteLine("Mail sent");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Mail failed");
                    }
                };
                await channel.BasicConsumeAsync(queue: "notification",
                                         autoAck: true,
                                         consumer: consumer);

                Console.CancelKeyPress += (o, e) =>
                {
                    Console.WriteLine("Exit...");
                    waitHandle.Set();
                };

                waitHandle.WaitOne();
            }
            catch (Exception)
            {
                Console.WriteLine("Error on connect to rabbitmq");
                Console.WriteLine("Trying reconnect");
                Thread.Sleep(5000);
                goto Inicio;
            }
        }
    }
}