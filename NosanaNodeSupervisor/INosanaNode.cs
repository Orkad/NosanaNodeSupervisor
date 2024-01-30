namespace NosanaNodeSupervisor
{
    public interface INosanaNode
    {
        Task<StreamReader> GetLogReaderAsync(CancellationToken cancellationToken);
        Task RestartAsync(CancellationToken cancellationToken);
    }
}
