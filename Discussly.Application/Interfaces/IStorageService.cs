using Discussly.Core.Commons;
using Microsoft.AspNetCore.Http;

namespace Discussly.Application.Interfaces
{
    public interface IStorageService
    {
        Task<Result<string>> SaveMediaAsync(Guid Id, Storage storage, IFormFile file);
        Result DeleteMedia(string fileName, Storage storage);
    }
}