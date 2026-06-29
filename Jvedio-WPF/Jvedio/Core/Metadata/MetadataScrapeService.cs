using Jvedio.Core.Config.Interfaces;
using Jvedio.Core.Net;
using Jvedio.Core.Tasks;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.CustomEventArgs;
using System.Collections.Generic;
using System.Linq;
using static Jvedio.App;

namespace Jvedio.Core.Metadata
{
    public static class MetadataScrapeService
    {
        private static IScanSettings ScanSettings => ConfigScanSettings.Instance;

        public static int Enqueue(IEnumerable<Video> videos, bool? scrapeOnly = null)
        {
            if (videos == null)
                return 0;

            bool metadataOnly = scrapeOnly ?? ShouldScrapeOnly();
            int added = 0;
            foreach (Video video in videos) {
                if (video == null || video.DataID <= 0)
                    continue;

                DownLoadTask task = CreateTask(video, metadataOnly);
                if (App.DownloadManager.Exists(task))
                    continue;

                task.onError += (s, ev) => MessageCard.Error((ev as MessageCallBackEventArgs).Message);
                App.TaskHub.AddTask(task, TaskKind.Scrape);
                added++;
            }

            return added;
        }

        /// <summary>扫描导入后自动入队：缺 metadata 且有 VID 的新影片。</summary>
        public static int EnqueueAfterScan(IEnumerable<Video> imported)
        {
            if (imported == null || !ScanSettings.DownloadInfo)
                return 0;

            List<Video> candidates = imported
                .Where(NeedsScrapeAfterScan)
                .ToList();
            return Enqueue(candidates);
        }

        public static bool NeedsScrapeAfterScan(Video video)
        {
            return video != null
                && video.DataID > 0
                && !string.IsNullOrEmpty(video.VID)
                && Video.NeedsMetadataDownload(video.Title, video.WebUrl, video.ImageUrls);
        }

        public static int Enqueue(Video video, bool? scrapeOnly = null)
        {
            return Enqueue(new[] { video }, scrapeOnly);
        }

        public static bool ShouldScrapeOnly()
        {
            var cfg = ConfigManager.DownloadConfig;
            return ShouldScrapeOnly(cfg.DownloadPoster, cfg.DownloadThumbNail, cfg.DownloadPreviewImage);
        }

        public static bool ShouldScrapeOnly(bool downloadPoster, bool downloadThumbNail, bool downloadPreviewImage)
        {
            return !downloadPoster && !downloadThumbNail && !downloadPreviewImage;
        }

        public static DownLoadTask CreateTask(Video video, bool scrapeOnly)
        {
            DownLoadTask task = new DownLoadTask(
                video,
                downloadPreview: !scrapeOnly && ConfigManager.DownloadConfig.DownloadPreviewImage,
                ConfigManager.DownloadConfig.OverrideInfo) {
                ScrapeOnly = scrapeOnly,
            };
            if (scrapeOnly) {
                string label = LangManager.GetValueByKey("RefreshMetadata");
                task.Title = string.IsNullOrEmpty(task.Title) ? label : $"{label} {task.Title}";
            }
            return task;
        }
    }
}
