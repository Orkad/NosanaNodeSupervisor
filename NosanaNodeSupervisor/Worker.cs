using Docker.DotNet;
using Docker.DotNet.Models;
using System.Threading;

namespace NosanaNodeSupervisor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string CONTAINER_NAME = "nosana-node";
        private string lastLog = "";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var client = new DockerClientConfiguration().CreateClient();
            var test = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            while (!cancellationToken.IsCancellationRequested)
            {
                var parameters = new ContainerLogsParameters { Follow = true, Tail = "1", ShowStdout = true };
                var stream = await client.Containers.GetContainerLogsAsync(CONTAINER_NAME, parameters, cancellationToken);
                string line = null;
                using var streamReader = new StreamReader(stream);

                while ((line = await streamReader.ReadLineAsync(cancellationToken)) != null)
                {
                    var log = line.Split(" - ").Last().Trim();
                    if (log != lastLog)
                    {
                        _logger.LogInformation("Node is working on: " + log);
                        lastLog = log;
                    }
                    if (log == "flow has expired")
                    {
                        await RestartNodeAsync(client, cancellationToken);
                    }
                }
                await Task.Delay(3000, cancellationToken);
            }
        }

        private async Task RestartNodeAsync(DockerClient client, CancellationToken cancellationToken)
        {
            _logger.LogInformation("trying to restart node");
            await client.Containers.RestartContainerAsync(CONTAINER_NAME, new ContainerRestartParameters
            {
                WaitBeforeKillSeconds = 30,
            }, cancellationToken);
            _logger.LogInformation("node was restarted");
        }
    }
}
