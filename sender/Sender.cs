using System.Text;
using RabbitMQ.Client;

namespace sender;

public class Sender {
    public void ProduceMessages() {        
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        Send(channel,  "AAPL");
        Send(channel,  "GOOG");
        Send(channel, "AAPL");
        Send(channel,  "GOOG");
        Send(channel, "GOOG");
        Send(channel,  "FB");
        Send(channel,  "MIC");
        Send(channel,  "MIC");
        Send(channel, "FB");
        Send(channel, "MIC");
    }
    private void Send(IModel channel, string symbol) {
        var text = $"{symbol}";
        var message = Encoding.UTF8.GetBytes(text);
        var routingKey = "trade.eq.q";
        Console.WriteLine($"sending: {text}");
        channel.BasicPublish("", routingKey, null, message);
        Thread.Sleep(500);
    }
}