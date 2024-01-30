using Docker.DotNet;
using Docker.DotNet.Models;

namespace NosanaNodeSupervisor
{
    public class DockerNosanaNode(
        ILogger<DockerNosanaNode> logger,
        DockerClientConfiguration dockerClientConfiguration)
        : INosanaNode, IDisposable
    {
        private const string CONTAINER_NAME = "nosana-node";
        private readonly DockerClient dockerClient = dockerClientConfiguration.CreateClient();

        public async Task<StreamReader> GetLogReaderAsync(CancellationToken cancellationToken)
        {
            var parameters = new ContainerLogsParameters { Follow = true, Tail = "1", ShowStdout = true };
            var stream = await dockerClient.Containers.GetContainerLogsAsync(CONTAINER_NAME, parameters, cancellationToken);
            return new StreamReader(stream);
        }

        public async Task RestartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("restarting node");
            await dockerClient.Containers.RestartContainerAsync(CONTAINER_NAME, new ContainerRestartParameters
            {
                WaitBeforeKillSeconds = 30,
            }, cancellationToken);
            logger.LogInformation("node restarted");
        }

        public void Dispose()
        {
            dockerClient.Dispose();
        }
    }
}
