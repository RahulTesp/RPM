namespace RPM.Job.PatientSmsWebJob;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // No-op for single execution job
        _logger.LogInformation("Worker executed once at: {time}", DateTimeOffset.Now);
    }
}
