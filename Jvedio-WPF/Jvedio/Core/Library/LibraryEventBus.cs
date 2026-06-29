using Jvedio.Core.Enums;
using Jvedio.Core.Net;
using Jvedio.Core.Scan;
using Jvedio.Core.FFmpeg;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using System;
using System.Collections.Generic;

namespace Jvedio.Core.Library
{
    public sealed class ScanCompletedEventArgs : EventArgs
    {
        public ScanCompletedEventArgs(ScanJobBase scanJob, DataType dataType)
        {
            ScanJob = scanJob;
            DataType = dataType;
        }

        public ScanJobBase ScanJob { get; }
        public DataType DataType { get; }
    }

    /// <summary>第三参语义为 deleted（历史命名 Added 保留兼容）。</summary>
    public sealed class TagStampChangedEventArgs : EventArgs
    {
        public TagStampChangedEventArgs(long dataId, long tagId, bool deleted)
        {
            DataId = dataId;
            TagId = tagId;
            Deleted = deleted;
        }

        public long DataId { get; }
        public long TagId { get; }
        public bool Deleted { get; }
        [Obsolete("Use Deleted; legacy name kept for callers.")]
        public bool Added => Deleted;
    }

    public sealed class VideosDeletedEventArgs : EventArgs
    {
        public VideosDeletedEventArgs(IReadOnlyList<long> dataIds, IReadOnlyList<Video> videos)
        {
            DataIds = dataIds;
            Videos = videos;
        }

        public IReadOnlyList<long> DataIds { get; }
        public IReadOnlyList<Video> Videos { get; }
    }

    public sealed class MetadataRefreshedEventArgs : EventArgs
    {
        public MetadataRefreshedEventArgs(long dataId = 0)
        {
            DataId = dataId;
        }

        /// <summary>0 表示全库统计刷新。</summary>
        public long DataId { get; }
    }

    public sealed class DownloadCompletedEventArgs : EventArgs
    {
        public DownloadCompletedEventArgs(DownLoadTask task)
        {
            Task = task;
        }

        public DownLoadTask Task { get; }
    }

    public sealed class DownloadPreviewEventArgs : EventArgs
    {
        public DownloadPreviewEventArgs(long dataId, string path, byte[] fileByte)
        {
            DataId = dataId;
            Path = path;
            FileByte = fileByte;
        }

        public long DataId { get; }
        public string Path { get; }
        public byte[] FileByte { get; }
    }

    public sealed class ScreenShotCompletedEventArgs : EventArgs
    {
        public ScreenShotCompletedEventArgs(bool success, long dataId)
        {
            Success = success;
            DataId = dataId;
        }

        public bool Success { get; }
        public long DataId { get; }
    }

    public sealed class ScreenShotErrorEventArgs : EventArgs
    {
        public ScreenShotErrorEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public sealed class TagStampFilterEventArgs : EventArgs
    {
        public TagStampFilterEventArgs(long tagId)
        {
            TagId = tagId;
        }

        public long TagId { get; }
    }

    public sealed class ActorInfoChangedEventArgs : EventArgs
    {
        public ActorInfoChangedEventArgs(long actorId)
        {
            ActorId = actorId;
        }

        public long ActorId { get; }
    }

    public sealed class TabFocusEventArgs : EventArgs
    {
        public TabFocusEventArgs(TabItemEx tabItem)
        {
            TabItem = tabItem;
        }

        public TabItemEx TabItem { get; }
    }

    public sealed class SearchingChangedEventArgs : EventArgs
    {
        public SearchingChangedEventArgs(bool isSearching)
        {
            IsSearching = isSearching;
        }

        public bool IsSearching { get; }
    }

    public sealed class WaitingChangedEventArgs : EventArgs
    {
        public WaitingChangedEventArgs(string message, bool visible)
        {
            Message = message;
            Visible = visible;
        }

        public string Message { get; }
        public bool Visible { get; }
    }

    public static class LibraryEventBus
    {
        public static event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        public static event EventHandler<MetadataRefreshedEventArgs> MetadataRefreshed;
        public static event EventHandler<VideosDeletedEventArgs> VideosDeleted;
        public static event EventHandler<TagStampChangedEventArgs> TagStampChanged;
        public static event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
        public static event EventHandler<DownloadPreviewEventArgs> DownloadPreview;
        public static event EventHandler<ScreenShotCompletedEventArgs> ScreenShotCompleted;
        public static event EventHandler<ScreenShotErrorEventArgs> ScreenShotError;
        public static event EventHandler<TagStampFilterEventArgs> TagStampDeleted;
        public static event EventHandler<TagStampFilterEventArgs> TagStampFilterRefresh;
        public static event EventHandler TagStampPanelRefresh;
        public static event EventHandler<ActorInfoChangedEventArgs> ActorInfoChanged;
        public static event EventHandler<TabFocusEventArgs> TabFocusChanged;
        public static event EventHandler<SearchingChangedEventArgs> SearchingChanged;
        public static event EventHandler<WaitingChangedEventArgs> WaitingChanged;

        public static void RaiseScanCompleted(ScanJobBase scanJob, DataType dataType)
        {
            ScanCompleted?.Invoke(null, new ScanCompletedEventArgs(scanJob, dataType));
        }

        public static void RaiseMetadataRefreshed(long dataId = 0)
        {
            MetadataRefreshed?.Invoke(null, new MetadataRefreshedEventArgs(dataId));
        }

        public static void RaiseVideosDeleted(IReadOnlyList<long> dataIds, IReadOnlyList<Video> videos)
        {
            if (dataIds == null || dataIds.Count == 0)
                return;
            VideosDeleted?.Invoke(null, new VideosDeletedEventArgs(dataIds, videos));
        }

        public static void RaiseTagStampChanged(long dataId, long tagId, bool deleted)
        {
            TagStampChanged?.Invoke(null, new TagStampChangedEventArgs(dataId, tagId, deleted));
        }

        public static void RaiseDownloadCompleted(DownLoadTask task)
        {
            if (task == null)
                return;
            DownloadCompleted?.Invoke(null, new DownloadCompletedEventArgs(task));
        }

        public static void RaiseDownloadPreview(long dataId, string path, byte[] fileByte)
        {
            DownloadPreview?.Invoke(null, new DownloadPreviewEventArgs(dataId, path, fileByte));
        }

        public static void RaiseScreenShotCompleted(bool success, long dataId)
        {
            ScreenShotCompleted?.Invoke(null, new ScreenShotCompletedEventArgs(success, dataId));
        }

        public static void RaiseScreenShotError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            ScreenShotError?.Invoke(null, new ScreenShotErrorEventArgs(message));
        }

        public static void RaiseTagStampDeleted(long tagId)
        {
            TagStampDeleted?.Invoke(null, new TagStampFilterEventArgs(tagId));
        }

        public static void RaiseTagStampFilterRefresh(long tagId)
        {
            TagStampFilterRefresh?.Invoke(null, new TagStampFilterEventArgs(tagId));
        }

        public static void RaiseTagStampPanelRefresh()
        {
            TagStampPanelRefresh?.Invoke(null, EventArgs.Empty);
        }

        public static void RaiseActorInfoChanged(long actorId)
        {
            if (actorId <= 0)
                return;
            ActorInfoChanged?.Invoke(null, new ActorInfoChangedEventArgs(actorId));
        }

        public static void RaiseTabFocusChanged(TabItemEx tabItem)
        {
            if (tabItem == null)
                return;
            TabFocusChanged?.Invoke(null, new TabFocusEventArgs(tabItem));
        }

        public static void RaiseSearchingChanged(bool isSearching)
        {
            SearchingChanged?.Invoke(null, new SearchingChangedEventArgs(isSearching));
        }

        public static void RaiseWaitingChanged(string message, bool visible)
        {
            WaitingChanged?.Invoke(null, new WaitingChangedEventArgs(message, visible));
        }
    }
}
