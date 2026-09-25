namespace ClineTestInfra.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string BucketName { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string? ServiceUrl { get; set; }

    public string? PublicBaseUrl { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    public bool ForcePathStyle { get; set; }
}
