using System;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Core.Scan
{
    internal static class HashPathImportClassifier
    {
        public static ImportClassification<T> Classify<T>(
            List<T> import,
            IHashPathIndex<T> index,
            Func<T, string> getPath,
            string samePathHashReason,
            string hashSamePathDiffReason,
            string pathSameHashDiffReason,
            Action<T, T> prepareUpdate = null) where T : class
        {
            ImportClassification<T> result = new ImportClassification<T>();
            if (import == null || import.Count == 0 || index == null)
                return result;

            foreach (T item in import.Where(arg => index.HasSamePathAndHash(arg)).ToList())
                result.NotImport[getPath(item)] = new ScanDetailInfo(samePathHashReason);
            import.RemoveAll(arg => index.HasSamePathAndHash(arg));

            foreach (T item in import.ToList()) {
                T exist = index.FindSameHashDifferentPath(item);
                if (exist == null)
                    continue;
                prepareUpdate?.Invoke(item, exist);
                result.ToUpdate.Add(item);
                result.UpdateReasons[getPath(item)] = hashSamePathDiffReason;
            }
            import.RemoveAll(arg => index.HasSameHashDifferentPath(arg));

            foreach (T item in import.ToList()) {
                T exist = index.FindSamePathDifferentHash(item);
                if (exist == null)
                    continue;
                prepareUpdate?.Invoke(item, exist);
                result.ToUpdate.Add(item);
                result.UpdateReasons[getPath(item)] = pathSameHashDiffReason;
            }
            import.RemoveAll(arg => index.HasSamePathDifferentHash(arg));

            result.ToInsert.AddRange(import);
            return result;
        }
    }
}
