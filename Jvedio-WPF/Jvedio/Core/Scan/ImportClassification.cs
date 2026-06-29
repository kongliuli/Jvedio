using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public class ImportClassification<T>
    {
        public List<T> ToInsert { get; set; } = new List<T>();
        public List<T> ToUpdate { get; set; } = new List<T>();
        public Dictionary<string, ScanDetailInfo> NotImport { get; set; } = new Dictionary<string, ScanDetailInfo>();
        public Dictionary<string, string> UpdateReasons { get; set; } = new Dictionary<string, string>();
    }
}
