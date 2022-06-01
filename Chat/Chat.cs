using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Chat;

public class Chat
{
    private IConnection connection;
    private IModel channel;
    private EventingBasicConsumer consumer;
    
    private Task asyncHandlerThread;
    
    private readonly Guid clientGuid;
    private readonly string clientName;
    
    private string baseQueue;
    private string _currentQueue;

    public Chat(string host, string queue, string name)
    {
        clientName = name;
        clientGuid = Guid.NewGuid();

        CreateConnection(host);
    }

    private void CreateConnection(string hostName)
    {
        var connectionFactory = new ConnectionFactory { HostName = hostName };
        connection = connectionFactory.CreateConnection();
        
        channel = connection.CreateModel();
        channel.ExchangeDeclare(exchange: "topic_chat", type: ExchangeType.Topic);
        
        consumer = new EventingBasicConsumer(channel);
        asyncHandlerThread = new Task(() =>
        {
            consumer.Received += (_, ea) =>
            {
                var properties = ea.BasicProperties.MessageId.Split();
                
                if (properties.Last() == clientGuid.ToString()) return;
                
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                Console.WriteLine($"{properties.First()} << {message}");
            };
        });
        asyncHandlerThread.Start();
    }
}