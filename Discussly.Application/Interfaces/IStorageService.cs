using Discussly.Application.Settings;
using Discussly.Core.Commons;
using Discussly.Core.DTOs.File;
using Microsoft.AspNetCore.Http;

namespace Discussly.Application.Interfaces
{
    public interface IStorageService
    {
        Task<Result<FileInfoDto>> SaveFileAsync(Guid fileId, IFormFile file, Storage storage);

        Result DeleteFile(string fileName, Storage storage, FileType fileType);
    }
}