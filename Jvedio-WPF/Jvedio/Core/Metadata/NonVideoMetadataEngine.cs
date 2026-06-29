using Jvedio.Core.Enums;
using Jvedio.Core.Metadata;
using Jvedio.Entity;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    /// <summary>Picture 本地元数据（META-005）。</summary>
    public static class NonVideoMetadataEngine
    {
        public static Dictionary<string, object> ReadLocalFields(MetaData data, DataType dataType)
        {
            if (data == null || dataType != DataType.Picture)
                return null;
            return PictureMetadataProvider.ReadLocal(data);
        }

        public static async Task<Dictionary<string, object>> ReadWithWebAsync(
            MetaData data,
            DataType dataType,
            RequestHeader header,
            TaskLogger logger,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, object> fields = ReadLocalFields(data, dataType) ?? new Dictionary<string, object>();
            return fields.Count > 0 ? fields : null;
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
            return fields.Count > 0 ? fields : null;
        }

        private static void Add(Dictionary<string, object> dict, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                dict[key] = value;
        }
    }
}
