namespace api.Services;

public interface IComputeExecutionService
{
    public Task<string> TriggerRemoteCommandAsync(string command);
}
