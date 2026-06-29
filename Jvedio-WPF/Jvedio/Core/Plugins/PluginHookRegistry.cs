using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using System;
using System.Collections.Generic;

namespace Jvedio.Core.Plugins
{
    public static class PluginHookRegistry
    {
        public static event Action<ScanJobBase, DataType> PostScanCompleted;
        public static event Action<Video, Dictionary<string, object>> PostMetadataMerged;

        public static void InvokePostScan(ScanJobBase job, DataType dataType)
        {
            PostScanCompleted?.Invoke(job, dataType);
        }

        public static void InvokePostMetadata(Video video, Dictionary<string, object> merged)
        {
            if (video == null || merged == null || merged.Count == 0)
                return;
            PostMetadataMerged?.Invoke(video, merged);
        }
    }
}
