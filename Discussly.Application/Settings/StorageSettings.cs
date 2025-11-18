namespace Discussly.Application.Settings
{
    public class StorageSettings
    {
        public string BasePath { get; set; } = "";
        public ImageSettings Avatars { get; set; } = new();
        public ImageSettings CommunityAvatars { get; set; } = new();
        public ImageSettings PostImages { get; set; } = new();
        public VideoSettings PostVideos { get; set; } = new();
        public FileSettings Attachments { get; set; } = new();
    }

    public class ImageSettings : FileSettings
    {
        public string SaveFileAs { get; set; } = ".webp";
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class VideoSettings : FileSettings
    {
        public bool Compress { get; set; }
    }

    public class FileSettings
    {
        public string Path { get; set; } = "";
        public long MaxFileSize { get; set; }
        public string[] AllowedFormats { get; set; } = Array.Empty<string>();
    }
}
