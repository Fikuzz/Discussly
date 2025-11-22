using Discussly.Core.Commons;

namespace Discussly.Core.DTOs.File
{
    public class FileInfoDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public FileType FileType { get; set; } = FileType.Unknown;
        public DateTime CreatedAt { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public object Metadata { get; set; } = new();
    }
}
