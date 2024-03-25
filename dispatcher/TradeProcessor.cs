namespace dispatcher;
public class TradeProcessor {
    private int threadId;
    private Queue<string> queue = new();
    private Action<int> onComplete;
    private Action<int> onStart;

    public TradeProcessor(int threadId, Action<int> onStart, Action<int> onComplete) {
        this.threadId = threadId;
        this.onComplete = onComplete;
        this.onStart = onStart;
    }

    public void AddMessage(string message) {
        queue.Enqueue(message);
    }
    
    public void Start() {
        while (true) {
            try {
                if (queue.Count > 0) {
                    onStart?.Invoke(threadId);
                    Thread.Sleep(5000); 
                    var message = queue.Dequeue();
                    Console.WriteLine($"Thread {threadId}: Processed {message}");
                    onComplete?.Invoke(threadId); 
                }
                Thread.Sleep(100); // Sleep briefly to avoid a tight loop
            } catch (Exception e) {
                Console.WriteLine(e.StackTrace); // Log the exception stack trace
            }
        }
    }
}

