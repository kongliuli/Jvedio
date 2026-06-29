using Jvedio.Entity;
using Jvedio.Core.Config;
using SuperUtils.Common;
using SuperUtils.IO;
using System.IO;

namespace Jvedio.Core.Metadata
{
    public interface IMetadataSaver
    {
        void SaveNfo(Video video);
    }

    public static class MetadataSaver
    {
        public static IMetadataSaver Default { get; } = new NfoMetadataSaver();
    }

    internal sealed class NfoMetadataSaver : IMetadataSaver
    {
        public void SaveNfo(Video video)
        {
            if (video == null || !ConfigManager.Settings.SaveInfoToNFO)
                return;

            string dir = ConfigManager.Settings.NFOSavePath;
            bool overrideInfo = ConfigManager.DownloadConfig.OverrideInfo;

            string saveName = $"{video.VID.ToProperFileName()}.nfo";
            if (string.IsNullOrEmpty(video.VID))
                saveName = $"{Path.GetFileNameWithoutExtension(video.Path)}.nfo";

            string saveFileName = string.Empty;
            if (Directory.Exists(dir))
                saveFileName = Path.Combine(dir, saveName);

            if (!Directory.Exists(dir) && File.Exists(video.Path))
                saveFileName = Path.Combine(new FileInfo(video.Path).DirectoryName, saveName);

            if (string.IsNullOrEmpty(saveFileName))
                return;
            if (overrideInfo || !File.Exists(saveFileName))
                NfoMetadataWriter.WriteVideo(video, saveFileName);
        }
    }
}
