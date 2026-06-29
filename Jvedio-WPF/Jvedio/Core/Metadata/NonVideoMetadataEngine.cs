using Jvedio.Core.Enums;
using Jvedio.Core.Metadata;
using Jvedio.Entity;
using Jvedio.Entity.Data;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    /// <summary>Game/Picture/Comics 本地 + WebUrl 联网元数据（META-005）。</summary>
    public static class NonVideoMetadataEngine
    {
        public static Dictionary<string, object> ReadLocalFields(MetaData data, DataType dataType)
        {
            if (data == null)
                return null;

            switch (dataType) {
                case DataType.Game:
                    return GameMetadataProvider.ReadLocal(data as Game);
                case DataType.Comics:
                case DataType.Picture:
                    return PictureMetadataProvider.ReadLocal(data);
                default:
                    return null;
            }
        }

        public static async Task<Dictionary<string, object>> ReadWithWebAsync(
            MetaData data,
            DataType dataType,
            RequestHeader header,
            TaskLogger logger,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, object> fields = ReadLocalFields(data, dataType) ?? new Dictionary<string, object>();
            string webUrl = ExtractWebUrl(data, dataType);
            if (string.IsNullOrWhiteSpace(webUrl))
                return fields.Count > 0 ? fields : null;

            Dictionary<string, object> remote = await WebMetadataFetcher.FetchAsync(webUrl, header, logger, cancellationToken)
                .ConfigureAwait(false);
            MergeRemote(fields, remote);
            return fields.Count > 0 ? fields : null;
        }

        private static string ExtractWebUrl(MetaData data, DataType dataType)
        {
            if (data == null)
                return null;
            if (dataType == DataType.Game && data is Game game)
                return game.WebUrl;
            if (dataType == DataType.Comics && data is Comic comic)
                return comic.WebUrl;
            return null;
        }

        private static void MergeRemote(Dictionary<string, object> local, Dictionary<string, object> remote)
        {
            if (remote == null)
                return;
            foreach (KeyValuePair<string, object> kv in remote) {
                if (kv.Value == null)
                    continue;
                string text = kv.Value.ToString();
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                if (!local.ContainsKey(kv.Key) || string.IsNullOrWhiteSpace(local[kv.Key]?.ToString()))
                    local[kv.Key] = text;
            }
        }
    }

    internal static class GameMetadataProvider
    {
        public static Dictionary<string, object> ReadLocal(Game game)
        {
            if (game == null)
                return null;

            var fields = new Dictionary<string, object>();
            Add(fields, "Title", game.Title);
            Add(fields, "Genre", game.Genre);
            Add(fields, "Studio", game.Studio);
            Add(fields, "WebUrl", game.WebUrl);
            Add(fields, "WebType", game.WebType);
            Add(fields, "Plot", game.Plot);
            Add(fields, "Publisher", game.Publisher);
            return fields.Count > 0 ? fields : null;
        }

        private static void Add(Dictionary<string, object> dict, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                dict[key] = value;
        }
    }

    internal static class PictureMetadataProvider
    {
        public static Dictionary<string, object> ReadLocal(MetaData data)
        {
            if (data == null)
                return null;

            var fields = new Dictionary<string, object>();
            Add(fields, "Title", data.Title);
            Add(fields, "Genre", data.Genre);
            if (data is Comic comic)
                Add(fields, "WebUrl", comic.WebUrl);
            return fields.Count > 0 ? fields : null;
        }

        private static void Add(Dictionary<string, object> dict, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                dict[key] = value;
        }
    }
}
