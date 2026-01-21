namespace A4OCore.Store.Attachment
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(Stream fileStream, string fileName);
        Task<(Stream FileStream, string ContentType, string FileName)> GetFileAsync(string relativePath);
        Task<string> SaveTempFileAsync(Stream fileStream, string fileName);
        Task<string> MoveTempToPermanentAsync(string tempPath);
        Task CleanupTempFilesAsync(TimeSpan olderThan);
        string GetStoragePath();
    }
}
