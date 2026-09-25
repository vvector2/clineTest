using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using ClineTestCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClineTestInfra.Storage;

public static class S3StorageServiceCollectionExtensions
{
    public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
    {
        var options = ResolveOptions(configuration);

        services.AddSingleton(options);
        services.AddSingleton<IAmazonS3>(_ => CreateClient(options));
        services.AddSingleton<IFileStorage, S3FileStorage>();

        return services;
    }

    private static StorageOptions ResolveOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(StorageOptions.SectionName);

        var options = new StorageOptions
        {
            BucketName = ResolveValue(section[nameof(StorageOptions.BucketName)], "CLINETEST_S3_BUCKET") ?? string.Empty,
            Region = ResolveValue(section[nameof(StorageOptions.Region)], "CLINETEST_S3_REGION") ?? string.Empty,
            ServiceUrl = ResolveValue(section[nameof(StorageOptions.ServiceUrl)], "CLINETEST_S3_SERVICE_URL"),
            PublicBaseUrl = ResolveValue(section[nameof(StorageOptions.PublicBaseUrl)], "CLINETEST_S3_PUBLIC_BASE_URL"),
            AccessKey = ResolveValue(section[nameof(StorageOptions.AccessKey)], "CLINETEST_S3_ACCESS_KEY"),
            SecretKey = ResolveValue(section[nameof(StorageOptions.SecretKey)], "CLINETEST_S3_SECRET_KEY")
        };

        var forcePathStyle = ResolveValue(section[nameof(StorageOptions.ForcePathStyle)], "CLINETEST_S3_FORCE_PATH_STYLE");

        if (bool.TryParse(forcePathStyle, out var parsedForcePathStyle))
        {
            options.ForcePathStyle = parsedForcePathStyle;
        }

        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException(
                "S3 bucket name is not configured. Set the CLINETEST_S3_BUCKET environment variable.");
        }

        if (string.IsNullOrWhiteSpace(options.Region))
        {
            throw new InvalidOperationException(
                "S3 region is not configured. Set the CLINETEST_S3_REGION environment variable.");
        }

        return options;
    }

    private static string? ResolveValue(string? value, string environmentVariable)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Environment.GetEnvironmentVariable(environmentVariable)
            : value;
    }

    private static IAmazonS3 CreateClient(StorageOptions options)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = options.ForcePathStyle
        };

        if (string.IsNullOrWhiteSpace(options.ServiceUrl))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
        }
        else
        {
            config.ServiceURL = options.ServiceUrl;
            config.AuthenticationRegion = options.Region;
            config.RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED;
        }

        var hasCredentials = !string.IsNullOrWhiteSpace(options.AccessKey)
            && !string.IsNullOrWhiteSpace(options.SecretKey);

        return hasCredentials
            ? new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config)
            : new AmazonS3Client(config);
    }
}
