using Jvedio.Core.Library;
using Jvedio.Entity;
using System;
using System.Collections.Generic;

namespace Jvedio.Core.UserControls
{
    /// <summary>VideoList 命令回调集中；发布走 LibraryEventBus，static Action 仅作 Obsolete 兼容。</summary>
    public static class VideoListCommands
    {
        [Obsolete("Subscribe to LibraryEventBus.MetadataRefreshed")]
        public static Action onStatistic;

        [Obsolete("Subscribe to LibraryEventBus.SearchingChanged")]
        public static Action<bool> onSearchingChange;

        [Obsolete("Subscribe to LibraryEventBus.TagStampChanged")]
        public static Action<long, long, bool> onTagStampChange;

        [Obsolete("Subscribe to LibraryEventBus.VideosDeleted")]
        public static Action<List<Video>> onDeleteID;

        [Obsolete("Subscribe to LibraryEventBus.WaitingChanged")]
        public static Action<string, bool> onWaiting;

        public static void NotifyTagStampChange(long dataId, long tagId, bool deleted)
        {
            LibraryEventBus.RaiseTagStampChanged(dataId, tagId, deleted);
            onTagStampChange?.Invoke(dataId, tagId, deleted);
        }

        public static void NotifyVideosDeleted(List<Video> videos)
        {
            if (videos == null || videos.Count == 0)
                return;
            List<long> ids = new List<long>();
            foreach (Video v in videos) {
                if (v != null && v.DataID > 0)
                    ids.Add(v.DataID);
            }
            LibraryEventBus.RaiseVideosDeleted(ids, videos);
            onDeleteID?.Invoke(videos);
        }

        public static void NotifyMetadataRefreshed(long dataId = 0)
        {
            LibraryEventBus.RaiseMetadataRefreshed(dataId);
            onStatistic?.Invoke();
        }

        public static void NotifySearchingChanged(bool isSearching)
        {
            LibraryEventBus.RaiseSearchingChanged(isSearching);
            onSearchingChange?.Invoke(isSearching);
        }

        public static void NotifyWaiting(string message, bool visible)
        {
            LibraryEventBus.RaiseWaitingChanged(message, visible);
            onWaiting?.Invoke(message, visible);
        }
    }
}
