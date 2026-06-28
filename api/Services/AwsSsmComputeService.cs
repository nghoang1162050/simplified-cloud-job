using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using api.Configurations;
using Microsoft.Extensions.Options;

namespace api.Services;

public class AwsSsmComputeService(IAmazonSimpleSystemsManagement ssmClient, IOptions<AwsSettings> awsOptions) : IComputeExecutionService
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient = ssmClient;
    private readonly AwsSettings _awsSettings = awsOptions.Value;

    public async Task<string> TriggerRemoteCommandAsync(string command)
    {
        var sendCommandRequest = new SendCommandRequest
        {
            InstanceIds = new List<string> { _awsSettings.TargetEc2InstanceId },
            DocumentName = "AWS-RunShellScript",
            Parameters = new Dictionary<string, List<string>>
            {
                { "commands", new List<string> { command } }
            }
        };

        try
        {
            var response = await _ssmClient.SendCommandAsync(sendCommandRequest);

            return response.Command.CommandId;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to trigger SSM command on EC2: {ex.Message}", ex);
        }
    }
}
