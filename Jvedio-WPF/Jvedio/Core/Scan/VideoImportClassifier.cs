using Jvedio.Entity;
using SuperControls.Style;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jvedio.Core.Scan
{
    internal interface IVideoImportRule
    {
        void Apply(List<Video> vidList, List<Video> noVidList, ExistVideoIndex index, ImportClassification<Video> result);
    }

    internal static class VideoImportClassifier
    {
        private static readonly List<IVideoImportRule> Rules = new List<IVideoImportRule>
        {
            new SamePathAndSizeRule(),
            new SameVidDifferentPathSameSizeRule(),
            new SameVidSameHashDuplicateRule(),
            new UpdateByVidRule(),
            new NoVidHashRules(),
        };

        internal static void RegisterRule(IVideoImportRule rule)
        {
            if (rule != null)
                Rules.Add(rule);
        }

        public static ImportClassification<Video> Classify(List<Video> import, ExistVideoIndex index)
        {
            ImportClassification<Video> result = new ImportClassification<Video>();
            if (import == null || import.Count == 0 || index == null)
                return result;

            List<Video> noVidList = import.Where(arg => string.IsNullOrEmpty(arg.VID)).ToList();
            List<Video> vidList = import.Where(arg => !string.IsNullOrEmpty(arg.VID)).ToList();

            foreach (IVideoImportRule rule in Rules)
                rule.Apply(vidList, noVidList, index, result);

            result.ToInsert.AddRange(vidList);
            result.ToInsert.AddRange(noVidList);
            return result;
        }

        internal static ScanDetailInfo BuildDuplicateDetail(string reason, string itemPath, IEnumerable<Video> duplicates)
        {
            ScanDetailInfo scanDetailInfo = new ScanDetailInfo(reason);
            StringBuilder builder = new StringBuilder($"和 {itemPath} 存在重复：");
            builder.AppendLine();
            builder.AppendLine();
            foreach (Video video in duplicates)
                builder.AppendLine(video.Path);
            scanDetailInfo.Detail = builder.ToString();
            return scanDetailInfo;
        }

        private sealed class SamePathAndSizeRule : IVideoImportRule
        {
            public void Apply(List<Video> vidList, List<Video> noVidList, ExistVideoIndex index, ImportClassification<Video> result)
            {
                foreach (Video item in vidList.Where(arg => index.HasSamePathAndSize(arg)).ToList())
                    result.NotImport[item.Path] = new ScanDetailInfo(LangManager.GetValueByKey("SamePathFileSize"));
                vidList.RemoveAll(arg => index.HasSamePathAndSize(arg));
            }
        }

        private sealed class SameVidDifferentPathSameSizeRule : IVideoImportRule
        {
            public void Apply(List<Video> vidList, List<Video> noVidList, ExistVideoIndex index, ImportClassification<Video> result)
            {
                foreach (Video item in vidList.ToList()) {
                    List<Video> duplicates = index.FindSameVidDifferentPathSameSize(item);
                    if (duplicates.Count == 0)
                        continue;
                    result.NotImport[item.Path] = BuildDuplicateDetail(
                        LangManager.GetValueByKey("NotSamePathSameFileSize"), item.Path, duplicates);
                }
                vidList.RemoveAll(arg => index.FindSameVidDifferentPathSameSize(arg).Count > 0);
            }
        }

        private sealed class SameVidSameHashDuplicateRule : IVideoImportRule
        {
            public void Apply(List<Video> vidList, List<Video> noVidList, ExistVideoIndex index, ImportClassification<Video> result)
            {
                foreach (Video item in vidList.ToList()) {
                    List<Video> duplicates = index.FindSameVidDifferentPathDifferentSizeSameSubSection(item);
                    if (duplicates.Count == 0)
                        continue;
                    result.NotImport[item.Path] = BuildDuplicateDetail(
                        LangManager.GetValueByKey("SamePathNotSameFileSize"), item.Path, duplicates);
                }
                vidList.RemoveAll(arg => index.FindSameVidDifferentPathDifferentSizeSameSubSection(arg).Count > 0);
            }
        }

        private sealed class UpdateByVidRule : IVideoImportRule
        {
            public void Apply(List<Video> vidList, List<Video> noVidList, ExistVideoIndex index, ImportClassification<Video> result)
            {
                foreach (Video video in vidList.ToList()) {
                    Video existVideo = index.FindForUpdateByVid(video);
                    if (existVideo == null)
                        continue;
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID;
                    video.LastScanDate = SuperUtils.Time.DateHelper.Now();
                    video.PathExist = 1;
                    result.ToUpdate.Add(video);
                    result.UpdateReasons[video.Path] = string.Empty;
                }
                vidList.RemoveAll(arg => index.HasAnyWithSameVid(arg));
            }
        }

        private sealed class NoVidHashRules : IVideoImportRule
        {
            public void Apply(List<Video> vidList, List<Video> noVidList, ExistVideoIndex index, ImportClassification<Video> result)
            {
                foreach (Video item in noVidList.Where(arg => index.HasSameHashAndPath(arg)).ToList()) {
                    result.NotImport[item.Path] = BuildDuplicateDetail(
                        LangManager.GetValueByKey("SameHashSamePath"), item.Path, index.FindSameHashSamePath(item));
                }
                noVidList.RemoveAll(arg => index.HasSameHashAndPath(arg));

                foreach (Video video in noVidList.ToList()) {
                    Video existVideo = index.FindSameHashDifferentPath(video);
                    if (existVideo == null)
                        continue;
                    video.DataID = existVideo.DataID;
                    video.MVID = existVideo.MVID;
                    video.LastScanDate = SuperUtils.Time.DateHelper.Now();
                    video.PathExist = 1;
                    result.ToUpdate.Add(video);
                    result.UpdateReasons[video.Path] = LangManager.GetValueByKey("SameHashNotSamePath");
                }
                noVidList.RemoveAll(arg => index.FindSameHashDifferentPath(arg) != null);
            }
        }
    }
}
