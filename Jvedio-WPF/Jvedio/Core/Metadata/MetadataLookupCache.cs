using Jvedio.Entity;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Metadata
{
    public enum MetadataLookupCacheStatus
    {
        Hit,
        Miss,
    }

    /// <summary>刮削 Lookup 结果缓存（成功长 TTL，404/无结果短 TTL）。ponytail: SQLite 单表，过期行惰性忽略。</summary>
    public static class MetadataLookupCache
    {
        public const string KeyVersion = "v1";
        public static readonly TimeSpan SuccessTtl = TimeSpan.FromDays(90);
        public static readonly TimeSpan NotFoundTtl = TimeSpan.FromDays(7);

        public static bool ShouldBypassCache()
        {
            return ConfigManager.DownloadConfig.OverrideInfo;
        }

        public static string BuildCacheKey(string providerName, Video video)
        {
            return BuildCacheKey(providerName, video?.VID, video != null ? (int)video.VideoType : 0);
        }

        public static string BuildCacheKey(string providerName, string vid, int videoType = 0)
        {
            return $"{KeyVersion}:{providerName?.ToLowerInvariant() ?? "unknown"}:lookup:{NormalizeVid(vid)}:{videoType}";
        }

        public static bool IsSuccessResult(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                return false;
            if (!data.TryGetValue("Title", out object titleObj))
                return false;
            return !string.IsNullOrWhiteSpace(titleObj?.ToString());
        }

        public static bool TryGetHit(string providerName, Video video, out Dictionary<string, object> data)
        {
            data = null;
            if (video == null || string.IsNullOrEmpty(video.VID))
                return false;

            MetadataLookupCacheEntry entry = MetadataLookupCacheStore.Get(BuildCacheKey(providerName, video));
            if (entry == null || entry.Status == MetadataLookupCacheStatus.Miss)
                return false;

            data = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(entry.Payload);
            return data != null && data.Count > 0;
        }

        public static bool IsCachedNotFound(string providerName, Video video)
        {
            if (video == null || string.IsNullOrEmpty(video.VID))
                return false;
            MetadataLookupCacheEntry entry = MetadataLookupCacheStore.Get(BuildCacheKey(providerName, video));
            return entry != null && entry.Status == MetadataLookupCacheStatus.Miss;
        }

        public static void SetHit(string providerName, Video video, Dictionary<string, object> data)
        {
            if (video == null || data == null || !IsSuccessResult(data))
                return;
            MetadataLookupCacheStore.Set(
                BuildCacheKey(providerName, video),
                MetadataLookupCacheStatus.Hit,
                JsonUtils.TrySerializeObject(data),
                DateTime.UtcNow.Add(SuccessTtl));
        }

        public static void SetNotFound(string providerName, Video video)
        {
            if (video == null || string.IsNullOrEmpty(video.VID))
                return;
            MetadataLookupCacheStore.Set(
                BuildCacheKey(providerName, video),
                MetadataLookupCacheStatus.Miss,
                null,
                DateTime.UtcNow.Add(NotFoundTtl));
        }

        internal static string NormalizeVid(string vid)
        {
            return string.IsNullOrWhiteSpace(vid) ? string.Empty : vid.Trim().ToUpperInvariant();
        }
    }

    internal sealed class MetadataLookupCacheEntry
    {
        public MetadataLookupCacheStatus Status { get; set; }
        public string Payload { get; set; }
    }

    internal static class MetadataLookupCacheStore
    {
        public static MetadataLookupCacheEntry Get(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return null;

            string now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            string sql = "SELECT Status, Payload FROM metadata_lookup_cache " +
                $"WHERE CacheKey={SqlQuote(cacheKey)} AND ExpiresUtc > {SqlQuote(now)} LIMIT 1;";
            List<Dictionary<string, object>> rows = appDatabaseMapper.Select(sql);
            if (rows == null || rows.Count == 0)
                return null;

            Dictionary<string, object> row = rows[0];
            string status = row["Status"]?.ToString();
            return new MetadataLookupCacheEntry {
                Status = "miss".Equals(status, StringComparison.OrdinalIgnoreCase)
                    ? MetadataLookupCacheStatus.Miss
                    : MetadataLookupCacheStatus.Hit,
                Payload = row["Payload"]?.ToString(),
            };
        }

        public static void Set(string cacheKey, MetadataLookupCacheStatus status, string payload, DateTime expiresUtc)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return;

            string statusText = status == MetadataLookupCacheStatus.Miss ? "miss" : "hit";
            string sql = "INSERT INTO metadata_lookup_cache (CacheKey, Status, Payload, ExpiresUtc) " +
                $"VALUES ({SqlQuote(cacheKey)}, {SqlQuote(statusText)}, {SqlQuote(payload ?? string.Empty)}, {SqlQuote(expiresUtc.ToString("o", CultureInfo.InvariantCulture))}) " +
                "ON CONFLICT(CacheKey) DO UPDATE SET " +
                $"Status={SqlQuote(statusText)}, Payload={SqlQuote(payload ?? string.Empty)}, " +
                $"ExpiresUtc={SqlQuote(expiresUtc.ToString("o", CultureInfo.InvariantCulture))};";
            appDatabaseMapper.ExecuteNonQuery(sql);
        }

        private static string SqlQuote(string value)
        {
            return "'" + (value ?? string.Empty).Replace("'", "''") + "'";
        }
    }
}
