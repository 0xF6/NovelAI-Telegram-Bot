namespace nai;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class EngineQueue(ILogger<EngineQueue> logger)
{
    public const string WithoutDelay = "none";
    public const string ImageGeneration = "image";
    public const string Payment = "payment";


    public Queue<(string taskName, Func<Task> action)> actions = new();
    public void Add(Func<Task> action, string taskName) 
        => actions.Enqueue((taskName, action));


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
            using var scope = logger.BeginScope("Execution task '{name}'", name);

            try
            {
                await task();
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "failed execute task '{name}'", name);
            }
            await Task.Delay(5000, ct);
        }
    }
}


public class EngineQueueWorker(EngineQueue queue) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => queue.CycleAsync(stoppingToken);
}
public static class QueueServices
{
    public static IServiceCollection UseQueue(this IServiceCollection collection)
    {
        collection.AddHostedService<EngineQueueWorker>();
        collection.AddSingleton<EngineQueue>();
        return collection;
    }
}

