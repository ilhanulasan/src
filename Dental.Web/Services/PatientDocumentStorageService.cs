namespace Dental.Web.Services;

public interface IPatientDocumentStorageService
{
    Task<string> SaveAsync(Guid patientId, string fileName, Stream content, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct);
    Task DeleteAsync(string storagePath, CancellationToken ct);
}

public class PatientDocumentStorageService(IWebHostEnvironment env, ILogger<PatientDocumentStorageService> log)
    : IPatientDocumentStorageService
{
    private string RootPath => Path.Combine(env.ContentRootPath, "Data", "patient-documents");

    public async Task<string> SaveAsync(Guid patientId, string fileName, Stream content, CancellationToken ct)
    {
        var safeName = Path.GetFileName(fileName);
        var dir = Path.Combine(RootPath, patientId.ToString("N"));
        Directory.CreateDirectory(dir);

        var storedName = $"{Guid.NewGuid():N}_{safeName}";
        var fullPath = Path.Combine(dir, storedName);

        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, ct);

        log.LogInformation("Stored patient document {Path}", fullPath);
        return Path.Combine(patientId.ToString("N"), storedName).Replace('\\', '/');
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct)
    {
        var fullPath = Path.Combine(RootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        var fullPath = Path.Combine(RootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
