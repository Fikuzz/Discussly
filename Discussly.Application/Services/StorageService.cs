using Discussly.Application.Interfaces;
using Discussly.Application.Settings;
using Discussly.Core.Commons;
using Discussly.Core.DTOs.File;
using Discussly.Core.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Discussly.Application.Services
{
    public class StorageService : IStorageService
    {
        private readonly StorageSettings _settings;
        private readonly ILogger<StorageService> _logger;
        public StorageService(IOptions<StorageSettings> settings, ILogger<StorageService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<Result<FileInfoDto>> SaveFileAsync(Guid fileId, IFormFile file, Storage storage)
        {
            var fileType = GetFileTypeFromMime(file);

            var settings = GetFileSettings(storage, fileType);

            var validate = ValidateFile(file, settings);
            if (validate.IsFailure)
                return Result<FileInfoDto>.Failure(validate.Error);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileType == FileType.Image)
            {
                if(settings is ImageSettings imageSettings)
                {
                    extension = imageSettings.SaveFileAs;
                }
            }
            var fileName = await GenerateFileNameAsync(fileId, file, extension);
            
            var result = await SaveFile(file, fileName, settings, fileType);

            var fileInfoDto = new FileInfoDto()
            {
                FileName = fileName,
                FilePath = settings.Path,
                FileType = fileType,
                FileSize = result.Value,
                CreatedAt = DateTime.UtcNow,
                Metadata = new
                {
                    Extension = extension
                }
            };

            return Result.Success(fileInfoDto);
        }

        public Result DeleteFile(string fileName, Storage storage, FileType fileType)
        {
            var settings = GetFileSettings(storage, fileType);

            var FullFilePath = Path.Combine(_settings.BasePath, settings.Path, fileName);
            if (File.Exists(FullFilePath))
            {
                File.Delete(FullFilePath);
                _logger.LogInformation("File deleted: {FileName} from {Storage}", fileName, storage);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FileName} in {Storage}", fileName, storage);
            }

            return Result.Success();
        }

        private FileSettings GetFileSettings(Storage storage, FileType fileType)
        {
            return storage switch
            {
                Storage.UserAvatar => _settings.Avatars,
                Storage.CommunityAvatar => _settings.CommunityAvatars,
                Storage.PostMedia => fileType switch
                {
                    FileType.Image => _settings.PostImages,
                    FileType.Video => _settings.PostVideos,
                    _ => _settings.Attachments
                },
                Storage.CommentMedia => _settings.CommentImages,
                _ => _settings.Attachments
            };
        }

        private FileType GetFileTypeFromMime(IFormFile file)
        {
            var mimeType = file.ContentType.ToLowerInvariant();

            if (mimeType.StartsWith("image/"))
                return FileType.Image;
            else if (mimeType.StartsWith("video/"))
                return FileType.Video;
            else if (mimeType.StartsWith("audio/"))
                return FileType.Audio;
            else if (mimeType.Contains("pdf") || mimeType.Contains("msword") ||
                     mimeType.Contains("excel") || mimeType.Contains("powerpoint") ||
                     mimeType.Contains("text/"))
                return FileType.Document;
            else
                return FileType.Unknown;
        }

        private Result ValidateFile(IFormFile file, FileSettings settings)
        {
            if (file == null || file.Length == 0)
                return Result.Failure("File is empty");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !settings.AllowedFormats.Contains(extension))
                return Result.Failure($"File format is not supported.");

            if (file.Length > settings.MaxFileSize)
                return Result.Failure($"The file is over {settings.MaxFileSize / 1024 / 1024}Mb");

            return Result.Success();
        }

        private async Task<string> GenerateFileNameAsync(Guid id, IFormFile file, string extension)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return $"{hashString}-{id}{extension}";
        }

        private async Task<Result<long>> SaveFile(IFormFile file, string fileName, FileSettings fileSettings, FileType fileType)
        {
            var basePath = Path.Combine(_settings.BasePath, fileSettings.Path);
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            var fullFilePath = Path.Combine(basePath, fileName);

            switch (fileType)
            {
                case FileType.Image:
                    if(fileSettings is ImageSettings imageSettings)
                    {
                        var result = await ResizeAndSaveImageAsync(file, fullFilePath, imageSettings.Width, imageSettings.Height);
                        return result;
                    }
                    else
                    {
                        return Result<long>.Failure("The file cannot be saved");
                    }

                case FileType.Video:
                    if(fileSettings is VideoSettings videoSettings)
                    {
                        var result = await SaveVideoAsync(file, fullFilePath, videoSettings.Compress);
                        return result;
                    }
                    else
                    {
                        return Result<long>.Failure("The file cannot be saved");
                    }

                default:
                    return Result<long>.Failure("Couldn't determine the file type");
            }
        }

        private async Task<Result<long>> ResizeAndSaveImageAsync(IFormFile file, string fullFileName, int width, int height)
        {
            await using var fileStream = new FileStream(fullFileName, FileMode.Create);

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());
            if (width > 0 && height > 0)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(width, height),
                    Mode = ResizeMode.Crop
                }));
            }
            await image.SaveAsync(fileStream, new JpegEncoder());

            await fileStream.FlushAsync();
            fileStream.Close();

            var fileInfo = new FileInfo(fullFileName);
            _logger.LogInformation($"Image saved as {fullFileName}, Size: {fileInfo.Length} bytes");
            return Result.Success(fileInfo.Length);
        }

        private async Task<Result<long>> SaveVideoAsync(IFormFile file, string fullFileName, bool compress)
        {
            try
            {
                long fileSize;

                if (compress)
                {
                    // Сжатая версия
                    fileSize = await CompressVideoAsync(file, fullFileName);
                }
                else
                {
                    await using var fileStream = new FileStream(fullFileName, FileMode.Create);
                    await file.CopyToAsync(fileStream);

                    var fileInfo = new FileInfo(fullFileName);
                    fileSize = fileInfo.Length;
                }

                _logger.LogInformation($"Video saved as {fullFileName}, Size: {fileSize} bytes");
                return Result.Success(fileSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video save failed");
                return Result<long>.Failure("Video save failed");
            }
        }

        private async Task<long> CompressVideoAsync(IFormFile file, string outputPath)
        {
            // Временная реализация - просто копируем
            await using var fileStream = new FileStream(outputPath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            var fileInfo = new FileInfo(outputPath);
            return fileInfo.Length;

            // TODO: Реализовать настоящее сжатие через FFmpeg
            // return await _ffmpegService.CompressAndGetSizeAsync(file, outputPath);
        }
    }
}
