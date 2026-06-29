using Jvedio.Core.Library;
using SuperControls.Style;
using SuperUtils.IO;
using SuperUtils.Time;
using System;
using System.IO;
using static Jvedio.App;

namespace Jvedio.Entity
{
    public partial class Video
    {
        [Obsolete("Subscribe to LibraryEventBus.MetadataRefreshed")]
        public static Action onPlayVideo;

        public static void PlayVideoWithPlayer(string filepath, long dataID = 0)
        {
            if (File.Exists(filepath)) {
                bool success = false;
                if (!string.IsNullOrEmpty(ConfigManager.Settings.VideoPlayerPath) && File.Exists(ConfigManager.Settings.VideoPlayerPath)) {
                    success = FileHelper.TryOpenFile(ConfigManager.Settings.VideoPlayerPath, filepath);
                } else {
                    success = FileHelper.TryOpenFile(filepath);
                }

                if (success && dataID > 0) {
                    MapperManager.metaDataMapper.UpdateFieldById("ViewDate", DateHelper.Now(), dataID);
                    LibraryEventBus.RaiseMetadataRefreshed(dataID);
                    onPlayVideo?.Invoke();
                }
            } else {
                MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_OpenFail") + "：" + filepath);
            }
        }
    }
}
