using System;
using System.Collections.Generic;
using System.Globalization;
using static Jvedio.MapperManager;

namespace Jvedio.Core.Scan.Discovery
{
    public interface IScanDirIndexStore
    {
        DirIndexEntry Get(int dbId, string dirPath);
        void Upsert(DirIndexEntry entry);
        void Remove(int dbId, string dirPath);
    }

    public sealed class ScanDirIndexStore : IScanDirIndexStore
    {
        public DirIndexEntry Get(int dbId, string dirPath)
        {
            if (dbId <= 0 || string.IsNullOrEmpty(dirPath))
                return null;

            string sql = "SELECT DbId, DirPath, MtimeUtcTicks, EntryCount, ChildrenChecksum, LastScanUtc " +
                $"FROM scan_dir_index WHERE DbId={dbId} AND DirPath={SqlQuote(dirPath)} LIMIT 1;";
            List<Dictionary<string, object>> rows = appDatabaseMapper.Select(sql);
            if (rows == null || rows.Count == 0)
                return null;

            Dictionary<string, object> row = rows[0];
            return new DirIndexEntry {
                DbId = Convert.ToInt32(row["DbId"], CultureInfo.InvariantCulture),
                DirPath = row["DirPath"]?.ToString(),
                MtimeUtcTicks = Convert.ToInt64(row["MtimeUtcTicks"], CultureInfo.InvariantCulture),
                EntryCount = Convert.ToInt32(row["EntryCount"], CultureInfo.InvariantCulture),
                ChildrenChecksum = row["ChildrenChecksum"]?.ToString(),
                LastScanUtc = row["LastScanUtc"]?.ToString(),
            };
        }

        public void Upsert(DirIndexEntry entry)
        {
            if (entry == null || entry.DbId <= 0 || string.IsNullOrEmpty(entry.DirPath))
                return;

            string sql = "INSERT INTO scan_dir_index (DbId, DirPath, MtimeUtcTicks, EntryCount, ChildrenChecksum, LastScanUtc) " +
                $"VALUES ({entry.DbId}, {SqlQuote(entry.DirPath)}, {entry.MtimeUtcTicks}, {entry.EntryCount}, " +
                $"{SqlQuote(entry.ChildrenChecksum ?? string.Empty)}, {SqlQuote(entry.LastScanUtc ?? DateTime.UtcNow.ToString("o"))}) " +
                "ON CONFLICT(DbId, DirPath) DO UPDATE SET " +
                $"MtimeUtcTicks={entry.MtimeUtcTicks}, EntryCount={entry.EntryCount}, " +
                $"ChildrenChecksum={SqlQuote(entry.ChildrenChecksum ?? string.Empty)}, " +
                $"LastScanUtc={SqlQuote(entry.LastScanUtc ?? DateTime.UtcNow.ToString("o"))};";
            appDatabaseMapper.ExecuteNonQuery(sql);
        }

        public void Remove(int dbId, string dirPath)
        {
            if (dbId <= 0 || string.IsNullOrEmpty(dirPath))
                return;
            appDatabaseMapper.ExecuteNonQuery(
                $"DELETE FROM scan_dir_index WHERE DbId={dbId} AND DirPath={SqlQuote(dirPath)};");
        }

        private static string SqlQuote(string value)
        {
            return "'" + (value ?? string.Empty).Replace("'", "''") + "'";
        }
    }
}
