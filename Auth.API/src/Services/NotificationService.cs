using RabbitMQ.Client;


using System.Text;
using Newtonsoft.Json;
using Auth.API.Models;
using System.Threading.Tasks;

namespace Auth.API.Services;

public class NotificationService : INotificationService
{

    public async Task Send(Message message)
    {
        var messageBrokerHost = Environment.GetEnvironmentVariable("MESSAGE_BROKER_HOST");
        var factory = new ConnectionFactory { HostName = messageBrokerHost };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
        {
            await channel.QueueDeclareAsync(queue: "notification",
                  durable: false,
                  exclusive: false,
                  autoDelete: false,
                  arguments: null);

            string messageSerialize = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(messageSerialize);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "notification", body: body);
        }
    }
}