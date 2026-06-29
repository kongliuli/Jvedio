using Jvedio.Entity;
using Jvedio.Entity.Common;
using System.Collections.Generic;
using System.IO;

namespace Jvedio.Core.Metadata
{
    internal static class NfoMetadataHelper
    {
        public static string ResolveNfoPath(Video video)
        {
            if (video == null || string.IsNullOrEmpty(video.Path) || video.Path.EndsWith(".nfo", System.StringComparison.OrdinalIgnoreCase))
                return null;

            string dir = Path.GetDirectoryName(video.Path);
            if (string.IsNullOrEmpty(dir))
                return null;

            string baseNfo = Path.Combine(dir, Path.GetFileNameWithoutExtension(video.Path) + ".nfo");
            if (File.Exists(baseNfo))
                return baseNfo;

            if (!string.IsNullOrEmpty(video.VID)) {
                string vidNfo = Path.Combine(dir, video.VID + ".nfo");
                if (File.Exists(vidNfo))
                    return vidNfo;
            }

            return null;
        }

        public static Dictionary<string, object> MovieToMetadataDict(Movie movie)
        {
            if (movie == null)
                return null;

            Video video = movie.toVideo();
            if (video == null)
                return null;

            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(video.Title))
                dict["Title"] = video.Title;
            if (!string.IsNullOrEmpty(video.VID))
                dict["VID"] = video.VID;
            if (!string.IsNullOrEmpty(video.WebUrl))
                dict["WebUrl"] = video.WebUrl;
            if (!string.IsNullOrEmpty(video.ActorNames))
                dict["ActorNames"] = video.ActorNames;
            if (!string.IsNullOrEmpty(video.ReleaseDate))
                dict["ReleaseDate"] = video.ReleaseDate;
            return dict.Count > 0 ? dict : null;
        }
    }

    internal sealed class NfoMetadataProvider : IMetadataProvider
    {
        public string Name => "Nfo";
        public int Priority => 10;

        public bool CanProvide(Video video)
        {
            return !string.IsNullOrEmpty(NfoMetadataHelper.ResolveNfoPath(video));
        }

        public System.Threading.Tasks.Task<Dictionary<string, object>> GetMetadataAsync(
            Video video,
            System.Threading.CancellationToken cancellationToken,
            SuperUtils.Framework.Tasks.TaskLogger logger,
            SuperUtils.NetWork.Entity.RequestHeader header,
            System.Action<SuperUtils.NetWork.Entity.RequestHeader> headerCallback)
        {
            string nfoPath = NfoMetadataHelper.ResolveNfoPath(video);
            if (string.IsNullOrEmpty(nfoPath))
                return System.Threading.Tasks.Task.FromResult<Dictionary<string, object>>(null);

            try {
                Movie movie = NfoMetadataReader.TryReadMovie(nfoPath);
                Dictionary<string, object> dict = NfoMetadataHelper.MovieToMetadataDict(movie);
                logger?.Info($"metadata nfo hit [{nfoPath}]");
                return System.Threading.Tasks.Task.FromResult(dict);
            } catch (System.Exception ex) {
                logger?.Error(ex.Message);
                return System.Threading.Tasks.Task.FromResult<Dictionary<string, object>>(null);
            }
        }
    }
}
