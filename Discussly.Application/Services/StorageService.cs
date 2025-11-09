using Discussly.Application.Interfaces;
using Discussly.Application.Settings;
using Discussly.Core.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;

namespace Discussly.Application.Services
{
    public class StorageService : IStorageService
    {
        private StorageSettings _settings;
        public StorageService(IOptions<StorageSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<Result<string>> SaveMediaAsync(Guid userId, Storage storage, IFormFile file)
        {
            MediaSettings settings = GetSettings(storage);

            if (file.Length > settings.MaxFileSize)
                Result<string>.Failure($"File too large. Max size: {settings.MaxFileSize} bytes");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!settings.AllowedFormats.Contains(extension))
                Result<string>.Failure($"Invalid format. Allowed: {string.Join(", ", settings.AllowedFormats)}");

            var avatarsPath = GetMediaPath(storage);

            var hashString = await GenerateAvatarHashAsync(file);

            var fileName = $"{hashString}-{userId}{settings.SaveFileAs}";
            var filePath = Path.Combine(avatarsPath, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await ResizeAndSaveImageAsync(file, fileStream, settings.Width, settings.Height);

            return Result.Success(fileName);
        }
        public Result DeleteMedia(string fileName, Storage storage)
        {
            var avatersPath = GetMediaPath(storage);

            var filePath = Path.Combine(avatersPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Result.Success();
            }
            return Result.Failure("File not exist");
        }

        private string GetMediaPath(Storage storage)
        {
            var basePath = Path.Combine(_settings.BasePath, GetSettings(storage).Path);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            return basePath;
        }
        private async Task<string> GenerateAvatarHashAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return hashString;
        }
        private async Task ResizeAndSaveImageAsync(IFormFile file, Stream outputStream, int width, int height)
        {
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(width, height),
                Mode = ResizeMode.Crop
            }));
            await image.SaveAsync(outputStream, new JpegEncoder());
        }
        private MediaSettings GetSettings(Storage storage)
        {
            switch (storage)
            {
                case Storage.UserAvatar:
                    return _settings.Avatars;

                case Storage.CommunityAvatar:
                    return _settings.CommunityAvatars;
                
                default:
                    return _settings.Avatars;
            }
        }
    }
}
