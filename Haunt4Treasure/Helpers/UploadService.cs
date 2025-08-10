using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;


namespace Haunt4Treasure.Helpers;
public interface IUploadFileService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder);
}
public class UploadFileService : IUploadFileService
{
    private readonly AppSettings _appSettings;
    private readonly Cloudinary _cloudinary;

    public UploadFileService(IOptions<AppSettings> appSettings, Cloudinary cloudinary)
    {
        _appSettings = appSettings.Value;
        _cloudinary = cloudinary;
    }
    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder, // Optional: your folder name on Cloudinary
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return uploadResult.SecureUrl.ToString(); // ✅ Return secure URL
        }

        throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message}");
    }
    public async Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder)
    {
        if (files == null || !files.Any())
            throw new ArgumentException("No files provided");

        var uploadTasks = files.Select(async file =>
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString(); // ✅ single secure URL per file
            }

            throw new Exception($"Cloudinary upload failed for {file.FileName}: {uploadResult.Error?.Message}");
        });

        var uploadedUrls = await Task.WhenAll(uploadTasks); // ✅ List<string> returned

        return uploadedUrls.ToList(); // ✅ Here is your List<string> of secure URLs for your 5 images
    }

}