namespace ClineTestCore.Storage;

public interface IFileStorage
{
    string BuildUrl(string key);

    Task UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);
}
