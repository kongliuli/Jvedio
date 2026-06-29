using Jvedio.Entity;
using Jvedio.Entity.Data;
using System;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    internal interface IHashPathIndex<T>
    {
        bool HasSamePathAndHash(T item);
        T FindSameHashDifferentPath(T item);
        bool HasSameHashDifferentPath(T item);
        T FindSamePathDifferentHash(T item);
        bool HasSamePathDifferentHash(T item);
    }

    internal sealed class HashPathIndex<T>
    {
        private readonly Dictionary<(string Path, string Hash), List<T>> _byPathHash =
            new Dictionary<(string Path, string Hash), List<T>>();

        private readonly Dictionary<string, List<T>> _byHash =
            new Dictionary<string, List<T>>(StringComparer.Ordinal);

        private readonly Dictionary<string, List<T>> _byPath =
            new Dictionary<string, List<T>>(StringComparer.OrdinalIgnoreCase);

        public HashPathIndex(IEnumerable<T> items, Func<T, string> getPath, Func<T, string> getHash)
        {
            if (items == null)
                return;
            foreach (T item in items) {
                if (item == null)
                    continue;
                string path = getPath(item) ?? string.Empty;
                string hash = getHash(item) ?? string.Empty;
                AddToMap(_byPathHash, (path, hash), item);
                if (!string.IsNullOrEmpty(hash))
                    AddToMap(_byHash, hash, item);
                if (!string.IsNullOrEmpty(path))
                    AddToMap(_byPath, path, item);
            }
        }

        private static void AddToMap<TKey>(Dictionary<TKey, List<T>> map, TKey key, T item)
        {
            if (!map.TryGetValue(key, out List<T> list)) {
                list = new List<T>();
                map[key] = list;
            }
            list.Add(item);
        }

        public bool HasSamePathAndHash(T item, Func<T, string> getPath, Func<T, string> getHash)
        {
            return _byPathHash.TryGetValue((getPath(item) ?? string.Empty, getHash(item) ?? string.Empty), out List<T> list) &&
                list.Count > 0;
        }

        public T FindSameHashDifferentPath(T item, Func<T, string> getPath, Func<T, string> getHash)
        {
            string hash = getHash(item);
            if (string.IsNullOrEmpty(hash) || !_byHash.TryGetValue(hash, out List<T> list))
                return default;
            foreach (T t in list) {
                if (!string.Equals(getPath(item), getPath(t), StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return default;
        }

        public bool HasSameHashDifferentPath(T item, Func<T, string> getPath, Func<T, string> getHash)
        {
            return FindSameHashDifferentPath(item, getPath, getHash) != null;
        }

        public T FindSamePathDifferentHash(T item, Func<T, string> getPath, Func<T, string> getHash)
        {
            string path = getPath(item);
            if (string.IsNullOrEmpty(path) || !_byPath.TryGetValue(path, out List<T> list))
                return default;
            foreach (T t in list) {
                if (!string.Equals(getHash(item), getHash(t), StringComparison.Ordinal))
                    return t;
            }
            return default;
        }

        public bool HasSamePathDifferentHash(T item, Func<T, string> getPath, Func<T, string> getHash)
        {
            return FindSamePathDifferentHash(item, getPath, getHash) != null;
        }
    }

    internal sealed class ExistPictureIndex : IHashPathIndex<Picture>
    {
        private readonly HashPathIndex<Picture> _index;

        public ExistPictureIndex(IEnumerable<Picture> pictures)
        {
            _index = new HashPathIndex<Picture>(pictures, p => p.Path, p => p.Hash);
        }

        public bool HasSamePathAndHash(Picture picture) => _index.HasSamePathAndHash(picture, p => p.Path, p => p.Hash);
        public Picture FindSameHashDifferentPath(Picture picture) => _index.FindSameHashDifferentPath(picture, p => p.Path, p => p.Hash);
        public bool HasSameHashDifferentPath(Picture picture) => _index.HasSameHashDifferentPath(picture, p => p.Path, p => p.Hash);
        public Picture FindSamePathDifferentHash(Picture picture) => _index.FindSamePathDifferentHash(picture, p => p.Path, p => p.Hash);
        public bool HasSamePathDifferentHash(Picture picture) => _index.HasSamePathDifferentHash(picture, p => p.Path, p => p.Hash);
    }
}
