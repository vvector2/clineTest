namespace ClineTestCore.Storage;

public interface IFileStorage
{
    Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);
}
