using Amazon.S3;
using Amazon.S3.Model;
using ClineTestCore.Storage;

namespace ClineTestInfra.Storage;

public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;

    public S3FileStorage(IAmazonS3 s3Client, StorageOptions options)
    {
        _s3Client = s3Client;
        _options = options;
    }

    public string BuildUrl(string key)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_options.PublicBaseUrl) ? _options.ServiceUrl : _options.PublicBaseUrl;

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            return $"{baseUrl.TrimEnd('/')}/{_options.BucketName}/{key}";
        }

        return $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}";
    }

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }
}
