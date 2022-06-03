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
        DeclareQueue(queue);
        
        Start();
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
    
    private void DeclareQueue(string baseQueue)
    {
        this.baseQueue = baseQueue;
        _currentQueue = baseQueue + "$" + clientGuid;
        
        channel.QueueDeclare(queue: _currentQueue,
            durable: false,
            exclusive: true,
            autoDelete: false,
            arguments: null);
        
        channel.BasicConsume(queue: _currentQueue,
            autoAck: true,
            consumer: consumer);
        channel.QueueBind(_currentQueue, "topic_chat", baseQueue + "$*", null);
    }

    private void Start()
    {
        ChatUtils.PrintChatInfo();
        while (true)
        {
            var input = Console.ReadLine();
            ChatUtils.ClearCurrentConsoleLine();
            string message, command = string.Empty;
            
            if (input.StartsWith('#'))
            {
                var splittedInput = input.Split(' ');
                
                command = splittedInput[0];
                message = splittedInput.Length > 1 ? splittedInput[1] : string.Empty;
            }
            else
            {
                message = input;
            }

            switch (command)
            {
                case "":
                    var body = Encoding.UTF8.GetBytes(message);
                    var properties = channel.CreateBasicProperties();
                    
                    properties.MessageId = clientName + ' ' + clientGuid;
                    channel.BasicPublish(exchange: "topic_chat",
                        routingKey: baseQueue + "$*",
                        basicProperties: properties,                  
                        body: body);
                    
                    Console.WriteLine($"{clientName} >> {message}");
                    break;
                case "#switch":
                    DeclareQueue(message);
                    
                    Console.Clear();
                    Console.WriteLine($"{clientName} switched to {message}");
                    break;
                case "#exit":
                    asyncHandlerThread.Dispose();
                    channel.Close();
                    connection.Close();
                    
                    Console.WriteLine($"{clientName} disconnected");
                    return;
            }
        }
    }
}