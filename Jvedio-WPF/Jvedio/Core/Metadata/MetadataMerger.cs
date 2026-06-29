using System.Collections.Generic;

namespace Jvedio.Core.Metadata
{
    public static class MetadataMerger
    {
        public static Dictionary<string, object> Merge(IEnumerable<Dictionary<string, object>> sources)
        {
            Dictionary<string, object> merged = new Dictionary<string, object>();
            if (sources == null)
                return merged;

            foreach (Dictionary<string, object> source in sources) {
                if (source == null)
                    continue;
                foreach (var kv in source) {
                    if (kv.Value == null)
                        continue;
                    merged[kv.Key] = kv.Value;
                }
            }
            return merged;
        }
    }
}
