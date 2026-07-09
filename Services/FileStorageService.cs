using Microsoft.AspNetCore.Components.Forms;

namespace ItAssets.Services;

public class FileStorageService
{
    private readonly string _uploadDirectory;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public FileStorageService(IWebHostEnvironment env)
    {
        _uploadDirectory = Path.Combine(env.ContentRootPath, "uploads");
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<string?> SaveFileAsync(IBrowserFile file)
    {
        if (file == null) return null;

        if (file.Size > MaxFileSize)
        {
            throw new InvalidOperationException($"File {file.Name} exceeds the maximum allowed size of 10 MB.");
        }

        var trustedFileNameForFileStorage = $"{Guid.NewGuid()}_{Path.GetFileName(file.Name)}";
        var path = Path.Combine(_uploadDirectory, trustedFileNameForFileStorage);

        await using FileStream fs = new(path, FileMode.Create);
        await file.OpenReadStream(MaxFileSize).CopyToAsync(fs);

        return trustedFileNameForFileStorage;
    }

    public void DeleteFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;

        var path = Path.Combine(_uploadDirectory, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
