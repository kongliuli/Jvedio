using Jvedio.Entity;
using Jvedio.Entity.Common;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using static Jvedio.App;

namespace Jvedio.Core.Metadata
{
    /// <summary>NFO 回写（与 <see cref="NfoMetadataReader"/> 读路径对称）。</summary>
    public static class NfoMetadataWriter
    {
        public static void WriteVideo(Video video, string nfoPath)
        {
            if (video == null || string.IsNullOrEmpty(nfoPath))
                return;

            var nfo = new NFO(nfoPath, "movie");
            nfo.SetNodeText("source", video.WebUrl);
            nfo.SetNodeText("title", video.Title);
            nfo.SetNodeText("director", video.Director);
            nfo.SetNodeText("rating", video.Rating.ToString());
            nfo.SetNodeText("year", video.ReleaseYear.ToString());
            nfo.SetNodeText("release", video.ReleaseDate);
            nfo.SetNodeText("premiered", video.ReleaseDate);
            nfo.SetNodeText("runtime", video.Duration.ToString());
            nfo.SetNodeText("country", video.Country);
            nfo.SetNodeText("studio", video.Studio);
            nfo.SetNodeText("id", video.VID);
            nfo.SetNodeText("num", video.VID);

            foreach (var item in video.Genre?.Split(SuperUtils.Values.ConstValues.Separator) ?? Array.Empty<string>()) {
                if (!string.IsNullOrEmpty(item))
                    nfo.AppendNewNode("genre", item);
            }

            foreach (var item in video.Series?.Split(SuperUtils.Values.ConstValues.Separator) ?? Array.Empty<string>()) {
                if (!string.IsNullOrEmpty(item))
                    nfo.AppendNewNode("tag", item);
            }

            try {
                Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(video.ImageUrls);
                if (dict != null && dict.ContainsKey("ExtraImageUrl")) {
                    List<string> imageUrls = JsonUtils.TryDeserializeObject<List<string>>(dict["ExtraImageUrl"].ToString());
                    if (imageUrls != null && imageUrls.Count > 0) {
                        nfo.AppendNewNode("fanart");
                        foreach (var item in imageUrls) {
                            if (!string.IsNullOrEmpty(item))
                                nfo.AppendNodeToNode("fanart", "thumb", item, "preview", item);
                        }
                    }
                }
            } catch (Exception ex) {
                App.Logger.Error(ex);
            }

            if (video.ActorInfos != null && video.ActorInfos.Count > 0) {
                foreach (ActorInfo info in video.ActorInfos) {
                    if (!string.IsNullOrEmpty(info.ActorName)) {
                        nfo.AppendNewNode("actor");
                        nfo.AppendNodeToNode("actor", "name", info.ActorName);
                        nfo.AppendNodeToNode("actor", "type", "Actor");
                    }
                }
            }
        }
    }
}
