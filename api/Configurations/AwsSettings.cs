namespace api.Configurations;

public class AwsSettings
{
    public const string SectionName = "AWS";
    public string Profile { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string TargetEc2InstanceId { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
}
