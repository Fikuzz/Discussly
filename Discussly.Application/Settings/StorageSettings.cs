namespace Discussly.Application.Settings
{
    public class StorageSettings
    {
        public string BasePath { get; set; } = "";
        public MediaSettings Avatars { get; set; } = new();
        public MediaSettings CommunityAvatars { get; set; } = new();
        public MediaSettings PostImages { get; set; } = new();
        public VideoSettings PostVideos { get; set; } = new();
        public FileSettings Attachments { get; set; } = new();
    }

    public class MediaSettings
    {
        public string Path { get; set; } = "";
        public long MaxFileSize { get; set; }
        public string[] AllowedFormats { get; set; } = Array.Empty<string>();
        public string SaveFileAs { get; set; } = ".webp";
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class VideoSettings
    {
        public string Path { get; set; } = "";
        public long MaxFileSize { get; set; }
        public string[] AllowedFormats { get; set; } = Array.Empty<string>();
        public int MaxDuration { get; set; } // в секундах
        public bool Compress { get; set; }
    }

    public class FileSettings
    {
        public string Path { get; set; } = "";
        public long MaxFileSize { get; set; }
        public string[] AllowedFormats { get; set; } = Array.Empty<string>();
    }
}
