namespace nai;

public class EngineQueue
{
    public Queue<(string taskName, Func<Task> action)> actions = new();
    public void Add(Func<Task> action, string taskName) => actions.Enqueue((taskName, action));


    public async Task CycleAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (actions.Count == 0)
            {
                await Task.Delay(200, ct);
                continue;
            }
            var (name, task) = actions.Dequeue();
            try
            {
                Console.WriteLine($"Start execute task '{name}'");
                await task();
                Console.WriteLine($"Success execute task '{name}'");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed execute task '{name}'");
                Console.WriteLine(e);
            }
            Console.WriteLine($"Wait...");
            await Task.Delay(5000, ct);
            Console.WriteLine($"Success wait...");
        }
    }
}