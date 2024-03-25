using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace dispatcher;
public class Dispatcher {
    private const int MaxThreads = 4; // Define the maximum number of TradeProcessors
    
    //account, thread
    private ConcurrentDictionary<string, int> allocationMap = new();
    
    //thread id, number of messages processing
    private ConcurrentDictionary<long, long> processingCountMap = new();
    
    //Thread id, thread object
    private ConcurrentDictionary<int, TradeProcessor> threadpool = new();
    
    private bool display = true;

    public void DispatchMessages() {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "trade.eq.q", durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Initialize and start TradeProcessor instances
        for (int i = 0; i < MaxThreads; i++) {
            var processor = new TradeProcessor(i, StartProcessing, TradeComplete);
            threadpool[i] = processor;
            processingCountMap[i] = 0;  // Initialize count for this thread
            Task.Run(() => processor.Start());
        }
        var consumer = new EventingBasicConsumer(channel);
        channel.BasicConsume(queue: "trade.eq.q", autoAck: true, consumer);

        consumer.Received += (model, ea) => {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
           string context = message;
           int threadId = 0;

           if (allocationMap.ContainsKey(context)) {
               threadId = allocationMap[context];
           } 
           else 
           {
               threadId = (int)GetNextAvailableThread();
               allocationMap[context] = threadId;
           }
           processingCountMap[threadId] = processingCountMap.ContainsKey(threadId) ? processingCountMap[threadId] + 1 : 1;
           
           if (display) 
               Console.WriteLine("Dispatcher: Received " + message);
           
           DisplayAllocationMap();
           threadpool[threadId].AddMessage(message);
        };

        Console.WriteLine("Dispatcher started. Press [enter] to exit.");
        if(display)
            StartDisplayTask(); 
        
        Console.ReadLine();
    }
    public void TradeComplete(int threadId) {
        // Safely decrement the count for the threadId
        if (processingCountMap.ContainsKey(threadId)) {
            var newCount = Math.Max(0, processingCountMap[threadId] - 1);
            processingCountMap[threadId] = newCount;
        }

        // If the count is zero, find and remove the corresponding context
        if (processingCountMap.TryGetValue((int)threadId, out long count) && count == 0) {
            string contextToRemove = null;
            foreach (var entry in allocationMap) {
                if (entry.Value == threadId) {
                    contextToRemove = entry.Key;
                    break;
                }
            }

            if (contextToRemove != null) {
                allocationMap.TryRemove(contextToRemove, out _);
                processingCountMap.TryRemove((int)threadId, out _);  // Also remove the threadId from processingCountMap
            }
        }
    }

    public void StartProcessing(int threadId) {
    }
    private long GetNextAvailableThread() {
        long count = int.MaxValue;
        long threadId = 0;

        foreach (var entry in processingCountMap) {
            if (entry.Value < count) {
                count = entry.Value;
                threadId = entry.Key;
            }
        }

        return threadId;
    }
    private void DisplayAllocationMap() {
        if (!display) 
            return;
        var threads = threadpool.Keys.ToList();
        if(threads.Any())
            Console.WriteLine($"Allocation Map status");
        
        threads.Sort();
        foreach (var id in threads) {
            var contextKey = GetContextKey(id);
            // Check if the thread ID exists in the processingCountMap before accessing its value
            if (processingCountMap.TryGetValue(id, out var count)) {
                Console.WriteLine($"Thread-{id}" + (!string.IsNullOrEmpty(contextKey) ? $",{contextKey}" : "") + $",{count}");
            } 
        }
        Console.WriteLine();
    }
    
    private string GetContextKey(int threadId) {
        string context = "";
        foreach (var entry in allocationMap) {
            if (entry.Value == threadId) {
                context = entry.Key;
                break;
            }
        }
        return context;
    }
    public void StartDisplayTask() {
        Task.Run(async () => {
            while (true) {
                DisplayAllocationMap();
                await Task.Delay(5000); 
            }
        });
    }

}

