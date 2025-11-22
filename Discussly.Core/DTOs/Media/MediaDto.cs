using Discussly.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discussly.Core.DTOs
{
    public class MediaDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public object Metadata { get; set; } = new();

        public static ICollection<MediaDto> MapList(IEnumerable<MediaAttachment>? medias)
        {
            ICollection<MediaDto> result = new List<MediaDto>();

            if (medias != null)
            {
                foreach (var media in medias)
                {
                    result.Add(
                        Map(media));
                }
            }

            return result;
        }

        public static MediaDto Map(MediaAttachment media)
        {
            return new MediaDto()
            {
                Name = media.FileName,
                Type = media.FileType.ToString(),
                Path = media.Path,
                Metadata = media.Metadata
            };
        }
    }
}
