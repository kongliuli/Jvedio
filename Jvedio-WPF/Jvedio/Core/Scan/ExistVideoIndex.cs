using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jvedio.Core.Scan
{
    internal sealed class ExistVideoIndex
    {
        private readonly Dictionary<(string Path, long Size), List<Video>> _byPathSize =
            new Dictionary<(string Path, long Size), List<Video>>();

        private readonly Dictionary<string, List<Video>> _byVid =
            new Dictionary<string, List<Video>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, List<Video>> _byHash =
            new Dictionary<string, List<Video>>(StringComparer.Ordinal);

        public ExistVideoIndex(IEnumerable<Video> videos)
        {
            if (videos == null)
                return;
            foreach (Video video in videos) {
                if (video == null)
                    continue;
                AddToMap(_byPathSize, (video.Path ?? string.Empty, video.Size), video);
                if (!string.IsNullOrEmpty(video.VID))
                    AddToMap(_byVid, video.VID, video);
                if (!string.IsNullOrEmpty(video.Hash))
                    AddToMap(_byHash, video.Hash, video);
            }
        }

        private static void AddToMap<TKey>(Dictionary<TKey, List<Video>> map, TKey key, Video video)
        {
            if (!map.TryGetValue(key, out List<Video> list)) {
                list = new List<Video>();
                map[key] = list;
            }
            list.Add(video);
        }

        public bool HasSamePathAndSize(Video video)
        {
            return _byPathSize.TryGetValue((video.Path ?? string.Empty, video.Size), out List<Video> list) &&
                list.Count > 0;
        }

        public List<Video> FindSameVidDifferentPathSameSize(Video video)
        {
            return FilterByVid(video, (t) =>
                video.Size.Equals(t.Size) &&
                !string.Equals(video.Path, t.Path, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(t.Path) &&
                (string.IsNullOrEmpty(video.Hash) || string.Equals(video.Hash, t.Hash ?? string.Empty, StringComparison.Ordinal)));
        }

        public List<Video> FindSameVidDifferentPathDifferentSizeSameSubSection(Video video)
        {
            return FilterByVid(video, (t) =>
                !string.Equals(video.Path, t.Path, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(t.Path) &&
                !string.IsNullOrEmpty(video.Hash) &&
                string.Equals(video.Hash, t.Hash ?? string.Empty, StringComparison.Ordinal));
        }

        public Video FindForUpdateByVid(Video video)
        {
            if (string.IsNullOrEmpty(video.VID) || !_byVid.TryGetValue(video.VID, out List<Video> list))
                return null;
            foreach (Video t in list) {
                if (!string.Equals(video.Path, t.Path, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(video.SubSection ?? string.Empty, t.SubSection ?? string.Empty, StringComparison.Ordinal))
                    return t;
            }
            return null;
        }

        public bool HasAnyWithSameVid(Video video)
        {
            return !string.IsNullOrEmpty(video.VID) &&
                _byVid.TryGetValue(video.VID, out List<Video> list) &&
                list.Count > 0;
        }

        public Video FindByVid(string vid)
        {
            if (string.IsNullOrEmpty(vid) || !_byVid.TryGetValue(vid, out List<Video> list))
                return null;
            return list.Count > 0 ? list[0] : null;
        }

        public bool HasSameHashAndPath(Video video)
        {
            if (string.IsNullOrEmpty(video.Hash))
                return false;
            if (!_byHash.TryGetValue(video.Hash, out List<Video> list))
                return false;
            foreach (Video t in list) {
                if (string.Equals(video.Hash, t.Hash, StringComparison.Ordinal) &&
                    string.Equals(video.Path, t.Path, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public List<Video> FindSameHashSamePath(Video video)
        {
            return FilterByHash(video, (t) => string.Equals(video.Path, t.Path, StringComparison.OrdinalIgnoreCase));
        }

        public Video FindSameHashDifferentPath(Video video)
        {
            if (string.IsNullOrEmpty(video.Hash) || !_byHash.TryGetValue(video.Hash, out List<Video> list))
                return null;
            foreach (Video t in list) {
                if (!string.Equals(video.Path, t.Path, StringComparison.OrdinalIgnoreCase))
                    return t;
            }
            return null;
        }

        private List<Video> FilterByVid(Video video, Func<Video, bool> predicate)
        {
            List<Video> result = new List<Video>();
            if (string.IsNullOrEmpty(video.VID) || !_byVid.TryGetValue(video.VID, out List<Video> list))
                return result;
            foreach (Video t in list) {
                if (predicate(t))
                    result.Add(t);
            }
            return result;
        }

        private List<Video> FilterByHash(Video video, Func<Video, bool> predicate)
        {
            List<Video> result = new List<Video>();
            if (string.IsNullOrEmpty(video.Hash) || !_byHash.TryGetValue(video.Hash, out List<Video> list))
                return result;
            foreach (Video t in list) {
                if (predicate(t))
                    result.Add(t);
            }
            return result;
        }
    }
}
