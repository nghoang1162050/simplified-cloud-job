using Amazon.S3;
using Amazon.S3.Transfer;
using api.Configurations;
using Microsoft.Extensions.Options;

namespace api.Services;

public class S3StorageService(IAmazonS3 s3Client, IOptions<AwsSettings> awsOptions) : IStorageService
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly AwsSettings _awsSettings = awsOptions.Value;

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string requestHash)
    {
        var utcNow = DateTime.UtcNow;
        string year = utcNow.ToString("yyyy");
        string month = utcNow.ToString("MM");
        string day = utcNow.ToString("dd");

        string secureFileName = $"{requestHash}_{fileName}";

        string objectKey = $"{year}/{month}/{day}/{secureFileName}";

        using var fileTransferUtility = new TransferUtility(_s3Client);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = objectKey,
            BucketName = _awsSettings.BucketName,
            CannedACL = S3CannedACL.Private
        };

        await fileTransferUtility.UploadAsync(uploadRequest);

        return $"s3://{_awsSettings.BucketName}/{objectKey}";
    }
}
