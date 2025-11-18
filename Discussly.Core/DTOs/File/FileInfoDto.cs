using Discussly.Core.Commons;

namespace Discussly.Core.DTOs.File
{
    public class FileInfoDto
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public FileType FileType { get; set; } = FileType.Unknown;
        public DateTime CreatedAt { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Extension { get; set; } = string.Empty;
    }
}
