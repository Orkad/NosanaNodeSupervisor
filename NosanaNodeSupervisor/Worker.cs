namespace NosanaNodeSupervisor
{
    public class Worker(ILogger<Worker> logger, INosanaNode nosanaNode) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private string lastLog = "";

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var streamReader = await nosanaNode.GetLogReaderAsync(cancellationToken);
                string line;
                while ((line = await streamReader.ReadLineAsync(cancellationToken)) != null)
                {
                    var log = line.Split(" - ").Last().Trim();
                    if (log != lastLog)
                    {
                        _logger.LogInformation("Nosana node: {0}", log);
                        lastLog = log;
                    }
                    if (log == "Flow has expired")
                    {
                        _logger.LogWarning("Expired flow detected");
                        await nosanaNode.RestartAsync(cancellationToken);
                    }
                }
                await Task.Delay(3000, cancellationToken);
            }
        }
    }
}
